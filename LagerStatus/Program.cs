using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

// main should primarily be used for entry point.
// nothing else. everything else you should have in other files

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();

string dataPath = "Data";
Directory.CreateDirectory(dataPath);

//util file instead of program :sob:
List<T> Load<T>(string file)
{
    var path = Path.Combine(dataPath, file);
    if (!File.Exists(path)) return new List<T>();
    return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path)) ?? new List<T>();
}
//util file instead of program :sob:
void Save<T>(string file, List<T> data)
{
    var path = Path.Combine(dataPath, file);
    File.WriteAllText(path, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
}
//util file instead of program :sob:
string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hashedBytes);
}

//user_handler/user_controller file instead of program :sob:
void InitializeUsers()
{
    var users = Load<User>("users.json");
    if (users.Count == 0)
    {
        users.Add(new User
        {
            Username = "admin",
            PasswordHash = HashPassword("admin123"),
            Role = "admin",
            CreatedAt = DateTime.Now
        });
        Save("users.json", users);
    }
}

InitializeUsers();

//util file instead of program :sob:
bool IsAuthenticated(HttpContext context)
{
    var token = context.Request.Headers["Authorization"].FirstOrDefault();
    if (string.IsNullOrEmpty(token)) return false;

    var sessions = Load<Session>("sessions.json");
    return sessions.Any(s => s.Token == token && s.ExpiresAt > DateTime.Now);
}

// Please set endpoints into separate files. its goofy ahh to have them all in one file
// ============= AUTHENTICATION =============
app.MapPost("/auth/login", async (HttpContext context) =>
{
    var req = await context.Request.ReadFromJsonAsync<LoginRequest>();
    if (req == null) return Results.BadRequest("Invalid request");

    var users = Load<User>("users.json");
    var passwordHash = HashPassword(req.Password);
    var user = users.FirstOrDefault(u => u.Username == req.Username && u.PasswordHash == passwordHash);

    if (user == null)
        return Results.Unauthorized();

    var sessionToken = Guid.NewGuid().ToString();
    var sessions = Load<Session>("sessions.json");

    sessions.Add(new Session
    {
        Token = sessionToken,
        Username = user.Username,
        CreatedAt = DateTime.Now,
        ExpiresAt = DateTime.Now.AddHours(24)
    });

    Save("sessions.json", sessions);

    return Results.Ok(new
    {
        Token = sessionToken,
        Username = user.Username,
        Role = user.Role
    });
});

app.MapPost("/auth/logout", async (HttpContext context) =>
{
    var req = await context.Request.ReadFromJsonAsync<LogoutRequest>();
    if (req == null) return Results.BadRequest("Invalid request");

    var sessions = Load<Session>("sessions.json");
    sessions.RemoveAll(s => s.Token == req.Token);
    Save("sessions.json", sessions);
    return Results.Ok();
});

app.MapPost("/auth/validate", async (HttpContext context) =>
{
    var req = await context.Request.ReadFromJsonAsync<ValidateRequest>();
    if (req == null) return Results.BadRequest("Invalid request");

    var sessions = Load<Session>("sessions.json");
    var session = sessions.FirstOrDefault(s => s.Token == req.Token && s.ExpiresAt > DateTime.Now);

    if (session == null)
        return Results.Unauthorized();

    var users = Load<User>("users.json");
    var user = users.FirstOrDefault(u => u.Username == session.Username);

    if (user == null)
        return Results.Unauthorized();

    return Results.Ok(new
    {
        Username = user.Username,
        Role = user.Role
    });
});

// ============= USER MANAGEMENT =============
app.MapGet("/users", (HttpContext context) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var users = Load<User>("users.json");
    return Results.Ok(users.Select(u => new
    {
        u.Username,
        u.Role,
        u.CreatedAt
    }));
});

app.MapPost("/users", async (HttpContext context) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var req = await context.Request.ReadFromJsonAsync<CreateUserRequest>();
    if (req == null) return Results.BadRequest("Invalid request");

    var users = Load<User>("users.json");

    if (users.Any(u => u.Username == req.Username))
        return Results.BadRequest("Brugernavn findes allerede");

    var newUser = new User
    {
        Username = req.Username,
        PasswordHash = HashPassword(req.Password),
        Role = req.Role ?? "user",
        CreatedAt = DateTime.Now
    };

    users.Add(newUser);
    Save("users.json", users);

    return Results.Ok(new
    {
        newUser.Username,
        newUser.Role,
        newUser.CreatedAt
    });
});

app.MapDelete("/users/{username}", (HttpContext context, string username) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    if (username == "admin")
        return Results.BadRequest("Kan ikke slette admin brugeren");

    var users = Load<User>("users.json");
    users.RemoveAll(u => u.Username == username);
    Save("users.json", users);

    return Results.Ok();
});

