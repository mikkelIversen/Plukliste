using WarehouseAPI.Models;
using WarehouseAPI.DTOs;

namespace WarehouseAPI.Services;

public class InventoryService : IInventoryService
{
    private readonly IDataService _dataService;

    public InventoryService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public List<InventoryItem> GetAllInventory()
    {
        return _dataService.Load<InventoryItem>("inventory.json");
    }

    public InventoryItem? GetInventoryItem(string productId)
    {
        var inventory = _dataService.Load<InventoryItem>("inventory.json");
        return inventory.FirstOrDefault(i => i.ProductId == productId);
    }

    public List<LowStockItem> GetLowStockItems()
    {
        var inventory = _dataService.Load<InventoryItem>("inventory.json");
        var products = _dataService.Load<Product>("products.json");

        var lowStock = from inv in inventory
                       join prod in products on inv.ProductId equals prod.Id
                       where inv.Quantity - inv.Reserved <= prod.MinStock
                       select new LowStockItem
                       {
                           ProductId = inv.ProductId,
                           Name = prod.Name,
                           Current = inv.Quantity - inv.Reserved,
                           MinStock = prod.MinStock
                       };

        return lowStock.ToList();
    }

    public InventoryItem AdjustInventory(string productId, int quantity, string? reason = null)
    {
        var inventory = _dataService.Load<InventoryItem>("inventory.json");
        var item = inventory.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
            throw new KeyNotFoundException($"Inventar for produkt {productId} findes ikke");

        item.Quantity += quantity;
        if (item.Quantity < 0) item.Quantity = 0;

        _dataService.Save("inventory.json", inventory);
        return item;
    }

    public bool HasSufficientStock(string productId, int requiredQuantity)
    {
        var inventory = _dataService.Load<InventoryItem>("inventory.json");
        var item = inventory.FirstOrDefault(i => i.ProductId == productId);
        
        if (item == null) return false;
        
        return (item.Quantity - item.Reserved) >= requiredQuantity;
    }

    public void ReserveStock(string productId, int quantity)
    {
        var inventory = _dataService.Load<InventoryItem>("inventory.json");
        var item = inventory.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
            throw new KeyNotFoundException($"Inventar for produkt {productId} findes ikke");

        if (!HasSufficientStock(productId, quantity))
            throw new InvalidOperationException($"Ikke nok på lager af {productId}");

        item.Reserved += quantity;
        _dataService.Save("inventory.json", inventory);
    }

    public void ReleaseReservedStock(string productId, int quantity)
    {
        var inventory = _dataService.Load<InventoryItem>("inventory.json");
        var item = inventory.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
            throw new KeyNotFoundException($"Inventar for produkt {productId} findes ikke");

        item.Reserved -= quantity;
        if (item.Reserved < 0) item.Reserved = 0;

        _dataService.Save("inventory.json", inventory);
    }

    public void DeductStock(string productId, int quantity)
    {
        var inventory = _dataService.Load<InventoryItem>("inventory.json");
        var item = inventory.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
            throw new KeyNotFoundException($"Inventar for produkt {productId} findes ikke");

        item.Quantity -= quantity;
        item.Reserved -= quantity;
        
        if (item.Quantity < 0) item.Quantity = 0;
        if (item.Reserved < 0) item.Reserved = 0;

        _dataService.Save("inventory.json", inventory);
    }
}
