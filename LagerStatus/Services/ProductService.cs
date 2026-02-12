using WarehouseAPI.Models;

namespace WarehouseAPI.Services;

public class ProductService : IProductService
{
    private readonly IDataService _dataService;

    public ProductService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public List<Product> GetAllProducts()
    {
        return _dataService.Load<Product>("products.json");
    }

    public Product? GetProduct(string id)
    {
        var products = _dataService.Load<Product>("products.json");
        return products.FirstOrDefault(p => p.Id == id);
    }

    public Product CreateProduct(Product product, int initialQuantity)
    {
        var products = _dataService.Load<Product>("products.json");
        var inventory = _dataService.Load<InventoryItem>("inventory.json");

        if (products.Any(p => p.Id == product.Id))
            throw new InvalidOperationException("Produkt ID findes allerede");

        products.Add(product);
        inventory.Add(new InventoryItem
        {
            ProductId = product.Id,
            Quantity = initialQuantity,
            Reserved = 0
        });

        _dataService.Save("products.json", products);
        _dataService.Save("inventory.json", inventory);

        return product;
    }

    public Product UpdateProduct(string id, Product updatedProduct)
    {
        var products = _dataService.Load<Product>("products.json");
        var product = products.FirstOrDefault(p => p.Id == id);

        if (product == null)
            throw new KeyNotFoundException($"Produkt med ID {id} findes ikke");

        product.Name = updatedProduct.Name;
        product.Category = updatedProduct.Category;
        product.Location = updatedProduct.Location;
        product.Description = updatedProduct.Description;
        product.Barcode = updatedProduct.Barcode;
        product.MinStock = updatedProduct.MinStock;

        _dataService.Save("products.json", products);
        return product;
    }

    public bool DeleteProduct(string id)
    {
        var picklists = _dataService.Load<Picklist>("picklists.json");
        
        if (picklists.Any(pl => pl.Status == "active" && pl.Items.Any(i => i.ProductId == id)))
            throw new InvalidOperationException("Kan ikke slette - produkt er i aktiv plukliste");

        var products = _dataService.Load<Product>("products.json");
        var inventory = _dataService.Load<InventoryItem>("inventory.json");

        var productRemoved = products.RemoveAll(p => p.Id == id) > 0;
        inventory.RemoveAll(i => i.ProductId == id);

        if (productRemoved)
        {
            _dataService.Save("products.json", products);
            _dataService.Save("inventory.json", inventory);
        }

        return productRemoved;
    }

    public List<string> GetCategories()
    {
        var products = _dataService.Load<Product>("products.json");
        return products
            .Select(p => p.Category)
            .Distinct()
            .Where(c => !string.IsNullOrEmpty(c))
            .OrderBy(c => c)
            .ToList();
    }

    public bool ProductExists(string id)
    {
        var products = _dataService.Load<Product>("products.json");
        return products.Any(p => p.Id == id);
    }
}