// ============= PRODUKTER =============
app.MapGet("/products", (HttpContext context) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();
    return Results.Ok(Load<Product>("products.json"));
});

app.MapGet("/products/{id}", (HttpContext context, string id) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();
    var products = Load<Product>("products.json");
    var product = products.FirstOrDefault(p => p.Id == id);
    return product != null ? Results.Ok(product) : Results.NotFound();
});

app.MapPost("/products", async (HttpContext context) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var req = await context.Request.ReadFromJsonAsync<ProductRequest>();
    if (req == null) return Results.BadRequest("Invalid request");

    var products = Load<Product>("products.json");
    var inventory = Load<InventoryItem>("inventory.json");

    if (products.Any(p => p.Id == req.Id))
        return Results.BadRequest("Produkt ID findes allerede");

    var newProd = new Product
    {
        Id = req.Id,
        Name = req.Name,
        Category = req.Category,
        Location = req.Location,
        Description = req.Description,
        Barcode = req.Barcode,
        MinStock = req.MinStock
    };
    products.Add(newProd);

    inventory.Add(new InventoryItem
    {
        ProductId = req.Id,
        Quantity = req.InitialQty,
        Reserved = 0
    });

    Save("products.json", products);
    Save("inventory.json", inventory);
    return Results.Ok(newProd);
});

app.MapPut("/products/{id}", async (HttpContext context, string id) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var req = await context.Request.ReadFromJsonAsync<Product>();
    if (req == null) return Results.BadRequest("Invalid request");

    var products = Load<Product>("products.json");
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product == null) return Results.NotFound();

    product.Name = req.Name;
    product.Category = req.Category;
    product.Location = req.Location;
    product.Description = req.Description;
    product.Barcode = req.Barcode;
    product.MinStock = req.MinStock;

    Save("products.json", products);
    return Results.Ok(product);
});

app.MapDelete("/products/{id}", (HttpContext context, string id) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var products = Load<Product>("products.json");
    var inventory = Load<InventoryItem>("inventory.json");
    var picklists = Load<Picklist>("picklists.json");

    if (picklists.Any(pl => pl.Status == "active" && pl.Items.Any(i => i.ProductId == id)))
        return Results.BadRequest("Kan ikke slette - produkt er i aktiv plukliste");

    products.RemoveAll(p => p.Id == id);
    inventory.RemoveAll(i => i.ProductId == id);

    Save("products.json", products);
    Save("inventory.json", inventory);
    return Results.Ok();
});

// ============= KATEGORIER =============
app.MapGet("/categories", (HttpContext context) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();
    var products = Load<Product>("products.json");
    var categories = products.Select(p => p.Category).Distinct().Where(c => !string.IsNullOrEmpty(c)).ToList();
    return Results.Ok(categories);
});

// ============= INVENTORY =============
app.MapGet("/inventory", (HttpContext context) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();
    return Results.Ok(Load<InventoryItem>("inventory.json"));
});

app.MapGet("/inventory/low-stock", (HttpContext context) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var inventory = Load<InventoryItem>("inventory.json");
    var products = Load<Product>("products.json");

    var lowStock = from inv in inventory
                   join prod in products on inv.ProductId equals prod.Id
                   where inv.Quantity - inv.Reserved <= prod.MinStock
                   select new
                   {
                       ProductId = inv.ProductId,
                       Name = prod.Name,
                       Current = inv.Quantity - inv.Reserved,
                       MinStock = prod.MinStock
                   };

    return Results.Ok(lowStock);
});

app.MapPost("/inventory/{productId}/adjust", async (HttpContext context, string productId) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var adj = await context.Request.ReadFromJsonAsync<InventoryAdjustment>();
    if (adj == null) return Results.BadRequest("Invalid request");

    var inventory = Load<InventoryItem>("inventory.json");
    var item = inventory.FirstOrDefault(i => i.ProductId == productId);

    if (item == null) return Results.NotFound();

    item.Quantity += adj.Quantity;
    if (item.Quantity < 0) item.Quantity = 0;

    Save("inventory.json", inventory);
    return Results.Ok(item);
});

// ============= PLUKLISTER =============
app.MapGet("/picklists", (HttpContext context) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();
    return Results.Ok(Load<Picklist>("picklists.json"));
});

app.MapGet("/picklists/{id}", (HttpContext context, string id) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();
    var picklists = Load<Picklist>("picklists.json");
    var picklist = picklists.FirstOrDefault(p => p.Id == id);
    return picklist != null ? Results.Ok(picklist) : Results.NotFound();
});

