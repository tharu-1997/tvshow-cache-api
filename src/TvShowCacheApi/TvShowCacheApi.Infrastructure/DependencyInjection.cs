using Microsoft.Extensions.DependencyInjection;
using TvShowCacheApi.Application.Interfaces;
using TvShowCacheApi.Domain.Interfaces;
using TvShowCacheApi.Infrastructure.Data;
using TvShowCacheApi.Infrastructure.ExternalApi;

namespace TvShowCacheApi.Infrastructure;

/// <summary>
/// register all Infrastructure services with the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register the ADO.NET repository
        services.AddScoped<ITvShowRepository, TvShowRepository>();

        // Register the TVMaze HTTP client 
        services.AddHttpClient<ITvMazeService, TvMazeService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}