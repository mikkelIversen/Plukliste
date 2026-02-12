namespace WarehouseAPI.DTOs;

public class ProductRequest
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Location { get; set; } = "";
    public string Description { get; set; } = "";
    public string Barcode { get; set; } = "";
    public int MinStock { get; set; } = 0;
    public int InitialQty { get; set; }
}

public class CreateUserRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? Role { get; set; }
}

public class InventoryAdjustment
{
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}

public class LowStockItem
{
    public string ProductId { get; set; } = "";
    public string Name { get; set; } = "";
    public int Current { get; set; }
    public int MinStock { get; set; }
}