app.MapPost("/picklists", async (HttpContext context) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var pl = await context.Request.ReadFromJsonAsync<Picklist>();
    if (pl == null) return Results.BadRequest("Invalid request");

    var picklists = Load<Picklist>("picklists.json");
    var inventory = Load<InventoryItem>("inventory.json");

    foreach (var item in pl.Items)
    {
        var inv = inventory.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (inv == null)
            return Results.BadRequest($"Produkt {item.ProductId} findes ikke");
        if ((inv.Quantity - inv.Reserved) < item.Qty)
            return Results.BadRequest($"Ikke nok p� lager af {item.ProductId}. Tilg�ngeligt: {inv.Quantity - inv.Reserved}");
    }

    foreach (var item in pl.Items)
    {
        var inv = inventory.First(i => i.ProductId == item.ProductId);
        inv.Reserved += item.Qty;
    }

    pl.CreatedAt = DateTime.Now;
    picklists.Add(pl);
    Save("picklists.json", picklists);
    Save("inventory.json", inventory);
    return Results.Ok(pl);
});

app.MapPost("/picklists/{id}/complete", (HttpContext context, string id) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var picklists = Load<Picklist>("picklists.json");
    var inventory = Load<InventoryItem>("inventory.json");
    var pl = picklists.FirstOrDefault(p => p.Id == id);

    if (pl == null) return Results.NotFound();
    if (pl.Status == "completed") return Results.BadRequest("Plukliste er allerede afsluttet");

    foreach (var item in pl.Items)
    {
        var inv = inventory.First(i => i.ProductId == item.ProductId);
        inv.Quantity -= item.Qty;
        inv.Reserved -= item.Qty;
    }

    pl.Status = "completed";
    pl.CompletedAt = DateTime.Now;
    Save("picklists.json", picklists);
    Save("inventory.json", inventory);
    return Results.Ok(pl);
});

app.MapPost("/picklists/{id}/cancel", (HttpContext context, string id) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var picklists = Load<Picklist>("picklists.json");
    var inventory = Load<InventoryItem>("inventory.json");
    var pl = picklists.FirstOrDefault(p => p.Id == id);

    if (pl == null) return Results.NotFound();
    if (pl.Status == "completed") return Results.BadRequest("Kan ikke annullere afsluttet plukliste");

    foreach (var item in pl.Items)
    {
        var inv = inventory.First(i => i.ProductId == item.ProductId);
        inv.Reserved -= item.Qty;
    }

    pl.Status = "cancelled";
    Save("picklists.json", picklists);
    Save("inventory.json", inventory);
    return Results.Ok(pl);
});

app.MapDelete("/picklists/{id}", (HttpContext context, string id) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var picklists = Load<Picklist>("picklists.json");
    var inventory = Load<InventoryItem>("inventory.json");
    var pl = picklists.FirstOrDefault(p => p.Id == id);

    if (pl == null) return Results.NotFound();

    if (pl.Status == "active")
    {
        foreach (var item in pl.Items)
        {
            var inv = inventory.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (inv != null) inv.Reserved -= item.Qty;
        }
        Save("inventory.json", inventory);
    }

    picklists.Remove(pl);
    Save("picklists.json", picklists);
    return Results.Ok();
});

// ============= STATISTIK =============
app.MapGet("/stats", (HttpContext context) =>
{
    if (!IsAuthenticated(context)) return Results.Unauthorized();

    var products = Load<Product>("products.json");
    var inventory = Load<InventoryItem>("inventory.json");
    var picklists = Load<Picklist>("picklists.json");

    return Results.Ok(new
    {
        TotalProducts = products.Count,
        TotalStock = inventory.Sum(i => i.Quantity),
        TotalReserved = inventory.Sum(i => i.Reserved),
        ActivePicklists = picklists.Count(p => p.Status == "active"),
        CompletedPicklists = picklists.Count(p => p.Status == "completed"),
        LowStockItems = inventory.Count(i => {
            var prod = products.FirstOrDefault(p => p.Id == i.ProductId);
            return prod != null && (i.Quantity - i.Reserved) <= prod.MinStock;
        })
    });
});

app.Run();

// please split classes into separate file :sob: on class per file
// ============= MODELS =============
public class User
{
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "user";
    public DateTime CreatedAt { get; set; }
}

public class Session
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

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

public class CreateUserRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? Role { get; set; }
}

public class Product
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Location { get; set; } = "";
    public string Description { get; set; } = "";
    public string Barcode { get; set; } = "";
    public int MinStock { get; set; } = 0;
}

public class ProductRequest
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Location { get; set; } = "";
    public string Description { get; set; } = "";
    public string Barcode { get; set; } = "";
    public int MinStock { get; set; } = 0;
    public int InitialQty { get; set; }
}

public class InventoryItem
{
    public string ProductId { get; set; } = "";
    public int Quantity { get; set; }
    public int Reserved { get; set; }
}

public class InventoryAdjustment
{
    public int Quantity { get; set; }
}

public class PickItem
{
    public string ProductId { get; set; } = "";
    public int Qty { get; set; }
}

public class Picklist
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "active";
    public List<PickItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Notes { get; set; } = "";
}