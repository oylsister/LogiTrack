using LogiTrack.Context;
using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<LogiTrackContext>();

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

    var order = new Order
    {
        CustomerName = "Acme Corp",
        DatePlaced = DateTime.Now
    };

    var getItem = await context.InventoryItems.FirstOrDefaultAsync();

    if (getItem != null)
    {
        order.AddItem(getItem);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();
        Console.WriteLine(order.GetOrderSummary());
    }

    // Retrieve and print inventory to confirm
        var items = context.InventoryItems.ToList();
    foreach (var item in items)
    {
        item.DisplayInfo(); // Should print: Item: Pallet Jack | Quantity: 12 | Location: Warehouse A
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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
