using WarehouseAPI.Models;

namespace WarehouseAPI.Services;

public class UserService : IUserService
{
    private readonly IDataService _dataService;
    private readonly IAuthService _authService;

    public UserService(IDataService dataService, IAuthService authService)
    {
        _dataService = dataService;
        _authService = authService;
    }

    public void InitializeDefaultAdmin()
    {
        var users = _dataService.Load<User>("users.json");
        if (users.Count == 0)
        {
            users.Add(new User
            {
                Username = "admin",
                PasswordHash = _authService.HashPassword("admin123"),
                Role = "admin",
                CreatedAt = DateTime.Now
            });
            _dataService.Save("users.json", users);
        }
    }

    public User? GetUser(string username)
    {
        var users = _dataService.Load<User>("users.json");
        return users.FirstOrDefault(u => u.Username == username);
    }

    public User? ValidateUser(string username, string password)
    {
        var users = _dataService.Load<User>("users.json");
        var passwordHash = _authService.HashPassword(password);
        return users.FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);
    }

    public List<User> GetAllUsers()
    {
        return _dataService.Load<User>("users.json");
    }

    public User CreateUser(string username, string password, string role)
    {
        var users = _dataService.Load<User>("users.json");

        if (users.Any(u => u.Username == username))
            throw new InvalidOperationException("Brugernavn findes allerede");

        var newUser = new User
        {
            Username = username,
            PasswordHash = _authService.HashPassword(password),
            Role = role,
            CreatedAt = DateTime.Now
        };

        users.Add(newUser);
        _dataService.Save("users.json", users);

        return newUser;
    }

    public bool DeleteUser(string username)
    {
        if (username == "admin")
            throw new InvalidOperationException("Kan ikke slette admin brugeren");

        var users = _dataService.Load<User>("users.json");
        var countBefore = users.Count;
        users.RemoveAll(u => u.Username == username);
        
        if (users.Count < countBefore)
        {
            _dataService.Save("users.json", users);
            return true;
        }
        
        return false;
    }

    public bool UserExists(string username)
    {
        var users = _dataService.Load<User>("users.json");
        return users.Any(u => u.Username == username);
    }
}
