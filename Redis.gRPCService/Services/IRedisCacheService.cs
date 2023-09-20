using Microsoft.Extensions.Caching.Distributed;

namespace Redis.gRPCService.Services
{
    public interface IRedisCacheService
    {
        Task<string> GetRecordAsync(string key);
        Task<bool> SetRecordAsync(string key, string data, TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null);
    }
}