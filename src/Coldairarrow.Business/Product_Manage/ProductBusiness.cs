using Coldairarrow.Entity.Product_Manage;
using Coldairarrow.IBusiness;
using Coldairarrow.IBusiness.Product_Manage;
using Coldairarrow.Util;
using EFCore.Sharding;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Coldairarrow.Business.Product_Manage
{
    public class ProductBusiness : BaseBusiness<Product>, IProductBusiness, ITransientDependency
    {
        public ProductBusiness(IProductDbAccessor db, IOperator @operator = null)
            : base(db, @operator)
        {
        }

        protected override string _textField => "Name";

        #region 外部接口

        public async Task<PageResult<ProductDTO>> GetDataListAsync(PageInput<ProductQueryDTO> input)
        {
            Expression<Func<Product, ProductCategory, ProductDTO>> select = (a, b) => new ProductDTO
            {
                CategoryName = b.Name
            };
            var search = input.Search;
            select = select.BuildExtendSelectExpre();
            var q = from a in GetIQueryable().AsExpandable()
                    join b in Db.GetIQueryable<ProductCategory>() on a.CategoryId equals b.Id into ab
                    from b in ab.DefaultIfEmpty()
                    select @select.Invoke(a, b);

            q = q.WhereIf(!search.Keyword.IsNullOrEmpty(), x => x.Name.Contains(search.Keyword) || x.ProductCode.Contains(search.Keyword))
                 .WhereIf(!search.CategoryId.IsNullOrEmpty(), x => x.CategoryId == search.CategoryId)
                 .WhereIf(search.Status.HasValue, x => x.Status == search.Status);

            return await q.GetPageResultAsync(input);
        }

        public async Task<Product> GetTheDataAsync(string id)
        {
            return await GetEntityAsync(id);
        }

        public new async Task<List<SelectOption>> GetOptionListAsync(OptionListInputDTO input)
        {
            return await GetOptionListAsync(input, _textField, _valueField, null);
        }

        [DataRepeatValidate(new string[] { "ProductCode" }, new string[] { "产品编码" })]
        public async Task AddDataAsync(Product data)
        {
            await InsertAsync(data);
        }

        [DataRepeatValidate(new string[] { "ProductCode" }, new string[] { "产品编码" })]
        public async Task UpdateDataAsync(Product data)
        {
            await UpdateAsync(data);
        }

        public async Task DeleteDataAsync(List<string> ids)
        {
            await DeleteAsync(ids);
        }

        #endregion
    }
}

