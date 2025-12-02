using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coldairarrow.Entity.Order_Manage
{
    /// <summary>
    /// 订单表
    /// </summary>
    [Table("Order")]
    public class Order : BaseEntity
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        [MaxLength(50)]
        public string OrderNo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [MaxLength(100)]
        public string CustomerName { get; set; }

        /// <summary>
        /// 客户电话
        /// </summary>
        [MaxLength(20)]
        public string CustomerPhone { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        [MaxLength(500)]
        public string Address { get; set; }

        /// <summary>
        /// 订单总金额
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// 支付状态
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PaymentTime { get; set; }

        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? ShippingTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        [MaxLength(500)]
        public string Remark { get; set; }
    }

    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatus
    {
        [Description("待确认")]
        Pending = 0,

        [Description("已确认")]
        Confirmed = 1,

        [Description("已发货")]
        Shipped = 2,

        [Description("已完成")]
        Completed = 3,

        [Description("已取消")]
        Cancelled = 4
    }

    /// <summary>
    /// 支付状态
    /// </summary>
    public enum PaymentStatus
    {
        [Description("未支付")]
        Unpaid = 0,

        [Description("已支付")]
        Paid = 1,

        [Description("已退款")]
        Refunded = 2
    }
}

