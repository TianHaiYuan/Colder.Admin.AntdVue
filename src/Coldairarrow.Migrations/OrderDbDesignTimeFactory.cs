using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Coldairarrow.Migrations
{
    /// <summary>
    /// 订单库设计时 DbContext 工厂 - 用于 EF Core 迁移命令
    /// </summary>
    public class OrderDbDesignTimeFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();

            var databaseType = configuration["Database:OrderDb:DatabaseType"];
            var connectionString = configuration["Database:OrderDb:ConnectionString"];

            Console.WriteLine($"[OrderDb] 使用数据库类型: {databaseType}");
            Console.WriteLine($"[OrderDb] 连接字符串: {connectionString}");

            switch (databaseType?.ToLower())
            {
                case "sqlserver":
                    optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("Coldairarrow.Migrations"));
                    break;
                case "postgresgl":
                    optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Coldairarrow.Migrations"));
                    break;
                case "mysql":
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                        b => b.MigrationsAssembly("Coldairarrow.Migrations"));
                    break;
                default:
                    throw new Exception($"不支持的数据库类型: {databaseType}");
            }

            return new OrderDbContext(optionsBuilder.Options);
        }
    }
}
