using Microsoft.EntityFrameworkCore;
using WeatherService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<WeatherService.Dtos.WeatherApiSettings>(builder.Configuration.GetSection("WeatherApi"));
builder.Services.AddHttpClient<WeatherService.Services.ExternalWeatherService>();
builder.Services.AddScoped<WeatherService.Services.DatabaseService>();
builder.Services.AddScoped<WeatherService.Services.WeatherOrchestrator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/weather", async (
    double? lat,
    double? lon,
    WeatherService.Services.WeatherOrchestrator orchestrator) =>
{
    double latitude = lat ?? 52.52;
    double longitude = lon ?? 13.41;

    var data = await orchestrator.GetWeatherDataAsync(latitude, longitude);
    if (data == null)
    {
        return Results.Ok((string?)null);
    }

    return Results.Content(data, "application/json");
})
.WithName("GetWeather");

app.Run();