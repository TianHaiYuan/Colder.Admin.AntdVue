using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NotificationCenter.Api.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationCenter.Api.Infrastructure;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "127.0.0.1";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "approval.events";
    public string Queue { get; set; } = "approval.notification";
}

public class RabbitMqListener : BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly RabbitMqOptions _options;
    private readonly NotificationOptions _notificationOptions;
    private readonly DingTalkNotifier _dingTalkNotifier;
    private readonly NotificationStore _notificationStore;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqListener(
        IHubContext<NotificationHub> hubContext,
        IOptions<RabbitMqOptions> options,
        IOptions<NotificationOptions> notificationOptions,
        DingTalkNotifier dingTalkNotifier,
        NotificationStore notificationStore)
    {
        _hubContext = hubContext;
        _options = options.Value;
        _notificationOptions = notificationOptions.Value;
        _dingTalkNotifier = dingTalkNotifier;
        _notificationStore = notificationStore;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 辅助方法：兼容 PascalCase 和 camelCase 属性名，比如 EventType / eventType
        static string? GetStringProperty(JsonElement element, string pascalName)
        {
            if (element.TryGetProperty(pascalName, out var prop) && prop.ValueKind == JsonValueKind.String)
                return prop.GetString();

            var camelName = char.ToLowerInvariant(pascalName[0]) + pascalName[1..];
            if (element.TryGetProperty(camelName, out var camelProp) && camelProp.ValueKind == JsonValueKind.String)
                return camelProp.GetString();

            return null;
        }

        static string BuildContent(
            string eventType,
            string? title,
            string? stepName,
            string? businessType,
            string? status)
        {
            var businessDisplay = businessType == "Product"
                ? "产品审批"
                : string.IsNullOrWhiteSpace(businessType) ? "单据" : businessType;
            if (string.IsNullOrWhiteSpace(eventType))
            {
                return title ?? businessDisplay;
            }
            return eventType switch
            {
                "approval.step.pending" => $"{businessDisplay}：{title ?? "-"}{(string.IsNullOrWhiteSpace(stepName) ? string.Empty : $"，步骤：{stepName}")}",
                "approval.step.approved" => $"{businessDisplay}：{title ?? "-"}{(string.IsNullOrWhiteSpace(stepName) ? string.Empty : $" 的步骤\"{stepName}\"已通过")}",
                "approval.step.rejected" => $"{businessDisplay}：{title ?? "-"}{(string.IsNullOrWhiteSpace(stepName) ? string.Empty : $" 的步骤\"{stepName}\"已被驳回")}",
                "approval.completed" => $"{businessDisplay}：{title ?? "-"} 已完成，状态：{(string.IsNullOrWhiteSpace(status) ? "已结束" : status)}",
                _ => $"{businessDisplay}：{title ?? "-"}（事件：{eventType}）"
            };
        }

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(_options.Queue, durable: true, exclusive: false, autoDelete: false);
        // 之前使用 "approval.*"，只能收到类似 "approval.xxx" 的一段路由
        // 像 "approval.step.pending" 这种多段路由不会匹配，导致收不到待审批事件
        // 改为 "approval.#"，表示匹配 approval. 开头的所有事件
        _channel.QueueBind(_options.Queue, _options.Exchange, routingKey: "approval.#");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.Span);
            var doc = JsonDocument.Parse(json).RootElement;
            var eventType = GetStringProperty(doc, "EventType");
            if (string.IsNullOrWhiteSpace(eventType))
            {
                // 没有 EventType，无意义消息，直接确认丢弃
                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                return;
            }

            var title = GetStringProperty(doc, "Title");
            var stepName = GetStringProperty(doc, "StepName");
            var businessType = GetStringProperty(doc, "BusinessType");
            var businessId = GetStringProperty(doc, "BusinessId");
            var status = GetStringProperty(doc, "Status");
            var initiatorUserId = GetStringProperty(doc, "InitiatorUserId");
            var initiatorUserName = GetStringProperty(doc, "InitiatorUserName");
            var approverUserId = GetStringProperty(doc, "ApproverUserId");
            var approverUserName = GetStringProperty(doc, "ApproverUserName");

            // SignalR 通知：
            //  - approval.step.pending  通知当前步骤所有审批人 (ApproverUserIds)
            //  - approval.completed / approval.step.approved / approval.step.rejected  通知流程发起人 (InitiatorUserId)
            if (_notificationOptions.EnableSignalR &&
                    (_notificationOptions.SignalREventTypes == null || _notificationOptions.SignalREventTypes.Contains(eventType)))
            {
                var recipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // 配置了 ApproverUserIds 的事件（例如 approval.step.pending），通知这些审批人
                var approverUserIdsRaw = GetStringProperty(doc, "ApproverUserIds") ?? string.Empty;
                var approverUserIds = approverUserIdsRaw
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var userId in approverUserIds)
                {
                    recipients.Add(userId);
                }

                // 审批结果类事件，给发起人发一条结果通知
                if (!string.IsNullOrWhiteSpace(initiatorUserId) &&
                        (eventType == "approval.completed" ||
                         eventType == "approval.step.approved" ||
                         eventType == "approval.step.rejected"))
                {
                    recipients.Add(initiatorUserId);
                }

                // 计算消息发送人（谁触发了这次审批事件）
                string? senderUserId = null;
                string? senderUserName = null;
                switch (eventType)
                {
                    case "approval.step.pending":
                        // 发起人把审批单流转给当前步骤审批人
                        senderUserId = initiatorUserId;
                        senderUserName = initiatorUserName;
                        break;
                    case "approval.step.approved":
                    case "approval.step.rejected":
                        // 当前步骤审批人操作
                        senderUserId = approverUserId;
                        senderUserName = approverUserName;
                        break;
                    case "approval.completed":
                        // 最后一步通过或驳回，优先认为是最后审批人，否则退回到发起人
                        senderUserId = !string.IsNullOrWhiteSpace(approverUserId) ? approverUserId : initiatorUserId;
                        senderUserName = !string.IsNullOrWhiteSpace(approverUserName) ? approverUserName : initiatorUserName;
                        break;
                    default:
                        senderUserId = initiatorUserId;
                        senderUserName = initiatorUserName;
                        break;
                }

                var contentText = BuildContent(eventType!, title, stepName, businessType, status);

                foreach (var userId in recipients)
                {
                    // 收件人是自己时，不再给自己发消息
                    if (!string.IsNullOrWhiteSpace(senderUserId) &&
                            string.Equals(senderUserId, userId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    await _hubContext.Clients.User(userId).SendAsync("ApprovalEvent", json);

                    // 同步持久化一条消息记录
                    var record = new NotificationRecord
                    {
                        UserId = userId,
                        SenderUserId = senderUserId,
                        SenderUserName = senderUserName,
                        EventType = eventType!,
                        BusinessType = businessType,
                        BusinessId = businessId,
                        Title = title,
                        StepName = stepName,
                        Status = status,
                        Content = contentText,
                        CreatedTime = DateTime.UtcNow
                    };

                    await _notificationStore.SaveAsync(record);
                }
            }

            // 钉钉机器人通知（可选）
            if (_notificationOptions.EnableDingTalk &&
                (_notificationOptions.DingTalkEventTypes == null || _notificationOptions.DingTalkEventTypes.Contains(eventType)))
            {
                var content = eventType switch
                {
                    "approval.step.pending" => $"您有新的审批待处理：{title}，步骤：{stepName}",
                    "approval.step.approved" => $"审批步骤已通过：{title}，步骤：{stepName}",
                    "approval.step.rejected" => $"审批步骤已被驳回：{title}，步骤：{stepName}",
                    "approval.completed" => $"审批流程已结束：{title}",
                    _ => $"审批事件：{eventType}，单据：{title}"
                };

                await _dingTalkNotifier.SendAsync(content);
            }

            _channel!.BasicAck(ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(_options.Queue, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
