using WarehouseAPI.Models;

namespace WarehouseAPI.Services;

public interface IProductService
{
    List<Product> GetAllProducts();
    Product? GetProduct(string id);
    Product CreateProduct(Product product, int initialQuantity);
    Product UpdateProduct(string id, Product updatedProduct);
    bool DeleteProduct(string id);
    List<string> GetCategories();
    bool ProductExists(string id);
}
