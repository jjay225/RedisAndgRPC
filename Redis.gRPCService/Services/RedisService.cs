using Grpc.Core;
using Microsoft.Extensions.Options;
using Redis.gRPCService.Contracts;
using RedisDemo.gRPCService;

namespace Redis.gRPCService.Services
{
    public class RedisService : RedisDemoService.RedisDemoServiceBase
    {
        private readonly IRedisCacheService _redisCacheService;
        private readonly RedisSettings _redisSettings;

        public RedisService(
            IRedisCacheService redisCacheService,
            IOptions<RedisSettings> redisSettings)
        {
            _redisCacheService = redisCacheService;
            _redisSettings = redisSettings.Value;
        }
        public override async Task<RedisGetResponse> GetRedisValue(RedisGetRequest request, ServerCallContext context)
        {
            if (_redisSettings.SimulateRedis)
            {
                return new RedisGetResponse
                {
                        Data = ""
                };
            }

            var response = await _redisCacheService.GetRecordAsync(key: request.Key);
            var redisResponse = new RedisGetResponse
            {
                Data = response
            };

            return redisResponse;
        }

        public override async Task<RedisSetResponse> SetRedisValue(RedisSetRequest request, ServerCallContext context)
        {
            if (_redisSettings.SimulateRedis)
            {
                return new RedisSetResponse
                {
                    Successful = true
                };
            }

            var timeSpan = request.AbsoluteExpireTime.ToTimeSpan();
            var setResult = await _redisCacheService.SetRecordAsync(key: request.Key, data: request.Data, absoluteExpireTime: timeSpan, unusedExpireTime: null);

            return new RedisSetResponse
            {
                Successful = setResult
            };
        }
    }
}
