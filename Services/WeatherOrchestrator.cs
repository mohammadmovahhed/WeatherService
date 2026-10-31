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

        using var apiTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(1.5));
        using var apiLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, apiTimeoutCts.Token);

        string? freshData = null;
        try
        {
            freshData = await _externalWeatherService.GetWeatherRawDataAsync(latitude, longitude, apiLinkedCts.Token);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("External API budget (1.5s) exhausted.");
        }


        if (freshData != null)
        {
            using var saveTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(0.5));
            using var saveLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, saveTimeoutCts.Token);

            try
            {
                var isSaved = await _databaseService.SaveWeatherDataAsync(freshData, latitude, longitude, saveLinkedCts.Token);
                if (isSaved) 
                    return freshData;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Database save budget (0.5s) exhausted.");
            }
        }


        using var dbTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(1.0));
        using var dbLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, dbTimeoutCts.Token);

        try
        {
            _logger.LogWarning("Falling back to cached data for Lat: {Lat}, Lon: {Lon}", latitude, longitude);
            return await _databaseService.GetLatestWeatherDataAsync(latitude, longitude, dbLinkedCts.Token);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Database fallback budget (1.0s) exhausted. Returning null.");
            return null;
        }
    }
}