using Redis.NET7.Contracts.Extensions;
using RedisDemo.gRPCService;

var builder = WebApplication.CreateBuilder(args);
var executablePath = Environment.CurrentDirectory + "\\";
var exeFullName = System.Reflection.Assembly.GetExecutingAssembly().Location;
var dirFullName = System.IO.Path.GetDirectoryName(exeFullName);


builder.Configuration.AddJsonFile($"{executablePath}appsettings.json", optional: true);


var redisGRPCServiceAddress = builder.Configuration.GetValue<string>("RedisGRPCServiceAddress");
var redisGRPCServicePort = builder.Configuration.GetValue<int>("RedisGRPCServicePort");

// Add services to the container.
builder.Services.AddApiServices();
builder.Services.AddControllers();
builder.Services.AddGrpcClient<RedisDemoService.RedisDemoServiceClient>(options =>
{
    options.Address = new Uri($"http://{redisGRPCServiceAddress}:{redisGRPCServicePort}");
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
