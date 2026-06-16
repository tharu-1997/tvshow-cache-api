using TvShowCacheApi.Domain.Entities;

namespace TvShowCacheApi.Application.Interfaces;

/// <summary>
/// Talks to the TVMaze API
/// </summary>
public interface ITvMazeService
{
    Task<TvShow?> FetchShowByIdAsync(int id);
    Task<TvShow?> FetchShowByNameAsync(string name);
    Task<IEnumerable<string>> FetchShowNamesAsync(int page = 0);
}