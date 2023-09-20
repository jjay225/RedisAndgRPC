using Microsoft.AspNetCore.Server.Kestrel.Core;
using Redis.gRPCService.Contracts;
using Redis.gRPCService.Interceptors;
using Redis.gRPCService.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var executablePath = Environment.CurrentDirectory + "\\";
var exeFullName = System.Reflection.Assembly.GetExecutingAssembly().Location;
var dirFullName = Path.GetDirectoryName(exeFullName);

builder.Configuration.AddJsonFile($"{executablePath}appsettings.json", optional: true, reloadOnChange: true);
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));
var redisSettings = builder.Configuration.GetSection("RedisSettings").Get<RedisSettings>();


builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.Http2.InitialConnectionWindowSize = 2 * 1024 * 1024 * 2; // 2 MB
    options.Limits.Http2.InitialStreamWindowSize = 1024 * 1024; // 1 MB;

    options.Listen(IPAddress.Parse(redisSettings.RedisServerHostIp), redisSettings.RedisServerHostPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });

});

builder.Services.AddScoped<IRedisCacheService, CacheService>();
builder.Services.AddGrpc()
.AddServiceOptions<RedisService>(options =>
{
    options.Interceptors.Add<ServerExceptionInterceptor>();
    options.EnableDetailedErrors = true;
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisSettings.RedisConnectionString;
    options.InstanceName = redisSettings.RedisInstanceName;
});

var app = builder.Build();

app.MapGrpcService<RedisService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
