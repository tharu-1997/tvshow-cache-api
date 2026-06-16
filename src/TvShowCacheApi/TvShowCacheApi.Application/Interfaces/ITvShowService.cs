using TvShowCacheApi.Domain.Entities;

namespace TvShowCacheApi.Application.Interfaces;

public interface ITvShowService
{
    Task<IEnumerable<TvShow>> GetAllCachedShowsAsync(CancellationToken ct = default);
    Task<TvShow?> GetShowByIdAsync(int id, CancellationToken ct = default);
    Task<TvShow?> GetShowByNameAsync(string name, CancellationToken ct = default);
    Task<bool> IsShowInCacheAsync(int id, CancellationToken ct = default);
}