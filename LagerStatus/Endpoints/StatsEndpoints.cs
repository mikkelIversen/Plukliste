using WarehouseAPI.Services;

namespace WarehouseAPI.Middleware;

public static class StatsEndpoints
{
    public static void RegisterStatsEndpoints(this WebApplication app)
    {
        app.MapGet("/stats", (HttpContext context, IAuthService authService, IStatsService statsService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            return Results.Ok(statsService.GetStatistics());
        });
    }
}
