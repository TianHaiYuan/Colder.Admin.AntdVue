using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Coldairarrow.Migrations
{
    /// <summary>
    /// 设计时 DbContext 工厂 - 用于 EF Core 迁移命令
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            var databaseType = configuration["Database:BaseDb:DatabaseType"];
            var connectionString = configuration["Database:BaseDb:ConnectionString"];

            Console.WriteLine($"使用数据库类型: {databaseType}");
            Console.WriteLine($"连接字符串: {connectionString}");

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

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

