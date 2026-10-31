using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherService.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherRecordCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WeatherRecords_Latitude_Longitude_CreatedAt",
                table: "WeatherRecords",
                columns: new[] { "Latitude", "Longitude", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeatherRecords_Latitude_Longitude_CreatedAt",
                table: "WeatherRecords");
        }
    }
}