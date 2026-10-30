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

    public async Task<string?> GetWeatherDataAsync(double latitude, double longitude)
    {
        _logger.LogInformation("Processing weather request for Lat: {Lat}, Lon: {Lon}", latitude, longitude);

        //get from api.open-meteo.com and save in database
        var freshData = await _externalWeatherService.GetWeatherRawDataAsync(latitude, longitude);
        if (freshData != null)
        {
            await _databaseService.SaveWeatherDataAsync(freshData, latitude, longitude);
            return freshData;
        }

        // get last data from database
        _logger.LogWarning("Falling back to cached data due to network failure or save error.");

        var cachedData = await _databaseService.GetLatestWeatherDataAsync(latitude, longitude);
        if (cachedData == null)
            _logger.LogError("No data available. Internet is down and database is empty for this location Lat: {Lat}, Lon: {Lon}", latitude, longitude);

        return cachedData;
    }
}