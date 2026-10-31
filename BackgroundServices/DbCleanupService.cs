using Microsoft.EntityFrameworkCore;
using WeatherService.Data;

namespace WeatherService.BackgroundServices;

public class DbCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbCleanupService> _logger;

    public DbCleanupService(IServiceProvider serviceProvider, ILogger<DbCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                //پیدا کردن رکوردهای قدیمی‌تر از ۲۴ ساعت
                var cutoffDate = DateTime.UtcNow.AddHours(-24);

                var oldRecords = await context.WeatherRecords
                    .Where(r => r.CreatedAt < cutoffDate)
                    .ToListAsync(stoppingToken);

                if (oldRecords.Any())
                {
                    context.WeatherRecords.RemoveRange(oldRecords);
                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Database cleanup: Removed {Count} old weather records.", oldRecords.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during database cleanup.");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}