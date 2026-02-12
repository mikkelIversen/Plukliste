using WarehouseAPI.Services;

namespace WarehouseAPI.Middleware;

public static class AuthMiddleware
{
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
