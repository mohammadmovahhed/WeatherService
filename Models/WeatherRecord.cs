namespace WeatherService.Models;

public class WeatherRecord
{
    public int Id { get; set; }
    public string RawData { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}