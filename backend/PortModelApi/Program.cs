using PortModelApi.Data;
using Microsoft.EntityFrameworkCore;
using PortModelApi.Services;
using Polly;
using Polly.CircuitBreaker;

var builder = WebApplication.CreateBuilder(args);

// Configure default application timezone (resolve IANA and Windows ids, fallback to UTC)
var configuredTzId = builder.Configuration["AppSettings:TimeZone"] ?? "Asia/Bangkok";
TimeZoneInfo ResolveTz(string id)
{
    if (string.IsNullOrWhiteSpace(id)) return TimeZoneInfo.Utc;
    try
    {
        return TimeZoneInfo.FindSystemTimeZoneById(id);
    }
    catch (TimeZoneNotFoundException)
    {
        // Try common alternate for Bangkok
        if (string.Equals(id, "Asia/Bangkok", StringComparison.OrdinalIgnoreCase))
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); } catch { }
        }
        if (string.Equals(id, "SE Asia Standard Time", StringComparison.OrdinalIgnoreCase))
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok"); } catch { }
        }
        return TimeZoneInfo.Utc;
    }
    catch (InvalidTimeZoneException)
    {
        return TimeZoneInfo.Utc;
    }
}

var appTimeZone = ResolveTz(configuredTzId);
// For Linux containers, set TZ environment variable to IANA id when possible
try { Environment.SetEnvironmentVariable("TZ", "Asia/Bangkok"); } catch { }
builder.Services.AddSingleton(appTimeZone);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Add Authentication (Keycloak)
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Keycloak:Authority"];
    options.Audience = builder.Configuration["Keycloak:ClientId"];
    options.RequireHttpsMetadata = false; // Dev only
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Keycloak:Authority"]
    };
});

// Enable CORS
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSvelteKit",
        policy =>
        {
            policy.WithOrigins(allowedOrigins.Split(',').Select(o => o.Trim()).ToArray())
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Allow cookies/auth tokens
        });
});

// Register IHttpContextAccessor so controllers that take it in ctor can be activated.
builder.Services.AddHttpContextAccessor();

// Register ColumnLengthProvider so IColumnLengthProvider can be injected.
builder.Services.AddScoped<IColumnLengthProvider, ColumnLengthProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Apply pending migrations at startup with retry logic
var maxRetries = builder.Configuration.GetValue<int>("DatabaseRetry:MaxRetryAttempts", 5);
var delayMs = builder.Configuration.GetValue<int>("DatabaseRetry:DelayMilliseconds", 2000);

var retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetry(
        retryCount: maxRetries,
        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(delayMs * attempt),
        onRetry: (exception, timespan, retryCount, context) =>
        {
            Console.WriteLine($"[DB Retry {retryCount}/{maxRetries}] Retrying after {timespan.TotalSeconds}s due to: {exception.Message}");
        });

try
{
    retryPolicy.Execute(() =>
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Console.WriteLine("Testing database connection...");
            db.Database.OpenConnection();
            db.Database.CloseConnection();
            Console.WriteLine("Database connection successful!");
            
            // Optionally log pending migrations:
            var pending = db.Database.GetPendingMigrations();
            if (pending.Any())
            {
                Console.WriteLine($"Found {pending.Count()} pending migrations.");
                // db.Database.Migrate(); // uncomment to auto-apply
            }
        }
    });
}
catch (Exception ex)
{
    Console.WriteLine($"[FATAL] Failed to connect to database after {maxRetries} retries: {ex.Message}");
    throw;
}

app.UseCors("AllowSvelteKit");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();