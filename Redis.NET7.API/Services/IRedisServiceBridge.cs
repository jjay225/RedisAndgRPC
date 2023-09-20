namespace Redis.NET7.API.Services
{
    public interface IRedisServiceBridge
    {
        Task<T> GetRecordAsync<T>(string key);
        Task<bool> SetRecordAsync<T>(string key, T value);
    }
}