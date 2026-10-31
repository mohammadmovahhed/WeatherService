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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ایندکس ترکیبی برای جستجوی سریع مختصات و زمان
            modelBuilder.Entity<WeatherRecord>()
                .HasIndex(r => new { r.Latitude, r.Longitude, r.CreatedAt });
        }
    }
}