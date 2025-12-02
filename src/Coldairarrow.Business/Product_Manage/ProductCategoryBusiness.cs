using Coldairarrow.Entity.Product_Manage;
using Coldairarrow.IBusiness;
using Coldairarrow.IBusiness.Product_Manage;
using Coldairarrow.Util;
using EFCore.Sharding;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coldairarrow.Business.Product_Manage
{
    public class ProductCategoryBusiness : BaseBusiness<ProductCategory>, IProductCategoryBusiness, ITransientDependency
    {
        public ProductCategoryBusiness(IProductDbAccessor db, IOperator @operator = null)
            : base(db, @operator)
        {
        }

        #region 外部接口

        public async Task<List<ProductCategoryTreeDTO>> GetTreeDataListAsync()
        {
            var list = await GetIQueryable().ToListAsync();
            var treeList = list
                .Select(x => new ProductCategoryTreeDTO
                {
                    Id = x.Id,
                    ParentId = x.ParentId,
                    Text = x.Name,
                    Value = x.Id,
                    Description = x.Description,
                    Sort = x.Sort,
                    Enabled = x.Enabled
                }).ToList();

            return TreeHelper.BuildTree(treeList);
        }

        public async Task<ProductCategory> GetTheDataAsync(string id)
        {
            return await GetEntityAsync(id);
        }

        [DataRepeatValidate(new string[] { "Name" }, new string[] { "分类名称" })]
        public async Task AddDataAsync(ProductCategory data)
        {
            await InsertAsync(data);
        }

        [DataRepeatValidate(new string[] { "Name" }, new string[] { "分类名称" })]
        public async Task UpdateDataAsync(ProductCategory data)
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

