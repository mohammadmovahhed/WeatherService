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

    public async Task<bool> SaveWeatherDataAsync(string rawData)
    {
        try
        {
            var record = new WeatherRecord
            {
                RawData = rawData,
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

    public async Task<string?> GetLatestWeatherDataAsync()
    {
        try
        {
            var latestRecord = await _context.WeatherRecords
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