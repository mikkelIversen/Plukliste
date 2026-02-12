using WarehouseAPI.DTOs;
using WarehouseAPI.Services;

namespace WarehouseAPI.Middleware;

public static class UserEndpoints
{
    public static void RegisterUserEndpoints(this WebApplication app)
    {
        app.MapGet("/users", (HttpContext context, IAuthService authService, IUserService userService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            var users = userService.GetAllUsers();
            return Results.Ok(users.Select(u => new
            {
                u.Username,
                u.Role,
                u.CreatedAt
            }));
        });

        app.MapPost("/users", async (HttpContext context, IAuthService authService, IUserService userService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            var req = await context.Request.ReadFromJsonAsync<CreateUserRequest>();
            if (req == null) return Results.BadRequest("Invalid request");

            try
            {
                var newUser = userService.CreateUser(req.Username, req.Password, req.Role ?? "user");
                return Results.Ok(new
                {
                    newUser.Username,
                    newUser.Role,
                    newUser.CreatedAt
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        app.MapDelete("/users/{username}", (HttpContext context, string username, IAuthService authService, IUserService userService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            try
            {
                userService.DeleteUser(username);
                return Results.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });
    }
}
