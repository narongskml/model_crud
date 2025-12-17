using PortModelApi.Data;
using Microsoft.EntityFrameworkCore;
using PortModelApi.Services;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSvelteKit",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Default SvelteKit port
                  .AllowAnyHeader()
                  .AllowAnyMethod();
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

// Apply pending migrations at startup (recommended only for dev/staging)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Optionally log pending migrations:
    var pending = db.Database.GetPendingMigrations();
    if (pending.Any())
    {
        // db.Database.Migrate(); // uncomment to auto-apply
    }
}

app.UseCors("AllowSvelteKit");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();