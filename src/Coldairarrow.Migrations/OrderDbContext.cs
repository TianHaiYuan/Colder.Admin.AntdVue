using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Coldairarrow.Migrations
{
    /// <summary>
    /// 订单库 DbContext（用于 EF Core 迁移）
    /// </summary>
    public class OrderDbContext : DbContext
    {
        private static readonly Assembly EntityAssembly = Assembly.Load("Coldairarrow.Entity");

        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 只注册 Order_Manage 命名空间下的实体
            var entityTypes = EntityAssembly.GetTypes()
                .Where(t => t.IsClass
                    && !t.IsAbstract
                    && t.GetCustomAttribute<TableAttribute>(inherit: false) != null
                    && !t.Name.EndsWith("DTO")
                    && t.Namespace == "Coldairarrow.Entity.Order_Manage");

            foreach (var entityType in entityTypes)
            {
                if (modelBuilder.Model.FindEntityType(entityType) == null)
                    modelBuilder.Entity(entityType);
            }
        }
    }
}
