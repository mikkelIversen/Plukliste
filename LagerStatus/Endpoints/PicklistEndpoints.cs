using WarehouseAPI.Models;
using WarehouseAPI.Services;

namespace WarehouseAPI.Middleware;

public static class PicklistEndpoints
{
    public static void RegisterPicklistEndpoints(this WebApplication app)
    {
        app.MapGet("/picklists", (HttpContext context, IAuthService authService, IPicklistService picklistService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            return Results.Ok(picklistService.GetAllPicklists());
        });

        app.MapGet("/picklists/{id}", (HttpContext context, string id, IAuthService authService, IPicklistService picklistService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            var picklist = picklistService.GetPicklist(id);
            return picklist != null ? Results.Ok(picklist) : Results.NotFound();
        });

        app.MapPost("/picklists", async (HttpContext context, IAuthService authService, IPicklistService picklistService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            var pl = await context.Request.ReadFromJsonAsync<Picklist>();
            if (pl == null) return Results.BadRequest("Invalid request");

            try
            {
                var picklist = picklistService.CreatePicklist(pl);
                return Results.Ok(picklist);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        app.MapPost("/picklists/{id}/complete", (HttpContext context, string id, IAuthService authService, IPicklistService picklistService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            try
            {
                var picklist = picklistService.CompletePicklist(id);
                return Results.Ok(picklist);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        app.MapPost("/picklists/{id}/cancel", (HttpContext context, string id, IAuthService authService, IPicklistService picklistService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            try
            {
                var picklist = picklistService.CancelPicklist(id);
                return Results.Ok(picklist);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        app.MapDelete("/picklists/{id}", (HttpContext context, string id, IAuthService authService, IPicklistService picklistService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            var deleted = picklistService.DeletePicklist(id);
            return deleted ? Results.Ok() : Results.NotFound();
        });
    }
}
