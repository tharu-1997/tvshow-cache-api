using Microsoft.Extensions.Logging;
using TvShowCacheApi.Application.Interfaces;
using TvShowCacheApi.Domain.Entities;
using TvShowCacheApi.Domain.Interfaces;

namespace TvShowCacheApi.Application.Services;

/// <summary>
/// The main business logic (always check  DB first before hitting TVMaze.)
/// </summary>
public class TvShowService
{
    private readonly ITvShowRepository _repository;
    private readonly ITvMazeService _tvMazeService;
    private readonly ILogger<TvShowService> _logger;

    public TvShowService(
        ITvShowRepository repository,
        ITvMazeService tvMazeService,
        ILogger<TvShowService> logger)
    {
        _repository = repository;
        _tvMazeService = tvMazeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all TV shows currently cached in the local database.
    /// </summary>
    public async Task<IEnumerable<TvShow>> GetAllCachedShowsAsync()
    {
        _logger.LogInformation("Fetching all TV shows from local cache.");
        return await _repository.GetAllAsync();
    }

    /// <summary>
    /// Get a single TV show by its TVMaze ID.
    /// </summary>
    public async Task<TvShow?> GetShowByIdAsync(int id)
    {
        _logger.LogInformation("Looking up TV show ID {Id} in local cache.", id);

        // 1. Check local DB cache first
        var cached = await _repository.GetByIdAsync(id);
        if (cached is not null)
        {
            _logger.LogInformation("Cache HIT for TV show ID {Id}.", id);
            return cached;
        }

        // 2. Not in cache — fetch from TVMaze
        _logger.LogInformation("Cache MISS for TV show ID {Id}. Fetching from TVMaze.", id);
        var show = await _tvMazeService.FetchShowByIdAsync(id);

        if (show is null)
        {
            _logger.LogWarning("TV show ID {Id} not found in TVMaze.", id);
            return null;
        }

        // 3. Save to DB cache
        await _repository.SaveAsync(show);
        _logger.LogInformation("TV show ID {Id} ({Name}) saved to local cache.", id, show.Name);

        return show;
    }

    /// <summary>
    /// Get a single TV show by name
    /// </summary>
    public async Task<TvShow?> GetShowByNameAsync(string name)
    {
        var normalizedName = name.Trim().ToLower();
        _logger.LogInformation("Looking up TV show '{Name}' in local cache.", normalizedName);

        // 1. Check local DB cache
        var cached = await _repository.GetByNameAsync(normalizedName);
        if (cached is not null)
        {
            _logger.LogInformation("Cache HIT for TV show '{Name}'.", normalizedName);
            return cached;
        }

        // 2. Fetch from TVMaze
        _logger.LogInformation("Cache MISS for '{Name}'. Fetching from TVMaze.", normalizedName);
        var show = await _tvMazeService.FetchShowByNameAsync(normalizedName);

        if (show is null)
        {
            _logger.LogWarning("TV show '{Name}' not found in TVMaze.", normalizedName);
            return null;
        }

        // 3. Save to cache
        await _repository.SaveAsync(show);
        _logger.LogInformation("TV show '{Name}' saved to local cache.", show.Name);

        return show;
    }
}