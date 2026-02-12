using System.Security.Cryptography;
using System.Text;
using WarehouseAPI.Models;

namespace WarehouseAPI.Services;

public class AuthService : IAuthService
{
    private readonly IDataService _dataService;

    public AuthService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool ValidatePassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    public string CreateSession(string username)
    {
        var sessionToken = Guid.NewGuid().ToString();
        var sessions = _dataService.Load<Session>("sessions.json");

        sessions.Add(new Session
        {
            Token = sessionToken,
            Username = username,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddHours(24)
        });

        _dataService.Save("sessions.json", sessions);
        return sessionToken;
    }

    public bool IsAuthenticated(string? token)
    {
        if (string.IsNullOrEmpty(token)) return false;

        var sessions = _dataService.Load<Session>("sessions.json");
        return sessions.Any(s => s.Token == token && s.ExpiresAt > DateTime.Now);
    }

    public Session? GetSession(string token)
    {
        var sessions = _dataService.Load<Session>("sessions.json");
        return sessions.FirstOrDefault(s => s.Token == token && s.ExpiresAt > DateTime.Now);
    }

    public void RemoveSession(string token)
    {
        var sessions = _dataService.Load<Session>("sessions.json");
        sessions.RemoveAll(s => s.Token == token);
        _dataService.Save("sessions.json", sessions);
    }

    public string? GetUsernameFromToken(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        var session = GetSession(token);
        return session?.Username;
    }
}
