using WarehouseAPI.Models;

namespace WarehouseAPI.Services;

public interface IPicklistService
{
    List<Picklist> GetAllPicklists();
    Picklist? GetPicklist(string id);
    Picklist CreatePicklist(Picklist picklist);
    Picklist CompletePicklist(string id);
    Picklist CancelPicklist(string id);
    bool DeletePicklist(string id);
}
