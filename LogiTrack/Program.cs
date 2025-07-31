
using System.Text;
using LogiTrack.Context;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var root = builder.Environment.ContentRootPath;
Console.WriteLine($"Root path: {root}");
var dotEnv = Path.Combine(root, ".env");
if (File.Exists(dotEnv))
{
    Console.WriteLine($"Loading environment variables from {dotEnv}");
    DotNetEnv.Env.Load(dotEnv);

    builder.Configuration.AddEnvironmentVariables();
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<LogiTrackContext>();

// Add this after your other service configurations
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        //options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64; // Increase max depth if needed
    });

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<LogiTrackContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWT:Issuer"),
        ValidAudience = Environment.GetEnvironmentVariable("JWT:Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT:Key") ?? "YourSuperSecretKeyHere123456789012345"))
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LogiTrackContext>();

    context.Database.EnsureCreated();

    // Add test inventory item if none exist
    if (!await context.InventoryItems.AnyAsync())
    {
        var newItem = new InventoryItem
        {
            Name = "Pallet Jack",
            Quantity = 12,
            Location = "Warehouse A"
        };

        Console.WriteLine("Don't found any of them");
        await context.InventoryItems.AddAsync(newItem);
        await context.SaveChangesAsync();
    }

    if(!await context.Orders.AnyAsync())
    {
        Console.WriteLine("Don't found any orders");
        var order = new Order
        {
            CustomerName = "Acme Corp",
            DatePlaced = DateTime.Now
        };

        var getItem = await context.InventoryItems.FirstOrDefaultAsync();

        if (getItem != null)
        {
            order.AddItem(getItem, 2);
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();
            // Console.WriteLine(order.GetOrderSummary());
        }
    }

    // Retrieve and print inventory to confirm
    var items = context.InventoryItems.ToList();
    foreach (var item in items)
    {
        item.DisplayInfo(); // Should print: Item: Pallet Jack | Quantity: 12 | Location: Warehouse A
    }
    
    foreach (var ord in context.Orders.ToList())
    {
        Console.WriteLine(ord.GetOrderSummary());
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //Console.WriteLine("DEV MODE");
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "LogiTrack API Test";
        options.Theme = ScalarTheme.Default;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options.ShowSidebar = true;
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();