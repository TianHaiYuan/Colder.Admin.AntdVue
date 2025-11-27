using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coldairarrow.Entity
{
    /// <summary>
    /// 实体审计基类
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Key, Column(Order = 1)]
        [MaxLength(50)]
        public string Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        [MaxLength(50)]
        public string CreatorId { get; set; }

        /// <summary>
        /// 创建人姓名
        /// </summary>
        [MaxLength(100)]
        public string CreatorName { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? ModifyTime { get; set; }

        /// <summary>
        /// 修改人Id
        /// </summary>
        [MaxLength(50)]
        public string ModifierId { get; set; }

        /// <summary>
        /// 修改人姓名
        /// </summary>
        [MaxLength(100)]
        public string ModifierName { get; set; }

        /// <summary>
        /// 是否已删除
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// 租户Id
        /// </summary>
        [MaxLength(50)]
        public string TenantId { get; set; }
    }
}

