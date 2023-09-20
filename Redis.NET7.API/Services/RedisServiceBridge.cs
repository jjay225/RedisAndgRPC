using Google.Protobuf.WellKnownTypes;
using RedisDemo.gRPCService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Redis.NET7.API.Services
{
    public class RedisServiceBridge : IRedisServiceBridge
    {
        private readonly RedisDemoService.RedisDemoServiceClient _redisDemoServiceClient;

        public RedisServiceBridge(RedisDemoService.RedisDemoServiceClient redisDemoServiceClient)
        {
            _redisDemoServiceClient = redisDemoServiceClient;
        }

        public async Task<T> GetRecordAsync<T>(string key)
        {
            var result = await _redisDemoServiceClient.GetRedisValueAsync(new RedisGetRequest { Key = key});
            if(result.Data == string.Empty) { return default; }  

            return JsonSerializer.Deserialize<T>(result.Data);            
        }

        public async Task<bool> SetRecordAsync<T>(string key, T value)
        {
            var serializedData = JsonSerializer.Serialize<T>(value);
            var timeSpan = TimeSpan.FromMinutes(5);
            var result = await _redisDemoServiceClient.SetRedisValueAsync(new RedisSetRequest
            {
                Key = key,
                Data = serializedData,
                AbsoluteExpireTime = Duration.FromTimeSpan(timeSpan)
            });

            return result.Successful;
        }
    }
}
