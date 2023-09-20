namespace Redis.gRPCService.Contracts
{
    public class RedisSettings
    {
        public string RedisConnectionString { get; set; }
        public bool SimulateRedis { get; set; }
        public string RedisServerHostIp { get; set; }
        public int RedisServerHostPort { get; set; }
        public string RedisInstanceName { get; set; }    
        public bool UseAzureKeyVault { get; set; }       
    }
}
