namespace WeatherService.Services;

public class WeatherOrchestrator
{
    private readonly ExternalWeatherService _externalWeatherService;
    private readonly DatabaseService _databaseService;
    private readonly ILogger<WeatherOrchestrator> _logger;

    public WeatherOrchestrator(ExternalWeatherService externalWeatherService, DatabaseService databaseService, ILogger<WeatherOrchestrator> logger)
    {
        _externalWeatherService = externalWeatherService;
        _databaseService = databaseService;
        _logger = logger;
    }

    public async Task<string?> GetWeatherDataAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing weather request for Lat: {Lat}, Lon: {Lon}", latitude, longitude);

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var linkedToken = linkedCts.Token;

        try
        {
            //get from api.open-meteo.com and save in database
            var freshData = await _externalWeatherService.GetWeatherRawDataAsync(latitude, longitude, linkedToken);
            if (freshData != null)
            {
                await _databaseService.SaveWeatherDataAsync(freshData, latitude, longitude, linkedToken);
                return freshData;
            }

            // get last data from database
            _logger.LogWarning("Falling back to cached data due to network failure or save error.");

            var cachedData = await _databaseService.GetLatestWeatherDataAsync(latitude, longitude, linkedToken);
            if (cachedData == null)
                _logger.LogError("No data available. Internet is down and database is empty for this location Lat: {Lat}, Lon: {Lon}", latitude, longitude);

            return cachedData;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation cancelled or timed out (3 seconds limit reached).");

            // تلاش نهایی برای خواندن از دیتابیس (با یک توکن جدید بدون محدودیت زمانی)
            // تا اگر دیتابیس داده دارد، حتماً به کلاینت برگردانیم
            var fallbackData = await _databaseService.GetLatestWeatherDataAsync(latitude, longitude, CancellationToken.None);
            return fallbackData;
        }
    }
}