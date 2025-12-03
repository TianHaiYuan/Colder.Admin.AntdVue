using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using EFCore.Sharding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationCenter.Api.Infrastructure
{
    /// <summary>
    /// 通知消息实体，对应表：NotificationMessages
    /// </summary>
    [Table("NotificationMessages")]
    public class NotificationRecord
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 接收人 Id
        /// </summary>
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 发送人 Id
        /// </summary>
        [MaxLength(50)]
        public string? SenderUserId { get; set; }

        /// <summary>
        /// 发送人姓名
        /// </summary>
        [MaxLength(100)]
        public string? SenderUserName { get; set; }

        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? BusinessType { get; set; }

        [MaxLength(50)]
        public string? BusinessId { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(200)]
        public string? StepName { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(500)]
        public string? Content { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedTime { get; set; }
    }

    /// <summary>
    /// 消息持久化存储：完全依赖 EFCore.Sharding(IDbAccessor) 管理库表与读写。
    /// </summary>
    public class NotificationStore
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public NotificationStore(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// 保存一条通知记录
        /// </summary>
        public async Task SaveAsync(NotificationRecord record)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbAccessor>();

            if (record.CreatedTime == default)
                record.CreatedTime = DateTime.UtcNow;

            await db.InsertAsync(record);
        }

        /// <summary>
        /// 分页获取“我收到的 / 我发出的”通知列表
        /// </summary>
        public async Task<(IReadOnlyList<NotificationRecord> Items, int Total)> GetListAsync(
            string userId,
            string box,
            int pageIndex,
            int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;
            var offset = (pageIndex - 1) * pageSize;

            var isSentBox = string.Equals(box, "sent", StringComparison.OrdinalIgnoreCase);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbAccessor>();

            var query = db.GetIQueryable<NotificationRecord>();
            query = isSentBox
                ? query.Where(x => x.SenderUserId == userId)
                : query.Where(x => x.UserId == userId);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedTime)
                .ThenByDescending(x => x.Id)
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        /// <summary>
        /// 获取未读数量
        /// </summary>
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbAccessor>();

            return await db.GetIQueryable<NotificationRecord>()
                .CountAsync(x => x.UserId == userId && !x.IsRead);
        }

        /// <summary>
        /// 将指定消息标记为已读
        /// </summary>
        public async Task MarkAsReadAsync(long id, string userId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbAccessor>();

            var record = await db.GetIQueryable<NotificationRecord>()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (record == null)
                return;

            record.IsRead = true;
            await db.UpdateAsync(record);
        }
    }
}
