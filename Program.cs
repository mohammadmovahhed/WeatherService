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

app.Run();