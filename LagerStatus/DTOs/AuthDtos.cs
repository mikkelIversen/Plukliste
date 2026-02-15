// DTOs = Data Transfer Objects 
namespace WarehouseAPI.DTOs;

// Sent FROM client TO API when user tries to log in
public class LoginRequest
{
   
    public string Username { get; set; } = "";


    public string Password { get; set; } = "";
}

// Sent FROM client TO API when logging out
public class LogoutRequest
{
   
    public string Token { get; set; } = "";
}

// Sent FROM client TO API to check if a token is still valid
public class ValidateRequest
{

    public string Token { get; set; } = "";
}

// Returned FROM API TO client after successful login
public class LoginResponse
{
     public string Token { get; set; } = "";

   
    public string Username { get; set; } = "";

   public string Role { get; set; } = "";
}

// Returned FROM API TO client after token validation
public class ValidateResponse
{
    
    public string Username { get; set; } = "";

    
    public string Role { get; set; } = "";
}
