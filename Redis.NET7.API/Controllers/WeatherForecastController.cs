using Microsoft.AspNetCore.Mvc;
using Redis.NET7.API.Services;

namespace Redis.NET7.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IRedisServiceBridge _redisServiceBridge;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            IRedisServiceBridge redisServiceBridge)
        {
            _logger = logger;
            _redisServiceBridge = redisServiceBridge;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var randomKey = new Random().Next(1,4);
            var redisData = await _redisServiceBridge.GetRecordAsync<IEnumerable<WeatherForecast>>($"Net7Key:{randomKey}");

            if(redisData == null)
            {
                var weatherData = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();

                await _redisServiceBridge.SetRecordAsync<IEnumerable<WeatherForecast>>($"Net7Key:{randomKey}", weatherData);
            }

            return redisData;
        }
    }
}
