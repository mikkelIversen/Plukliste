using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use port 5000 with HTTP only
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();

// Serve static files (index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

string dataPath = "Data";
Directory.CreateDirectory(dataPath);

List<T> Load<T>(string file)
{
    var path = Path.Combine(dataPath, file);
    if (!File.Exists(path)) return new List<T>();
    return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path)) ?? new List<T>();
}

void Save<T>(string file, List<T> data)
{
    var path = Path.Combine(dataPath, file);
    File.WriteAllText(path, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
}

// ============= PRODUKTER =============
app.MapGet("/products", () => Load<Product>("products.json"));

app.MapGet("/products/{id}", (string id) => {
    var products = Load<Product>("products.json");
    var product = products.FirstOrDefault(p => p.Id == id);
    return product != null ? Results.Ok(product) : Results.NotFound();
});

app.MapPost("/products", ([FromBody] ProductRequest req) => {
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

app.MapPut("/products/{id}", (string id, [FromBody] Product req) => {
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

app.MapDelete("/products/{id}", (string id) => {
    var products = Load<Product>("products.json");
    var inventory = Load<InventoryItem>("inventory.json");
    var picklists = Load<Picklist>("picklists.json");

    // Tjek om produkt er i aktive pluklister
    if (picklists.Any(pl => pl.Status == "active" && pl.Items.Any(i => i.ProductId == id)))
        return Results.BadRequest("Kan ikke slette - produkt er i aktiv plukliste");

    products.RemoveAll(p => p.Id == id);
    inventory.RemoveAll(i => i.ProductId == id);

    Save("products.json", products);
    Save("inventory.json", inventory);
    return Results.Ok();
});

// ============= KATEGORIER =============
app.MapGet("/categories", () => {
    var products = Load<Product>("products.json");
    var categories = products.Select(p => p.Category).Distinct().Where(c => !string.IsNullOrEmpty(c)).ToList();
    return Results.Ok(categories);
});

// ============= INVENTORY =============
app.MapGet("/inventory", () => Load<InventoryItem>("inventory.json"));

app.MapGet("/inventory/low-stock", () => {
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

app.MapPost("/inventory/{productId}/adjust", (string productId, [FromBody] InventoryAdjustment adj) => {
    var inventory = Load<InventoryItem>("inventory.json");
    var item = inventory.FirstOrDefault(i => i.ProductId == productId);

    if (item == null) return Results.NotFound();

    item.Quantity += adj.Quantity;
    if (item.Quantity < 0) item.Quantity = 0;

    Save("inventory.json", inventory);
    return Results.Ok(item);
});

// ============= PLUKLISTER =============
app.MapGet("/picklists", () => Load<Picklist>("picklists.json"));

app.MapGet("/picklists/{id}", (string id) => {
    var picklists = Load<Picklist>("picklists.json");
    var picklist = picklists.FirstOrDefault(p => p.Id == id);
    return picklist != null ? Results.Ok(picklist) : Results.NotFound();
});

app.MapPost("/picklists", ([FromBody] Picklist pl) => {
    var picklists = Load<Picklist>("picklists.json");
    var inventory = Load<InventoryItem>("inventory.json");

    // Valider alle items
    foreach (var item in pl.Items)
    {
        var inv = inventory.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (inv == null)
            return Results.BadRequest($"Produkt {item.ProductId} findes ikke");
        if ((inv.Quantity - inv.Reserved) < item.Qty)
            return Results.BadRequest($"Ikke nok på lager af {item.ProductId}. Tilgængeligt: {inv.Quantity - inv.Reserved}");
    }

    // Reserver alle items
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

app.MapPost("/picklists/{id}/complete", (string id) => {
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

app.MapPost("/picklists/{id}/cancel", (string id) => {
    var picklists = Load<Picklist>("picklists.json");
    var inventory = Load<InventoryItem>("inventory.json");
    var pl = picklists.FirstOrDefault(p => p.Id == id);

    if (pl == null) return Results.NotFound();
    if (pl.Status == "completed") return Results.BadRequest("Kan ikke annullere afsluttet plukliste");

    // Frigør reserverede items
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

app.MapDelete("/picklists/{id}", (string id) => {
    var picklists = Load<Picklist>("picklists.json");
    var inventory = Load<InventoryItem>("inventory.json");
    var pl = picklists.FirstOrDefault(p => p.Id == id);

    if (pl == null) return Results.NotFound();

    // Hvis aktiv, frigør reservationer først
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
app.MapGet("/stats", () => {
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

// ============= MODELS =============
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