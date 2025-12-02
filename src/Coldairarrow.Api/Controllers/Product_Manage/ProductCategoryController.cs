using Coldairarrow.Business.Product_Manage;
using Coldairarrow.Entity.Product_Manage;
using Coldairarrow.Util;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coldairarrow.Api.Controllers.Product_Manage
{
    /// <summary>
    /// 产品分类
    /// </summary>
    [Route("/Product_Manage/[controller]/[action]")]
    [OpenApiTag("产品分类")]
    public class ProductCategoryController : BaseApiController
    {
        #region DI

        public ProductCategoryController(IProductCategoryBusiness categoryBus)
        {
            _categoryBus = categoryBus;
        }

        IProductCategoryBusiness _categoryBus { get; }

        #endregion

        #region 获取

        [HttpPost]
        public async Task<ProductCategory> GetTheData(IdInputDTO input)
        {
            return await _categoryBus.GetTheDataAsync(input.id) ?? new ProductCategory();
        }

        [HttpPost]
        public async Task<List<ProductCategoryTreeDTO>> GetTreeDataList()
        {
            return await _categoryBus.GetTreeDataListAsync();
        }

        #endregion

        #region 提交

        [HttpPost]
        public async Task SaveData(ProductCategory theData)
        {
            if (theData.Id.IsNullOrEmpty())
            {
                await _categoryBus.AddDataAsync(theData);
            }
            else
            {
                await _categoryBus.UpdateDataAsync(theData);
            }
        }

        [HttpPost]
        public async Task DeleteData(List<string> ids)
        {
            await _categoryBus.DeleteDataAsync(ids);
        }

        #endregion
    }
}

