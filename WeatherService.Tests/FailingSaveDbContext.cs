using Microsoft.EntityFrameworkCore;
using WeatherService.Data;

namespace WeatherService.Tests;

public class FailingSaveDbContext : AppDbContext
{
    public FailingSaveDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new Exception("Simulated save failure");
    }
}