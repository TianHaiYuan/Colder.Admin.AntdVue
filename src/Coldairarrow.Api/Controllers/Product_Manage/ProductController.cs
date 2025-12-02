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
    /// 产品管理
    /// </summary>
    [Route("/Product_Manage/[controller]/[action]")]
    [OpenApiTag("产品管理")]
    public class ProductController : BaseApiController
    {
        #region DI

        public ProductController(IProductBusiness productBus)
        {
            _productBus = productBus;
        }

        IProductBusiness _productBus { get; }

        #endregion

        #region 获取

        [HttpPost]
        public async Task<PageResult<ProductDTO>> GetDataList(PageInput<ProductQueryDTO> input)
        {
            return await _productBus.GetDataListAsync(input);
        }

        [HttpPost]
        public async Task<Product> GetTheData(IdInputDTO input)
        {
            return await _productBus.GetTheDataAsync(input.id) ?? new Product();
        }

        [HttpPost]
        public async Task<List<SelectOption>> GetOptionList(OptionListInputDTO input)
        {
            return await _productBus.GetOptionListAsync(input);
        }

        #endregion

        #region 提交

        [HttpPost]
        public async Task SaveData(Product theData)
        {
            if (theData.Id.IsNullOrEmpty())
            {
                await _productBus.AddDataAsync(theData);
            }
            else
            {
                await _productBus.UpdateDataAsync(theData);
            }
        }

        [HttpPost]
        public async Task DeleteData(List<string> ids)
        {
            await _productBus.DeleteDataAsync(ids);
        }

        #endregion
    }
}

