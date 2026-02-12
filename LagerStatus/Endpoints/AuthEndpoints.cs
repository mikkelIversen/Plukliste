using WarehouseAPI.DTOs;
using WarehouseAPI.Services;

namespace WarehouseAPI.Middleware;

public static class AuthEndpoints
{
    public static void RegisterAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", async (HttpContext context, IUserService userService, IAuthService authService) =>
        {
            var req = await context.Request.ReadFromJsonAsync<LoginRequest>();
            if (req == null) return Results.BadRequest("Invalid request");

            var user = userService.ValidateUser(req.Username, req.Password);

            if (user == null)
                return Results.Unauthorized();

            var sessionToken = authService.CreateSession(user.Username);

            return Results.Ok(new LoginResponse
            {
                Token = sessionToken,
                Username = user.Username,
                Role = user.Role
            });
        });

        app.MapPost("/auth/logout", async (HttpContext context, IAuthService authService) =>
        {
            var req = await context.Request.ReadFromJsonAsync<LogoutRequest>();
            if (req == null) return Results.BadRequest("Invalid request");

            authService.RemoveSession(req.Token);
            return Results.Ok();
        });

        app.MapPost("/auth/validate", async (HttpContext context, IAuthService authService, IUserService userService) =>
        {
            var req = await context.Request.ReadFromJsonAsync<ValidateRequest>();
            if (req == null) return Results.BadRequest("Invalid request");

            var session = authService.GetSession(req.Token);
            if (session == null)
                return Results.Unauthorized();

            var user = userService.GetUser(session.Username);
            if (user == null)
                return Results.Unauthorized();

            return Results.Ok(new ValidateResponse
            {
                Username = user.Username,
                Role = user.Role
            });
        });
    }
}
