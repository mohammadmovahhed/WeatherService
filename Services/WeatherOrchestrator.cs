namespace WeatherService.Services;

public class WeatherOrchestrator
{
    private readonly ExternalWeatherService _externalWeatherService;
    private readonly DatabaseService _databaseService;
    private readonly ILogger<WeatherOrchestrator> _logger;

    public WeatherOrchestrator(
        ExternalWeatherService externalWeatherService,
        DatabaseService databaseService,
        ILogger<WeatherOrchestrator> logger)
    {
        _externalWeatherService = externalWeatherService;
        _databaseService = databaseService;
        _logger = logger;
    }

    public async Task<string?> GetWeatherDataAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing weather request for Lat: {Lat}, Lon: {Lon}", latitude, longitude);

        using var apiTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(1.5));
        using var apiLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, apiTimeoutCts.Token);

        string? freshData = null;
        try
        {
            freshData = await _externalWeatherService.GetWeatherRawDataAsync(latitude, longitude, apiLinkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("External API timed out (1.5s).");
        }

        if (freshData != null)
        {
            var isSaved = await _databaseService.SaveWeatherDataAsync(freshData, latitude, longitude, cancellationToken);

            if (isSaved)
            {
                _logger.LogInformation("Data fetched and saved successfully.");
                return freshData;
            }
            else
            {
                _logger.LogWarning("Data fetched but failed to save. Attempting fallback to cached data.");
                // اگر ذخیره خطا داد، به جای برگرداندن داده تازه، سراغ دیتابیس می‌رویم تا از داده‌های قبلی مطمئن شویم
            }
        }

        using var dbTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(1.5));
        using var dbLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, dbTimeoutCts.Token);

        try
        {
            _logger.LogWarning("Falling back to cached data for Lat: {Lat}, Lon: {Lon}", latitude, longitude);
            var cachedData = await _databaseService.GetLatestWeatherDataAsync(latitude, longitude, dbLinkedCts.Token);

            if (cachedData == null)
            {
                _logger.LogError("No data available. Internet is down and database is empty.");
            }

            return cachedData;
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Database fallback timed out (1.5s). Returning null.");
            return null;
        }
    }
}