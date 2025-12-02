using Coldairarrow.Entity.Product_Manage;
using Coldairarrow.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coldairarrow.Business.Product_Manage
{
    public interface IProductBusiness
    {
        Task<PageResult<ProductDTO>> GetDataListAsync(PageInput<ProductQueryDTO> input);
        Task<Product> GetTheDataAsync(string id);
        Task<List<SelectOption>> GetOptionListAsync(OptionListInputDTO input);
        Task AddDataAsync(Product data);
        Task UpdateDataAsync(Product data);
        Task DeleteDataAsync(List<string> ids);
    }

    public class ProductQueryDTO
    {
        public string Keyword { get; set; }
        public string CategoryId { get; set; }
        public ProductStatus? Status { get; set; }
    }

    [Map(typeof(Product))]
    public class ProductDTO : Product
    {
        public string CategoryName { get; set; }
        public string StatusText { get => Status.GetDescription(); }
    }
}

