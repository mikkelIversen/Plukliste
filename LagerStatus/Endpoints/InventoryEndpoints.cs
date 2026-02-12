using WarehouseAPI.DTOs;
using WarehouseAPI.Services;

namespace WarehouseAPI.Middleware;

public static class InventoryEndpoints
{
    public static void RegisterInventoryEndpoints(this WebApplication app)
    {
        app.MapGet("/inventory", (HttpContext context, IAuthService authService, IInventoryService inventoryService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            return Results.Ok(inventoryService.GetAllInventory());
        });

        app.MapGet("/inventory/low-stock", (HttpContext context, IAuthService authService, IInventoryService inventoryService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            return Results.Ok(inventoryService.GetLowStockItems());
        });

        app.MapPost("/inventory/{productId}/adjust", async (HttpContext context, string productId, IAuthService authService, IInventoryService inventoryService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            var adj = await context.Request.ReadFromJsonAsync<InventoryAdjustment>();
            if (adj == null) return Results.BadRequest("Invalid request");

            try
            {
                var item = inventoryService.AdjustInventory(productId, adj.Quantity, adj.Reason);
                return Results.Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });
    }
}
