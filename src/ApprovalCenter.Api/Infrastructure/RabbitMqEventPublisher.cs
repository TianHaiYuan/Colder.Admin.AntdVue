using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ApprovalCenter.Api.Infrastructure;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "approval.events";
}

public interface IEventPublisher
{
    void Publish(string routingKey, object payload);
}

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqOptions _options;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        // 与 NotificationCenter.Api 的 RabbitMqListener 约定保持一致，使用 PascalCase 属性名
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null
    };

    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
    }

    public void Publish(string routingKey, object payload)
    {
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish(_options.Exchange, routingKey, basicProperties: null, body: body);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
