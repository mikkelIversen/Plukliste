namespace WarehouseAPI.DTOs;

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LogoutRequest
{
    public string Token { get; set; } = "";
}

public class ValidateRequest
{
    public string Token { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
}

public class ValidateResponse
{
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
}
