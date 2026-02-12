using WarehouseAPI.DTOs;
using WarehouseAPI.Models;
using WarehouseAPI.Services;

namespace WarehouseAPI.Middleware;

public static class ProductEndpoints
{
    public static void RegisterProductEndpoints(this WebApplication app)
    {
        app.MapGet("/products", (HttpContext context, IAuthService authService, IProductService productService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            return Results.Ok(productService.GetAllProducts());
        });

        app.MapGet("/products/{id}", (HttpContext context, string id, IAuthService authService, IProductService productService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            var product = productService.GetProduct(id);
            return product != null ? Results.Ok(product) : Results.NotFound();
        });

        app.MapPost("/products", async (HttpContext context, IAuthService authService, IProductService productService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            var req = await context.Request.ReadFromJsonAsync<ProductRequest>();
            if (req == null) return Results.BadRequest("Invalid request");

            try
            {
                var newProduct = new Product
                {
                    Id = req.Id,
                    Name = req.Name,
                    Category = req.Category,
                    Location = req.Location,
                    Description = req.Description,
                    Barcode = req.Barcode,
                    MinStock = req.MinStock
                };

                var product = productService.CreateProduct(newProduct, req.InitialQty);
                return Results.Ok(product);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        app.MapPut("/products/{id}", async (HttpContext context, string id, IAuthService authService, IProductService productService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            var req = await context.Request.ReadFromJsonAsync<Product>();
            if (req == null) return Results.BadRequest("Invalid request");

            try
            {
                var product = productService.UpdateProduct(id, req);
                return Results.Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        app.MapDelete("/products/{id}", (HttpContext context, string id, IAuthService authService, IProductService productService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            try
            {
                productService.DeleteProduct(id);
                return Results.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        app.MapGet("/categories", (HttpContext context, IAuthService authService, IProductService productService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService)) 
                return Results.Unauthorized();

            return Results.Ok(productService.GetCategories());
        });
    }
}
