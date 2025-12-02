using CartService.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("========================================");
Console.WriteLine("    CartService Starting...");
Console.WriteLine("========================================");

// Configuration Redis avec retry
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
Console.WriteLine($"?? Redis Connection String: {redisConnectionString}");

IConnectionMultiplexer? redis = null;
var maxRetries = 30;
var retryCount = 0;

while (retryCount < maxRetries && redis == null)
{
    try
    {
        retryCount++;
        Console.WriteLine($"?? Redis connection attempt {retryCount}/{maxRetries}...");

        var options = ConfigurationOptions.Parse(redisConnectionString);
        options.ConnectTimeout = 5000;
        options.SyncTimeout = 5000;
        options.AbortOnConnectFail = false;

        redis = ConnectionMultiplexer.Connect(options);

        if (redis.IsConnected)
        {
            Console.WriteLine("? Redis connected successfully!");

            // Test de ping
            var db = redis.GetDatabase();
            var pong = db.Ping();
            Console.WriteLine($"? Redis ping: {pong.TotalMilliseconds}ms");
        }
        else
        {
            redis = null;
            throw new Exception("Redis connection failed");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Redis connection attempt {retryCount} failed: {ex.Message}");

        if (retryCount >= maxRetries)
        {
            Console.WriteLine($"? FATAL: Failed to connect to Redis after {maxRetries} attempts");
            throw;
        }

        Console.WriteLine("? Waiting 2 seconds before retry...");
        Thread.Sleep(2000);
    }
}

builder.Services.AddSingleton(redis!);

// Services
builder.Services.AddScoped<CartManager>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

Console.WriteLine("========================================");
Console.WriteLine("    CartService Ready!");
Console.WriteLine("========================================");

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

Console.WriteLine("?? CartService listening on http://0.0.0.0:5236");
Console.WriteLine("?? Swagger UI: http://localhost:5236/swagger");

app.Run();