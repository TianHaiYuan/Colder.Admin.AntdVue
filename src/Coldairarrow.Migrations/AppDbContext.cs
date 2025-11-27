using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Coldairarrow.Migrations
{
    /// <summary>
    /// 应用程序数据库上下文 - 用于 EF Core 迁移
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// 实体程序集（Coldairarrow.Entity）
        /// </summary>
        private static readonly Assembly EntityAssembly = Assembly.Load("Coldairarrow.Entity");

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 自动扫描并注册所有带有 [Table] 特性的实体类
            // 注意：使用 GetCustomAttribute(inherit: false) 只获取直接标注的特性，排除继承的
            var entityTypes = EntityAssembly.GetTypes()
                .Where(t => t.IsClass
                    && !t.IsAbstract
                    && t.GetCustomAttribute<TableAttribute>(inherit: false) != null
                    && !t.Name.EndsWith("DTO"));  // 排除DTO类

            foreach (var entityType in entityTypes)
            {
                // 如果实体尚未注册，则注册它
                if (modelBuilder.Model.FindEntityType(entityType) == null)
                {
                    modelBuilder.Entity(entityType);
                }
            }

            // 可以在此处添加额外的 Fluent API 配置
            // 例如: modelBuilder.Entity<Base_User>().HasIndex(x => x.UserName).IsUnique();
        }
    }
}

