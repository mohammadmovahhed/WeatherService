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

    public async Task<string?> GetWeatherRawDataAsync()
    {
        try
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

            // درخواست به API (به عنوان مثال برای تهران)
            // نکته: داده‌ها به صورت رشته (String) دریافت می‌شوند تا بدون تغییر باقی بمانند
            var response = await _httpClient.GetAsync(_settings.BaseUrl + "?latitude=35.6892&longitude=51.3890&current_weather=true");

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