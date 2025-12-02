using Coldairarrow.Entity.Order_Manage;
using Coldairarrow.IBusiness.Order_Manage;
using Coldairarrow.Util;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coldairarrow.Api.Controllers.Order_Manage
{
    /// <summary>
    /// 订单管理
    /// </summary>
    [Route("/Order_Manage/[controller]/[action]")]
    [OpenApiTag("订单管理")]
    public class OrderController : BaseApiController
    {
        #region DI

        public OrderController(IOrderBusiness orderBus)
        {
            _orderBus = orderBus;
        }

        IOrderBusiness _orderBus { get; }

        #endregion

        #region 获取

        [HttpPost]
        public async Task<PageResult<OrderDTO>> GetDataList(PageInput<OrderQueryDTO> input)
        {
            return await _orderBus.GetDataListAsync(input);
        }

        [HttpPost]
        public async Task<OrderDTO> GetTheData(IdInputDTO input)
        {
            return await _orderBus.GetTheDataAsync(input.id) ?? new OrderDTO();
        }

        #endregion

        #region 提交

        [HttpPost]
        public async Task SaveData(OrderEditDTO theData)
        {
            if (theData.Id.IsNullOrEmpty())
            {
                await _orderBus.AddDataAsync(theData);
            }
            else
            {
                await _orderBus.UpdateDataAsync(theData);
            }
        }

        [HttpPost]
        public async Task DeleteData(List<string> ids)
        {
            await _orderBus.DeleteDataAsync(ids);
        }

        [HttpPost]
        public async Task UpdateStatus(UpdateOrderStatusDTO input)
        {
            await _orderBus.UpdateStatusAsync(input.Id, input.Status);
        }

        [HttpPost]
        public async Task UpdatePaymentStatus(UpdatePaymentStatusDTO input)
        {
            await _orderBus.UpdatePaymentStatusAsync(input.Id, input.PaymentStatus);
        }

        #endregion
    }

    public class UpdateOrderStatusDTO
    {
        public string Id { get; set; }
        public OrderStatus Status { get; set; }
    }

    public class UpdatePaymentStatusDTO
    {
        public string Id { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}

