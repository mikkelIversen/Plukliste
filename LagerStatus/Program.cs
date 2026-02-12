using WarehouseAPI.Services;
using WarehouseAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
});

// Add services
builder.Services.AddCors(o => o.AddDefaultPolicy(p => 
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddSingleton<IDataService, JsonDataService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<IInventoryService, InventoryService>();
builder.Services.AddSingleton<IPicklistService, PicklistService>();
builder.Services.AddSingleton<INotesService, NotesService>();
builder.Services.AddSingleton<IStatsService, StatsService>();

var app = builder.Build();

// Configure middleware
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();

// Initialize default data
var userService = app.Services.GetRequiredService<IUserService>();
userService.InitializeDefaultAdmin();

// Register endpoints
app.RegisterAuthEndpoints();
app.RegisterUserEndpoints();
app.RegisterProductEndpoints();
app.RegisterInventoryEndpoints();
app.RegisterPicklistEndpoints();
app.RegisterNotesEndpoints();
app.RegisterStatsEndpoints();

app.Run();
