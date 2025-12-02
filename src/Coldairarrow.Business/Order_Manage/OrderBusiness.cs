using Coldairarrow.Entity.Order_Manage;
using Coldairarrow.Entity.Product_Manage;
using Coldairarrow.IBusiness;
using Coldairarrow.IBusiness.Order_Manage;
using Coldairarrow.IBusiness.Product_Manage;
using Coldairarrow.Util;
using EFCore.Sharding;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coldairarrow.Business.Order_Manage
{
    public class OrderBusiness : BaseBusiness<Order>, IOrderBusiness, ITransientDependency
    {
        private readonly IProductDbAccessor _productDb;

        public OrderBusiness(IOrderDbAccessor db, IProductDbAccessor productDb, IOperator @operator = null)
            : base(db, @operator)
        {
            _productDb = productDb;
        }

        #region 外部接口

        public async Task<PageResult<OrderDTO>> GetDataListAsync(PageInput<OrderQueryDTO> input)
        {
            var search = input.Search;
            var q = GetIQueryable()
                .WhereIf(!search.Keyword.IsNullOrEmpty(), x => x.OrderNo.Contains(search.Keyword) || x.CustomerName.Contains(search.Keyword))
                .WhereIf(search.Status.HasValue, x => x.Status == search.Status)
                .WhereIf(search.PaymentStatus.HasValue, x => x.PaymentStatus == search.PaymentStatus)
                .WhereIf(search.StartDate.HasValue, x => x.CreateTime >= search.StartDate.Value)
                .WhereIf(search.EndDate.HasValue, x => x.CreateTime <= search.EndDate.Value);

            var result = await q.ProjectToType<OrderDTO>().GetPageResultAsync(input);
            return result;
        }

        public async Task<OrderDTO> GetTheDataAsync(string id)
        {
            var order = await GetEntityAsync(id);
            if (order == null) return null;

            var dto = order.Adapt<OrderDTO>();

            var details = await Db.GetIQueryable<OrderDetail>()
                .Where(x => x.OrderId == id)
                .ToListAsync();

            if (details.Any())
            {
                var productIds = details
                    .Where(d => !d.ProductId.IsNullOrEmpty())
                    .Select(d => d.ProductId)
                    .Distinct()
                    .ToList();

                var products = await _productDb.GetIQueryable<Product>()
                        .WhereIf(productIds.Any(),p => productIds.Contains(p.Id))
                        .Select(p => new { p.Id, p.ImageUrl })
                        .ToListAsync();

                var imageDict = products.ToDictionary(x => x.Id, x => x.ImageUrl);

                foreach (var detail in details)
                {
                    if (!detail.ProductId.IsNullOrEmpty() && imageDict.TryGetValue(detail.ProductId, out var img))
                    {
                        detail.ProductImageUrl = img;
                    }
                }
            }

            dto.Details = details;
            return dto;
        }

        [Transactional]
        public async Task AddDataAsync(OrderEditDTO data)
        {
            // 生成订单编号
            data.OrderNo = $"ORD{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";

            // 计算订单总金额
            if (data.Details != null && data.Details.Any())
            {
                data.TotalAmount = data.Details.Sum(x => x.SubTotal);
            }

            // 使用 BaseBusiness.InsertAsync 为订单实体初始化审计并插入
            var orderEntity = data.Adapt<Order>();
            await InsertAsync(orderEntity);
            var orderId = orderEntity.Id;

            // 保存订单明细，使用 BaseBusiness.InsertEntityAsync 统一处理审计字段
            if (data.Details != null && data.Details.Any())
            {
                foreach (var detail in data.Details)
                {
                    detail.OrderId = orderId;
                }
                await InsertEntityAsync(data.Details);
            }
        }

        [Transactional]
        public async Task UpdateDataAsync(OrderEditDTO data)
        {
            // 重新计算订单总金额
            if (data.Details != null && data.Details.Any())
            {
                data.TotalAmount = data.Details.Sum(x => x.SubTotal);
            }

            await UpdateAsync(data.Adapt<Order>());

            // 删除旧的订单明细
            await Db.DeleteAsync<OrderDetail>(x => x.OrderId == data.Id);

            // 保存新的订单明细：全部当作新明细插入，避免与已删除实体的主键冲突
            if (!data.Details.IsNullOrEmpty())
            {
                foreach (var detail in data.Details)
                {
                    // 置空 Id，让 InsertEntityAsync/InitCreateAudit 重新生成主键
                    detail.Id = null;
                    detail.OrderId = data.Id;
                }
                await InsertEntityAsync(data.Details);
            }
        }

        [Transactional]
        public async Task DeleteDataAsync(List<string> ids)
        {
            await DeleteAsync(ids);
            await Db.DeleteAsync<OrderDetail>(x => ids.Contains(x.OrderId));
        }

        public async Task UpdateStatusAsync(string id, OrderStatus status)
        {
            var order = await GetEntityAsync(id);
            if (order == null) throw new BusException("订单不存在");

            order.Status = status;
            if (status == OrderStatus.Shipped)
                order.ShippingTime = DateTime.Now;
            else if (status == OrderStatus.Completed)
                order.CompleteTime = DateTime.Now;

            await UpdateAsync(order);
        }

        public async Task UpdatePaymentStatusAsync(string id, PaymentStatus paymentStatus)
        {
            var order = await GetEntityAsync(id);
            if (order == null) throw new BusException("订单不存在");

            order.PaymentStatus = paymentStatus;
            if (paymentStatus == PaymentStatus.Paid)
                order.PaymentTime = DateTime.Now;

            await UpdateAsync(order);
        }

        #endregion
    }
}

