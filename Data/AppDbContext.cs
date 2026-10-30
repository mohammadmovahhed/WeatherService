using Microsoft.EntityFrameworkCore;
using WeatherService.Models;

namespace WeatherService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<WeatherRecord> WeatherRecords { get; set; }
    }
}