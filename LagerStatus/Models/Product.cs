namespace WarehouseAPI.Models;

public class Product
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Location { get; set; } = "";
    public string Description { get; set; } = "";
    public string Barcode { get; set; } = "";
    public int MinStock { get; set; } = 0;
}

public class InventoryItem
{
    public string ProductId { get; set; } = "";
    public int Quantity { get; set; }
    public int Reserved { get; set; }
}
