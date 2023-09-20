using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using RedisDemo.gRPCService;
using System.Text.Json;

namespace Redis.ServiceBridge
{
    public static class RedisBridge
    {
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
            redisServerUrl = "192.168.10.3";
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

        public static void SetRecord<T>(string key, T data, TimeSpan absoluteExpireTime)
        {
            Channel redisChannel = new Channel(host: redisServerUrl, port: redisServerPort, credentials: ChannelCredentials.Insecure);
            RedisDemoService.RedisDemoServiceClient redisClient = new RedisDemoService.RedisDemoServiceClient(redisChannel);

            string redisData = JsonSerializer.Serialize(data);

            redisClient.SetRedisValue(new RedisSetRequest { Key = key, Data = redisData, AbsoluteExpireTime = Duration.FromTimeSpan(absoluteExpireTime) });
        }
    }
}
