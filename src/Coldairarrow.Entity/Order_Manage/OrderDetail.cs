using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coldairarrow.Entity.Order_Manage
{
    /// <summary>
    /// 订单明细表
    /// </summary>
    [Table("OrderDetail")]
    public class OrderDetail : BaseEntity
    {
        /// <summary>
        /// 订单Id
        /// </summary>
        [MaxLength(50)]
        public string OrderId { get; set; }

        /// <summary>
        /// 产品Id
        /// </summary>
        [MaxLength(50)]
        public string ProductId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [MaxLength(200)]
        public string ProductName { get; set; }

        /// <summary>
        /// 产品单价
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 小计金额
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500)]
        public string Remark { get; set; }

	        /// <summary>
	        /// 产品图片地址（仅用于展示，不入库）
	        /// </summary>
	        [NotMapped]
	        public string ProductImageUrl { get; set; }
    }
}

