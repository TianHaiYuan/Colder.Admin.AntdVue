using Coldairarrow.Entity.Product_Manage;
using Coldairarrow.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coldairarrow.Business.Product_Manage
{
    public interface IProductCategoryBusiness
    {
        Task<List<ProductCategoryTreeDTO>> GetTreeDataListAsync();
        Task<ProductCategory> GetTheDataAsync(string id);
        Task AddDataAsync(ProductCategory data);
        Task UpdateDataAsync(ProductCategory data);
        Task DeleteDataAsync(List<string> ids);
    }

    public class ProductCategoryTreeDTO : TreeModel
    {
        public object children { get => Children; }
        public string title { get => Text; }
        public string value { get => Id; }
        public string key { get => Id; }
        public string Description { get; set; }
        public int Sort { get; set; }
        public bool Enabled { get; set; }
    }
}

