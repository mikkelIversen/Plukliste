namespace WarehouseAPI.Models;

public class Picklist
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "active";
    public List<PickItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Notes { get; set; } = "";
}

public class PickItem
{
    public string ProductId { get; set; } = "";
    public int Qty { get; set; }
}
