using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherService.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinatesToWeatherRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "WeatherRecords",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "WeatherRecords",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "WeatherRecords");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "WeatherRecords");
        }
    }
}