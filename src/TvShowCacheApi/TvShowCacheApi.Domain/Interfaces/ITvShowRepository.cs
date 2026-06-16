using TvShowCacheApi.Domain.Entities;

namespace TvShowCacheApi.Domain.Interfaces;

public interface ITvShowRepository
{
    Task<IEnumerable<TvShow>> GetAllAsync(CancellationToken ct = default);
    Task<TvShow?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TvShow?> GetByNameAsync(string name, CancellationToken ct = default);
    Task SaveAsync(TvShow show, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}