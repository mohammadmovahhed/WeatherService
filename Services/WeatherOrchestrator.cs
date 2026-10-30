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

    public async Task<string?> GetWeatherDataAsync()
    {
        //get from api.open-meteo.com and save in database
        var freshData = await _externalWeatherService.GetWeatherRawDataAsync();
        if (freshData != null)
        {
            await _databaseService.SaveWeatherDataAsync(freshData);
            return freshData;
        }

        // get last data from database
        var cachedData = await _databaseService.GetLatestWeatherDataAsync();
        return cachedData;
    }
}