using AuthService.Data;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("========================================");
Console.WriteLine("    AuthService Starting...");
Console.WriteLine("========================================");

// Configuration de logging détaillé
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Lire la connection string
var connectionString = builder.Configuration.GetConnectionString("Default");
Console.WriteLine($"?? Connection String: {connectionString}");

// Configuration DB avec logs
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
    options.LogTo(Console.WriteLine, LogLevel.Information);
});

// Token service
builder.Services.AddScoped<ITokenService, TokenService>();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    Console.WriteLine("? JWT Key is missing!");
    throw new InvalidOperationException("JWT Key is missing in configuration");
}
Console.WriteLine($"?? JWT Key configured: YES (length: {jwtKey.Length})");

var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

Console.WriteLine("========================================");
Console.WriteLine("    Database Migration Process");
Console.WriteLine("========================================");

// Appliquer les migrations avec retry et logs détaillés
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>();

    var maxRetries = 30;
    var retryCount = 0;
    var connected = false;

    while (retryCount < maxRetries && !connected)
    {
        try
        {
            retryCount++;
            Console.WriteLine($"?? Connection attempt {retryCount}/{maxRetries}...");

            // Tester la connexion
            var canConnect = context.Database.CanConnect();
            Console.WriteLine($"? Database reachable: {canConnect}");

            if (!canConnect)
            {
                throw new Exception("Cannot connect to database");
            }

            connected = true;

            // Lister toutes les migrations disponibles
            var allMigrations = context.Database.GetMigrations().ToList();
            Console.WriteLine($"?? Total migrations in assembly: {allMigrations.Count}");
            foreach (var migration in allMigrations)
            {
                Console.WriteLine($"   ?? {migration}");
            }

            // Lister les migrations déjà appliquées
            var appliedMigrations = context.Database.GetAppliedMigrations().ToList();
            Console.WriteLine($"? Already applied migrations: {appliedMigrations.Count}");
            foreach (var migration in appliedMigrations)
            {
                Console.WriteLine($"   ? {migration}");
            }

            // Lister les migrations en attente
            var pendingMigrations = context.Database.GetPendingMigrations().ToList();
            Console.WriteLine($"? Pending migrations: {pendingMigrations.Count}");

            if (pendingMigrations.Any())
            {
                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine($"   ? {migration}");
                }

                Console.WriteLine("?? Applying pending migrations...");
                context.Database.Migrate();
                Console.WriteLine("? All migrations applied successfully!");
            }
            else
            {
                Console.WriteLine("? Database is up to date!");
            }

            // Vérifier que la table Users existe et est accessible
            try
            {
                var userCount = context.Users.Count();
                Console.WriteLine($"? Users table accessible (current count: {userCount})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"??  Cannot query Users table: {ex.Message}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Attempt {retryCount} failed: {ex.Message}");

            if (retryCount >= maxRetries)
            {
                Console.WriteLine($"? FATAL: Failed to connect after {maxRetries} attempts");
                Console.WriteLine($"? Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"? Message: {ex.Message}");
                Console.WriteLine($"? Stack Trace: {ex.StackTrace}");
                throw;
            }

            Console.WriteLine($"? Waiting 2 seconds before retry...");
            Thread.Sleep(2000);
        }
    }
}

Console.WriteLine("========================================");
Console.WriteLine("    AuthService Ready!");
Console.WriteLine("========================================");

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("?? AuthService listening on http://0.0.0.0:5112");
Console.WriteLine("?? Swagger UI: http://localhost:5112/swagger");

app.Run();