using WarehouseAPI.Models;

namespace WarehouseAPI.Services;

public class StatsService : IStatsService
{
    private readonly IDataService _dataService;

    public StatsService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public object GetStatistics()
    {
        var products = _dataService.Load<Product>("products.json");
        var inventory = _dataService.Load<InventoryItem>("inventory.json");
        var picklists = _dataService.Load<Picklist>("picklists.json");
        var notes = _dataService.Load<Note>("notes.json");

        var lowStockCount = inventory.Count(i =>
        {
            var prod = products.FirstOrDefault(p => p.Id == i.ProductId);
            return prod != null && (i.Quantity - i.Reserved) <= prod.MinStock;
        });

        return new
        {
            TotalProducts = products.Count,
            TotalStock = inventory.Sum(i => i.Quantity),
            TotalReserved = inventory.Sum(i => i.Reserved),
            ActivePicklists = picklists.Count(p => p.Status == "active"),
            CompletedPicklists = picklists.Count(p => p.Status == "completed"),
            LowStockItems = lowStockCount,
            TotalNotes = notes.Count,
            UnresolvedNotes = notes.Count(n => !n.IsResolved),
            PinnedNotes = notes.Count(n => n.IsPinned && !n.IsResolved),
            UrgentNotes = notes.Count(n => n.Priority == "urgent" && !n.IsResolved)
        };
    }
}
