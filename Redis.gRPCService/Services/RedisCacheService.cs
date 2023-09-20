using Microsoft.Extensions.Caching.Distributed;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using System.Net.Sockets;

namespace Redis.gRPCService.Services
{
    public class CacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public CacheService(
            IDistributedCache cache,
            ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
            _retryPolicy = Policy.Handle<RedisConnectionException>()
                                 .Or<SocketException>()
                                 .WaitAndRetryAsync(
                                    retryCount: 1,
                                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                    onRetry: (exception, retryCount, context) =>
                                    {
                                        _logger.LogInformation(
                                            "Retry {retryCount} due to {exceptionType}, exception detail: {exception}",
                                            retryCount,
                                            exception.GetType().Name,
                                            exception.Message);
                                    });
        }

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

        public async Task<bool> SetRecordAsync(string key, string data, TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(5),
                    SlidingExpiration = unusedExpireTime
                };

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await _cache.SetStringAsync(key, data, options);
                });

                return true;
            }
            catch (Exception ex)
            {
                //Other exceptions, not RedisConnectionException and SocketException
                _logger.LogInformation("Exception in {setMethod}, exception detail: {exception}", nameof(SetRecordAsync), ex.Message);
                return false;
            }
        }
    }

}
