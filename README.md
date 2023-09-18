# RedisAndgRPC
Redis example using a .NET 7 gRPC service as the service to that communicates to the Redis on Docker or Azure with .NET Framework and .NET 7 integrations

## How does it all work?

The core of this solution is the **_Redis.gRPCService_**, it is a .NET 7 gRPC that connects to Redis in either Docker or Azure Cache For Redis. The .NET 7 API testing project is **_Redis.NET7.API_**, the .NET Framework API testing project is **_Redis.Framework.API_** . Due to the fact the Protobuf doesn't implement generics, it has the "any" message type but it's not the same as Generics in c# I've implemented a "Bridge" to get the benefits of Generics and proper typing. 

The **_Redis.NET7.API_** project implements an interface **_IRedisServiceBridge_** where for instance you can do a Redis call to the gRPC service, it will send a string back and you will Deserialize to the type you need like so:

```
 public async Task<T> GetRecordAsync<T>(string key)
 {
     var result = await _redisDemoServiceClient.GetRedisValueAsync(new RedisGetRequest { Key = key});
     if(result.Data == string.Empty) { return default; }  

     return JsonSerializer.Deserialize<T>(result.Data);            
 }

```
**Calling code:**

```
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
**gRPC Redis Code**

```

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

