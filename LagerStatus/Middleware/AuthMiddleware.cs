using WarehouseAPI.Services;

namespace WarehouseAPI.Middleware;

// Static helper class for checking authentication from HTTP requests
public static class AuthMiddleware
{
    // Checks if the current request is authenticated
    public static bool IsAuthenticated(HttpContext context, IAuthService authService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault();

        return authService.IsAuthenticated(token);
    }

    public static string? GetAuthToken(HttpContext context)
    {
       return context.Request.Headers["Authorization"].FirstOrDefault();
    }
}
