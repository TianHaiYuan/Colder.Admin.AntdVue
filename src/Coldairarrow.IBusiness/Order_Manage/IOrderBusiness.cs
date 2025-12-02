using Coldairarrow.Entity.Order_Manage;
using Coldairarrow.Entity.Product_Manage;
using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coldairarrow.IBusiness.Order_Manage
{
    public interface IOrderBusiness
    {
        Task<PageResult<OrderDTO>> GetDataListAsync(PageInput<OrderQueryDTO> input);
        Task<OrderDTO> GetTheDataAsync(string id);
        Task AddDataAsync(OrderEditDTO data);
        Task UpdateDataAsync(OrderEditDTO data);
        Task DeleteDataAsync(List<string> ids);
        Task UpdateStatusAsync(string id, OrderStatus status);
        Task UpdatePaymentStatusAsync(string id, PaymentStatus paymentStatus);
    }

    public class OrderQueryDTO
    {
        public string Keyword { get; set; }
        public OrderStatus? Status { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    [Map(typeof(Order))]
    public class OrderDTO : Order
    {
        public string StatusText { get => Status.GetDescription(); }
        public string PaymentStatusText { get => PaymentStatus.GetDescription(); }
        public List<OrderDetail> Details { get; set; }
    }

    [Map(typeof(Order))]
    public class OrderEditDTO : Order
    {
        public List<OrderDetail> Details { get; set; }
    }
}

