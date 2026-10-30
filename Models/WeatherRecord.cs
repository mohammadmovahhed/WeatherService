namespace WeatherService.Models;

public class WeatherRecord
{
    public int Id { get; set; }
    public string RawData { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}