using Microsoft.Extensions.Logging;
using TvShowCacheApi.Application.Interfaces;
using TvShowCacheApi.Domain.Entities;
using TvShowCacheApi.Domain.Interfaces;

namespace TvShowCacheApi.Application.Services;

public class TvShowService : ITvShowService
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

    public async Task<IEnumerable<TvShow>> GetAllCachedShowsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all TV shows from local cache.");
        return await _repository.GetAllAsync(ct);
    }

    public async Task<TvShow?> GetShowByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Looking up TV show ID {Id} in local cache.", id);

        var cached = await _repository.GetByIdAsync(id, ct);
        if (cached is not null)
        {
            _logger.LogInformation("Cache HIT for TV show ID {Id}.", id);
            return cached;
        }

        _logger.LogInformation("Cache MISS for TV show ID {Id}. Fetching from TVMaze.", id);
        var show = await _tvMazeService.FetchShowByIdAsync(id);

        if (show is null)
        {
            _logger.LogWarning("TV show ID {Id} not found in TVMaze.", id);
            return null;
        }

        await _repository.SaveAsync(show, ct);
        _logger.LogInformation("TV show ID {Id} ({Name}) saved to local cache.", id, show.Name);

        return show;
    }

    public async Task<TvShow?> GetShowByNameAsync(string name, CancellationToken ct = default)
    {
        var normalizedName = name.Trim().ToLower();
        _logger.LogInformation("Looking up TV show '{Name}' in local cache.", normalizedName);

        var cached = await _repository.GetByNameAsync(normalizedName, ct);
        if (cached is not null)
        {
            _logger.LogInformation("Cache HIT for TV show '{Name}'.", normalizedName);
            return cached;
        }

        _logger.LogInformation("Cache MISS for '{Name}'. Fetching from TVMaze.", normalizedName);
        var show = await _tvMazeService.FetchShowByNameAsync(normalizedName);

        if (show is null)
        {
            _logger.LogWarning("TV show '{Name}' not found in TVMaze.", normalizedName);
            return null;
        }

        await _repository.SaveAsync(show, ct);
        _logger.LogInformation("TV show '{Name}' saved to local cache.", show.Name);

        return show;
    }

    public async Task<bool> IsShowInCacheAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Checking cache for TV show ID {Id}.", id);
        var existing = await _repository.GetByIdAsync(id, ct);
        return existing is not null;
    }
}