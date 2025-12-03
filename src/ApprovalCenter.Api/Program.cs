using ApprovalCenter.Api.Domain;
using ApprovalCenter.Api.Infrastructure;
using ApprovalCenter.Api.Services;
using EFCore.Sharding;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
// 使用 EFCore.Sharding 作为审批数据的统一数据访问层
builder.Services.AddEFCoreSharding(config =>
{
	// 当前审批中心的实体全部在本项目程序集内
	config.SetEntityAssemblies(typeof(WorkflowDefinition).Assembly);

	var connString = builder.Configuration.GetConnectionString("ApprovalDb");
	if (string.IsNullOrWhiteSpace(connString))
		throw new InvalidOperationException("Connection string 'ApprovalDb' is not configured.");

	// 审批中心目前使用 SQL Server 数据库
	config.UseDatabase(connString, DatabaseType.SqlServer);
});

// 仍然保留传统 DbContext, 仅用于启动时 EnsureCreated 创建表结构
builder.Services.AddDbContext<ApprovalDbContext>(options =>
		    options.UseSqlServer(builder.Configuration.GetConnectionString("ApprovalDb")));
builder.Services.AddScoped<ApprovalService>();
builder.Services.AddControllers()
	    .AddJsonOptions(options =>
	    {
	        // 与主系统 Coldairarrow.Api 保持一致，使用 PascalCase 属性名，方便前端直接用 "Status" 等字段
	        options.JsonSerializerOptions.PropertyNamingPolicy = null;
	        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
	    });
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
// 确保数据库存在并创建表结构（首次启动时自动创建 ApprovalCenterDb）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApprovalDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors();

app.MapControllers();

app.Run();

