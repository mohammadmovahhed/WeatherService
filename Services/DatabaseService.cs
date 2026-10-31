using Microsoft.EntityFrameworkCore;
using WeatherService.Data;
using WeatherService.Models;

namespace WeatherService.Services;

public class DatabaseService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseService> _logger;
    public DatabaseService(AppDbContext context, ILogger<DatabaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> SaveWeatherDataAsync(string rawData, double latitude, double longitude, CancellationToken cancellationToken)
    {
        try
        {
            var record = new WeatherRecord
            {
                RawData = rawData,
                Latitude = Math.Round(latitude, 3),     //به سه رقم محدود محدود میکنیم (دقت حدود 100 متر میشه)
                Longitude = Math.Round(longitude, 3),
                CreatedAt = DateTime.UtcNow
            };

            _context.WeatherRecords.Add(record);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Weather data saved to database for Lat: {Lat}, Lon: {Lon}", latitude, longitude);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Database save operation was canceled.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save weather data to database.");
            return false;
        }
    }

    public async Task<string?> GetLatestWeatherDataAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        try
        {
            var searchLat = Math.Round(latitude, 3);    //به سه رقم محدود محدود میکنیم (دقت حدود 100 متر میشه)
            var searchLon = Math.Round(longitude, 3);

            var latestRecord = await _context.WeatherRecords
                .Where(r => r.Latitude == searchLat && r.Longitude == searchLon)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestRecord == null)
            {
                _logger.LogInformation("No cached weather data found for Lat: {Lat}, Lon: {Lon}", latitude, longitude);
            }
            else
            {
                _logger.LogInformation("Cached weather data retrieved for Lat: {Lat}, Lon: {Lon}", latitude, longitude);
            }

            return latestRecord?.RawData;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve weather data from database.");
            return null;
        }
    }
}