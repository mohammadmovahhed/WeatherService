using Microsoft.Extensions.Options;
using WeatherService.Dtos;

namespace WeatherService.Services;

public class ExternalWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiSettings _settings;

    public ExternalWeatherService(HttpClient httpClient, IOptions<WeatherApiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<string?> GetWeatherRawDataAsync(double latitude, double longitude)
    {
        try
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

            var url = $"{_settings.BaseUrl}?latitude={latitude}&longitude={longitude}&current_weather=true"; //hourly=temperature_2m
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            // اگر خطا داد (مثلاً به خاطر 30٪ قطعی اینترنت خودرو)
            return null;
        }
        catch
        {
            // در صورت بروز هرگونه خطای شبکه، Timeout یا قطعی اینترنت
            return null;
        }
    }
}