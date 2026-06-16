using TvShowCacheApi.Domain.Entities;

namespace TvShowCacheApi.Domain.Interfaces;

/// <summary>
///  TV show data access.(ADO.NET.)
/// </summary>
public interface ITvShowRepository
{
    Task<IEnumerable<TvShow>> GetAllAsync();
    Task<TvShow?> GetByIdAsync(int id);
    Task<TvShow?> GetByNameAsync(string name);
    Task SaveAsync(TvShow show);
    Task<bool> ExistsAsync(int id);
}