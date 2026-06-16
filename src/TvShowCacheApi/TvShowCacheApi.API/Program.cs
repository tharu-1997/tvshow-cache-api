using TvShowCacheApi.API.Middleware;
using TvShowCacheApi.Application.Interfaces;
using TvShowCacheApi.Application.Services;
using TvShowCacheApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// SERVICES
// ─────────────────────────────────────────────────────────────────────────────

builder.Services.AddControllers();

// Swagger 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "TV Show Cache API",
        Version = "v1",
        Description = "ASP.NET Core Web API that caches TV show data from TVMaze API into MS SQL Server."
    });
});

// Register Infrastructure 
builder.Services.AddInfrastructure();

// Register Application service using interface 
builder.Services.AddScoped<ITvShowService, TvShowService>();


var app = builder.Build();

// Global exception handler
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TV Show Cache API v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();