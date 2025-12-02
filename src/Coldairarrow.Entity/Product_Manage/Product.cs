using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coldairarrow.Entity.Product_Manage
{
    /// <summary>
    /// 产品表
    /// </summary>
    [Table("Product")]
    public class Product : BaseEntity
    {
        /// <summary>
        /// 产品编码
        /// </summary>
        [MaxLength(50)]
        public string ProductCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [MaxLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// 产品分类Id
        /// </summary>
        [MaxLength(50)]
        public string CategoryId { get; set; }

        /// <summary>
        /// 产品描述
        /// </summary>
        [MaxLength(2000)]
        public string Description { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CostPrice { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 产品图片
        /// </summary>
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        /// <summary>
        /// 产品状态
        /// </summary>
        public ProductStatus Status { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [MaxLength(20)]
        public string Unit { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int Sort { get; set; }
    }

    /// <summary>
    /// 产品状态
    /// </summary>
    public enum ProductStatus
    {
        [Description("草稿")]
        Draft = 0,

        [Description("上架")]
        OnSale = 1,

        [Description("下架")]
        OffSale = 2
    }
}

