using Microsoft.Extensions.Options;
using WeatherService.Dtos;

namespace WeatherService.Services;

public class ExternalWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiSettings _settings;
    private readonly ILogger<ExternalWeatherService> _logger;


    public ExternalWeatherService(HttpClient httpClient, IOptions<WeatherApiSettings> settings, ILogger<ExternalWeatherService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string?> GetWeatherRawDataAsync(double latitude, double longitude)
    {
        try
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

            var url = $"{_settings.BaseUrl}?latitude={latitude}&longitude={longitude}&current_weather=true"; //hourly=temperature_2m
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Weather data successfully fetched from API for Lat: {Lat}, Lon: {Lon}", latitude, longitude);
                return await response.Content.ReadAsStringAsync();
            }

            // اگر خطا داد (مثلاً به خاطر 30٪ قطعی اینترنت خودرو)
            _logger.LogWarning("API returned non-success status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Request to weather API timed out after {Timeout} seconds.", _settings.TimeoutSeconds);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while connecting to weather API.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ExternalWeatherService.");
            return null;
        }
    }
}