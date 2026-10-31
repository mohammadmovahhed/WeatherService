using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using WeatherService.Data;
using WeatherService.Dtos;
using WeatherService.Models;
using WeatherService.Services;

namespace WeatherService.Tests
{
    public class WeatherServiceTests
    {
        private readonly Mock<ILogger<ExternalWeatherService>> _mockExtLogger;
        private readonly Mock<ILogger<DatabaseService>> _mockDbLogger;
        private readonly Mock<ILogger<WeatherOrchestrator>> _mockOrchLogger;
        private readonly WeatherApiSettings _apiSettings;

        public WeatherServiceTests()
        {
            _mockExtLogger = new Mock<ILogger<ExternalWeatherService>>();
            _mockDbLogger = new Mock<ILogger<DatabaseService>>();
            _mockOrchLogger = new Mock<ILogger<WeatherOrchestrator>>();
            _apiSettings = new WeatherApiSettings { BaseUrl = "https://api.test.com", TimeoutSeconds = 3 };
        }

        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetWeatherDataAsync_WhenNetworkFails_ReturnsCachedData()
        {
            var context = GetInMemoryDbContext();

            var dbService = new DatabaseService(context, _mockDbLogger.Object);
            await dbService.SaveWeatherDataAsync("{\"temp\": 25}", 35.0, 51.0, CancellationToken.None);

            var mockHttpHandler = new Mock<HttpMessageHandler>();
            mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Simulated network failure"));

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var extService = new ExternalWeatherService(httpClient, Options.Create(_apiSettings), _mockExtLogger.Object);

            var orchestrator = new WeatherOrchestrator(extService, dbService, _mockOrchLogger.Object);

            var result = await orchestrator.GetWeatherDataAsync(35.0, 51.0, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("{\"temp\": 25}", result);
        }

        [Fact]
        public async Task GetWeatherDataAsync_WhenNetworkAndDatabaseFail_ReturnsNull()
        {
            var context = GetInMemoryDbContext();
            var dbService = new DatabaseService(context, _mockDbLogger.Object);

            var mockHttpHandler = new Mock<HttpMessageHandler>();
            mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Simulated network failure"));

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var extService = new ExternalWeatherService(httpClient, Options.Create(_apiSettings), _mockExtLogger.Object);

            var orchestrator = new WeatherOrchestrator(extService, dbService, _mockOrchLogger.Object);

            var result = await orchestrator.GetWeatherDataAsync(40.0, 50.0, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetLatestWeatherDataAsync_ReturnsLatestDataForSpecificCoordinates()
        {
            var context = GetInMemoryDbContext();
            var dbService = new DatabaseService(context, _mockDbLogger.Object);

            await dbService.SaveWeatherDataAsync("{\"data\": \"old\"}", 35.0, 51.0, CancellationToken.None);
            await dbService.SaveWeatherDataAsync("{\"data\": \"new\"}", 40.0, 60.0, CancellationToken.None);

            var result = await dbService.GetLatestWeatherDataAsync(40.0, 60.0, CancellationToken.None);

            Assert.Equal("{\"data\": \"new\"}", result);
        }

        [Fact]
        public async Task GetWeatherDataAsync_WhenSaveFailsButCacheExists_ReturnsCachedData()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // استفاده از یک کانتکست خاص که فقط زمان ذخیره خطا می‌دهد
            var context = new FailingSaveDbContext(options);
            var dbService = new DatabaseService(context, _mockDbLogger.Object);

            // ذخیره دستی یک داده در کش
            context.WeatherRecords.Add(new WeatherRecord { RawData = "{\"temp\": 20}", Latitude = 35.0, Longitude = 51.0, CreatedAt = DateTime.UtcNow });
            context.SaveChanges();

            // شبیه‌سازی API موفق
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent("{\"temp\": 30}") });

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var extService = new ExternalWeatherService(httpClient, Options.Create(_apiSettings), _mockExtLogger.Object);

            var orchestrator = new WeatherOrchestrator(extService, dbService, _mockOrchLogger.Object);

            var result = await orchestrator.GetWeatherDataAsync(35.0, 51.0, CancellationToken.None);

            // بررسی: باید داده کش شده قبلی (20) را برگرداند نه داده جدید (30)
            Assert.Equal("{\"temp\": 20}", result);
        }
    }
}