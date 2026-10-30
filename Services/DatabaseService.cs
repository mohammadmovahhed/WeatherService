using Microsoft.EntityFrameworkCore;
using WeatherService.Data;
using WeatherService.Models;

namespace WeatherService.Services;

public class DatabaseService
{
    private readonly AppDbContext _context;
    public DatabaseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SaveWeatherDataAsync(string rawData, double latitude, double longitude)
    {
        try
        {
            var record = new WeatherRecord
            {
                RawData = rawData,
                Latitude = latitude,
                Longitude = longitude,
                CreatedAt = DateTime.UtcNow
            };

            _context.WeatherRecords.Add(record);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetLatestWeatherDataAsync(double latitude, double longitude)
    {
        try
        {
            var latestRecord = await _context.WeatherRecords
                .Where(r => r.Latitude == latitude && r.Longitude == longitude)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            return latestRecord?.RawData;
        }
        catch
        {
            return null;
        }
    }
}