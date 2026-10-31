using Microsoft.EntityFrameworkCore;
using WeatherService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOptions<WeatherService.Dtos.WeatherApiSettings>()
    .Bind(builder.Configuration.GetSection("WeatherApi"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<WeatherService.Services.ExternalWeatherService>();
builder.Services.AddScoped<WeatherService.Services.DatabaseService>();
builder.Services.AddScoped<WeatherService.Services.WeatherOrchestrator>();

builder.Services.AddHostedService<WeatherService.BackgroundServices.DbCleanupService>();

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
    WeatherService.Services.WeatherOrchestrator orchestrator,
    HttpContext httpContext) =>
{
    if (lat.HasValue != lon.HasValue)
        return Results.BadRequest("Both lat and lon must be provided together.");

    if ((lat.HasValue && (lat < -90 || lat > 90)) || (lon.HasValue && (lon < -180 || lon > 180)))
        return Results.BadRequest("Invalid coordinates. Latitude must be between -90 and 90, Longitude between -180 and 180.");

    double latitude = lat ?? 52.52;
    double longitude = lon ?? 13.41;

    var cancellationToken = httpContext.RequestAborted;
    var data = await orchestrator.GetWeatherDataAsync(latitude, longitude, cancellationToken);
    if (data == null)
    {
        return Results.Ok((string?)null);
    }

    return Results.Content(data, "application/json");
})
.WithName("GetWeather");

app.Run();