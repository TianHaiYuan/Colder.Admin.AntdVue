using Microsoft.AspNetCore.SignalR;
using NotificationCenter.Api.Hubs;
using NotificationCenter.Api.Infrastructure;
using EFCore.Sharding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection("Notification"));
builder.Services.Configure<DingTalkOptions>(builder.Configuration.GetSection("DingTalk"));

// 使用 Util 中封装的 EFCore.Sharding 作为 NotificationCenter 的数据访问层
builder.Services.AddEFCoreSharding(config =>
{
    config.SetEntityAssemblies(typeof(NotificationRecord).Assembly);

    var connString = builder.Configuration.GetConnectionString("NotificationDb");
    if (string.IsNullOrWhiteSpace(connString))
        throw new InvalidOperationException("Connection string 'NotificationDb' is not configured.");

    config.UseDatabase(connString, DatabaseType.SqlServer);
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, QueryStringUserIdProvider>();
builder.Services.AddSingleton<DingTalkNotifier>();
builder.Services.AddSingleton<NotificationStore>();
builder.Services.AddHostedService<RabbitMqListener>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

app.UseCors();

// SignalR Hub
app.MapHub<NotificationHub>("/hubs/notification");

// 简单的消息查询接口：
// GET  /api/notifications?userId=xxx&box=received|sent&page=1&pageSize=20
// GET  /api/notifications/unread-count?userId=xxx
// POST /api/notifications/{id}/read?userId=xxx
app.MapGet("/api/notifications", async (string userId, string? box, int page, int pageSize, NotificationStore store) =>
{
    if (string.IsNullOrWhiteSpace(userId))
        return Results.BadRequest(new { message = "userId 不能为空" });

    var normalizedBox = string.Equals(box, "sent", StringComparison.OrdinalIgnoreCase)
        ? "sent"
        : "received";
    var (items, total) = await store.GetListAsync(userId, normalizedBox, page, pageSize);
    return Results.Ok(new { items, total });
});

app.MapGet("/api/notifications/unread-count", async (string userId, NotificationStore store) =>
{
    if (string.IsNullOrWhiteSpace(userId))
        return Results.BadRequest(new { message = "userId 不能为空" });

    var count = await store.GetUnreadCountAsync(userId);
    return Results.Ok(new { count });
});

app.MapPost("/api/notifications/{id:long}/read", async (long id, string userId, NotificationStore store) =>
{
    if (id <= 0 || string.IsNullOrWhiteSpace(userId))
        return Results.BadRequest(new { message = "参数不合法" });

    await store.MarkAsReadAsync(id, userId);
    return Results.Ok();
});

app.Run();

