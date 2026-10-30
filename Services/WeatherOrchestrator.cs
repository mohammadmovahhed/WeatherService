namespace WeatherService.Services;

public class WeatherOrchestrator
{
    private readonly ExternalWeatherService _externalWeatherService;
    private readonly DatabaseService _databaseService;

    public WeatherOrchestrator(ExternalWeatherService externalWeatherService, DatabaseService databaseService)
    {
        _externalWeatherService = externalWeatherService;
        _databaseService = databaseService;
    }

    public async Task<string?> GetWeatherDataAsync(double latitude, double longitude)
    {
        //get from api.open-meteo.com and save in database
        var freshData = await _externalWeatherService.GetWeatherRawDataAsync(latitude, longitude);
        if (freshData != null)
        {
            await _databaseService.SaveWeatherDataAsync(freshData, latitude, longitude);
            return freshData;
        }

        // get last data from database
        var cachedData = await _databaseService.GetLatestWeatherDataAsync(latitude, longitude);
        return cachedData;
    }
}