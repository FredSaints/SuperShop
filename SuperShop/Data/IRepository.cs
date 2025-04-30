using SuperShop.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperShop.Data
{
    public interface IRepository
    {
        void AddProduct(Product product);

        IEnumerable<Product> GetProducts();

        Product GetProduct(int id);

        bool ProductExists(int id);

        void RemoveProduct(Product product);

        Task<bool> SaveAllAsync();

        void UpdateProduct(Product product);
    }
}