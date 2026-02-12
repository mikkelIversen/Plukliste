using WarehouseAPI.Models;

namespace WarehouseAPI.Services;

public class PicklistService : IPicklistService
{
    private readonly IDataService _dataService;
    private readonly IInventoryService _inventoryService;

    public PicklistService(IDataService dataService, IInventoryService inventoryService)
    {
        _dataService = dataService;
        _inventoryService = inventoryService;
    }

    public List<Picklist> GetAllPicklists()
    {
        return _dataService.Load<Picklist>("picklists.json");
    }

    public Picklist? GetPicklist(string id)
    {
        var picklists = _dataService.Load<Picklist>("picklists.json");
        return picklists.FirstOrDefault(p => p.Id == id);
    }

    public Picklist CreatePicklist(Picklist picklist)
    {
        // Validate stock availability
        foreach (var item in picklist.Items)
        {
            if (!_inventoryService.HasSufficientStock(item.ProductId, item.Qty))
            {
                var inventoryItem = _inventoryService.GetInventoryItem(item.ProductId);
                var available = inventoryItem != null 
                    ? inventoryItem.Quantity - inventoryItem.Reserved 
                    : 0;
                throw new InvalidOperationException(
                    $"Ikke nok på lager af {item.ProductId}. Tilgængeligt: {available}");
            }
        }

        // Reserve stock
        foreach (var item in picklist.Items)
        {
            _inventoryService.ReserveStock(item.ProductId, item.Qty);
        }

        // Save picklist
        picklist.CreatedAt = DateTime.Now;
        picklist.Status = "active";
        
        var picklists = _dataService.Load<Picklist>("picklists.json");
        picklists.Add(picklist);
        _dataService.Save("picklists.json", picklists);

        return picklist;
    }

    public Picklist CompletePicklist(string id)
    {
        var picklists = _dataService.Load<Picklist>("picklists.json");
        var picklist = picklists.FirstOrDefault(p => p.Id == id);

        if (picklist == null)
            throw new KeyNotFoundException($"Plukliste {id} findes ikke");

        if (picklist.Status == "completed")
            throw new InvalidOperationException("Plukliste er allerede afsluttet");

        // Deduct stock (removes from both quantity and reserved)
        foreach (var item in picklist.Items)
        {
            _inventoryService.DeductStock(item.ProductId, item.Qty);
        }

        picklist.Status = "completed";
        picklist.CompletedAt = DateTime.Now;
        
        _dataService.Save("picklists.json", picklists);
        return picklist;
    }

    public Picklist CancelPicklist(string id)
    {
        var picklists = _dataService.Load<Picklist>("picklists.json");
        var picklist = picklists.FirstOrDefault(p => p.Id == id);

        if (picklist == null)
            throw new KeyNotFoundException($"Plukliste {id} findes ikke");

        if (picklist.Status == "completed")
            throw new InvalidOperationException("Kan ikke annullere afsluttet plukliste");

        // Release reserved stock
        foreach (var item in picklist.Items)
        {
            _inventoryService.ReleaseReservedStock(item.ProductId, item.Qty);
        }

        picklist.Status = "cancelled";
        _dataService.Save("picklists.json", picklists);
        
        return picklist;
    }

    public bool DeletePicklist(string id)
    {
        var picklists = _dataService.Load<Picklist>("picklists.json");
        var picklist = picklists.FirstOrDefault(p => p.Id == id);

        if (picklist == null)
            return false;

        // Release stock if active
        if (picklist.Status == "active")
        {
            foreach (var item in picklist.Items)
            {
                _inventoryService.ReleaseReservedStock(item.ProductId, item.Qty);
            }
        }

        picklists.Remove(picklist);
        _dataService.Save("picklists.json", picklists);
        
        return true;
    }
}
