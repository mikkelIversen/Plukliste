namespace WarehouseAPI.Models;

public class Note
{
    public string Id { get; set; } = "";

    public string EntityType { get; set; } = "";

    public string? EntityId { get; set; }

   public string Title { get; set; } = "";

    public string Content { get; set; } = "";

    public string CreatedBy { get; set; } = "";

    public DateTime CreatedAt { get; set; }

   public DateTime? UpdatedAt { get; set; }

    public string Priority { get; set; } = "normal";

   public List<string> Tags { get; set; } = new();

    public bool IsPinned { get; set; } = false;

    public bool IsResolved { get; set; } = false;
}
