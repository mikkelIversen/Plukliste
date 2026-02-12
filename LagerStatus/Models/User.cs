namespace WarehouseAPI.Models;

public class User
{
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "user";
    public DateTime CreatedAt { get; set; }
}

public class Session
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
