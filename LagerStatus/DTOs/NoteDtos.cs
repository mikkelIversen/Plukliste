namespace WarehouseAPI.DTOs;

public class CreateNoteRequest
{
    public string EntityType { get; set; } = "";
    public string? EntityId { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string Priority { get; set; } = "normal";
    public List<string> Tags { get; set; } = new();
    public bool IsPinned { get; set; } = false;
}

public class UpdateNoteRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Priority { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsPinned { get; set; }
    public bool? IsResolved { get; set; }
}

public class NoteFilterRequest
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Priority { get; set; }
    public bool? IsPinned { get; set; }
    public bool? IsResolved { get; set; }
    public List<string>? Tags { get; set; }
}
