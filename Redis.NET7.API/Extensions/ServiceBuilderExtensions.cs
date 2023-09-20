
using Microsoft.Extensions.DependencyInjection;
using Redis.NET7.API.Services;

namespace Redis.NET7.Contracts.Extensions
{
    public static class ServiceBuilderExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddScoped<IRedisServiceBridge, RedisServiceBridge>();           
         
            return services;
        }
    }
}