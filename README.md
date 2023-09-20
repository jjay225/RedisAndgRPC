# RedisAndgRPC
Redis example using a .NET 7 gRPC service as the service to that communicates to the Redis on Docker or Azure with .NET Framework and .NET 7 integrations. If you don't want to be left off the gRPC and Redis train this project will help you. 

## How does it all work?

The core of this solution is the **_Redis.gRPCService_**, it is a .NET 7 gRPC that connects to Redis in either Docker or Azure Cache For Redis. The .NET 7 API testing project is **_Redis.NET7.API_**, the .NET Framework API testing project is **_Redis.Framework.API_** . Due to the fact the Protobuf doesn't implement generics, it has the "any" message type but it's not the same as Generics in c# so I've implemented a "Bridge" to get the benefits of Generics and proper typing. 

Have the **_Redis.Framework.ServiceBridge_** and **_Redis.NET7.Contracts_** projects separate enables you to add them to any project you need with the complexities of adding proto and gRPC nuget packages to each project you want to use Redis and gRPC in.

The **_Redis.NET7.API_** project implements an interface **_IRedisServiceBridge_** where you can do a Redis call to the gRPC service, it will send a string back and you will Deserialize to the type you need like so:

**.NET 7 Bridge code of IRedisServiceBridge.cs:**

```cs
 public async Task<T> GetRecordAsync<T>(string key)
 {
     var result = await _redisDemoServiceClient.GetRedisValueAsync(new RedisGetRequest { Key = key});
     if(result.Data == string.Empty) { return default; }  

     return JsonSerializer.Deserialize<T>(result.Data);            
 }

```

**Calling code .NET 7 to the Bridge code:**

```cs
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

```

**.NET Framework Bridge code of RedisBridge.cs:**

```cs
private static string redisServerUrl = "";
private static int redisServerPort = 0;

static RedisBridge()
{

    
    /*if (Environment.GetEnvironmentVariable("RedisGRPCServiceAddress") == null)
    {
        throw new ArgumentNullException(nameof(redisServerUrl));
    }

    if (Environment.GetEnvironmentVariable("RedisGRPCServicePort") == null)
    {
        throw new ArgumentNullException(nameof(redisServerPort));
    }

    redisServerUrl = Environment.GetEnvironmentVariable("RedisGRPCServiceAddress");
    redisServerPort = int.Parse(Environment.GetEnvironmentVariable("RedisGRPCServicePort"));*/

    redisServerUrl = "192.168.1.1";
    redisServerPort = 9100;

}

public static T GetRecord<T>(string key)
{

    Channel redisChannel = new Channel(host: redisServerUrl, port: redisServerPort, credentials: ChannelCredentials.Insecure);

    RedisDemoService.RedisDemoServiceClient redisClient = new RedisDemoService.RedisDemoServiceClient(redisChannel);

    RedisGetResponse redisResponse = redisClient.GetRedisValue(new RedisGetRequest { Key = key });

    if (string.IsNullOrEmpty(redisResponse.Data))
        return default;

    return JsonSerializer.Deserialize<T>(redisResponse.Data);

}
```

Now of course there are a few other ways one can set the address of the gRPC Redis service for .NET Framework: Pass in the URL, have an environment variable, up to you.

**Calling code from .NET Framework:**

```cs
  // GET api/values
  public IEnumerable<string> Get()
  {
      var randomKey = new Random().Next(1, 4);
      var redisResults = RedisBridge.GetRecord<IEnumerable<string>>($"FrameWorkKey:{randomKey}");
      var resultsLocal = new string[] { "value1", "value2" };
      if (redisResults != null)
      {
          return redisResults;
      }

      RedisBridge.SetRecord($"FrameWorkKey:{randomKey}", redisResults, TimeSpan.FromMinutes(5));

      return resultsLocal;
  }

```

**gRPC .NET 7 Redis Code:**

```cs
  public async Task<string> GetRecordAsync(string key)
  {
      string? returnValue = null;

      try
      {                
          return await _retryPolicy.ExecuteAsync(async () =>
          {
              returnValue = await _cache.GetStringAsync(key);
              return returnValue ?? "";
          });
      }
      catch (Exception ex)
      {
          //Other exceptions, not RedisConnectionException and SocketException
          _logger.LogInformation("Exception in {getMethod}, exception detail: {exception}", nameof(GetRecordAsync), ex.Message);
          return returnValue ?? "";
      }
  }

```

As you will see the great Polly package is used here as well. So there you have it, calling a Redis gRPC service from .NET Framework and .NET 7. **NOTE:** This is a pretty basic example, any tweaks you may need for production code will need to be done.




