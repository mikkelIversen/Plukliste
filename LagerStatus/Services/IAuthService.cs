using WarehouseAPI.Models;

namespace WarehouseAPI.Services;

public interface IAuthService
{
    string HashPassword(string password);
    bool ValidatePassword(string password, string hash);
    string CreateSession(string username);
    bool IsAuthenticated(string? token);
    Session? GetSession(string token);
    void RemoveSession(string token);
    string? GetUsernameFromToken(string? token);
}
