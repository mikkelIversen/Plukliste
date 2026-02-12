using WarehouseAPI.Models;

namespace WarehouseAPI.Services;

public interface IUserService
{
    void InitializeDefaultAdmin();
    User? GetUser(string username);
    User? ValidateUser(string username, string password);
    List<User> GetAllUsers();
    User CreateUser(string username, string password, string role);
    bool DeleteUser(string username);
    bool UserExists(string username);
}
