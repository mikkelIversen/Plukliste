using WarehouseAPI.Models;
using WarehouseAPI.DTOs;

namespace WarehouseAPI.Services;

public interface IInventoryService
{
    List<InventoryItem> GetAllInventory();
    InventoryItem? GetInventoryItem(string productId);
    List<LowStockItem> GetLowStockItems();
    InventoryItem AdjustInventory(string productId, int quantity, string? reason = null);
    bool HasSufficientStock(string productId, int requiredQuantity);
    void ReserveStock(string productId, int quantity);
    void ReleaseReservedStock(string productId, int quantity);
    void DeductStock(string productId, int quantity);
}
