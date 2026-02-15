using System.Security.Cryptography; 
using System.Text;                
using WarehouseAPI.Models;        

namespace WarehouseAPI.Services;

// Handles password hashing + session creation + token validation
public class AuthService : IAuthService
{
    private readonly IDataService _dataService;

    public AuthService(IDataService dataService)
    {
        _dataService = dataService;
    }

    // Used when storing passwords or checking login input
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();

        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

        return Convert.ToBase64String(hashedBytes);
    }

    // Compares plain password with stored hash
    public bool ValidatePassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    // Creates a login session and returns a token
    public string CreateSession(string username)
    {
        var sessionToken = Guid.NewGuid().ToString();

        var sessions = _dataService.Load<Session>("sessions.json");

        // Add new session record
        sessions.Add(new Session
        {
            Token = sessionToken,
            Username = username,

            CreatedAt = DateTime.Now,

            // Session expires in 24 hours
            ExpiresAt = DateTime.Now.AddHours(24)
        });

        // Save updated sessions back to file
        _dataService.Save("sessions.json", sessions);

       return sessionToken;
    }

    // Checks if token exists and is not expired
    public bool IsAuthenticated(string? token)
    {
        if (string.IsNullOrEmpty(token)) return false;

        var sessions = _dataService.Load<Session>("sessions.json");

        return sessions.Any(s =>
            s.Token == token &&
            s.ExpiresAt > DateTime.Now);
    }

    // Returns session object if valid, otherwise null
    public Session? GetSession(string token)
    {
        var sessions = _dataService.Load<Session>("sessions.json");

        return sessions.FirstOrDefault(s =>
            s.Token == token &&
            s.ExpiresAt > DateTime.Now);
    }

    // Deletes session (used during logout)
    public void RemoveSession(string token)
    {
        var sessions = _dataService.Load<Session>("sessions.json");

        sessions.RemoveAll(s => s.Token == token);

        _dataService.Save("sessions.json", sessions);
    }

    // Helper: get username linked to a token
    public string? GetUsernameFromToken(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;

        var session = GetSession(token);

        return session?.Username;
    }
}
