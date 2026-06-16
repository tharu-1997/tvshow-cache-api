using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TvShowCacheApi.Application.Interfaces;
using TvShowCacheApi.Domain.Entities;

namespace TvShowCacheApi.Infrastructure.ExternalApi;

/// <summary>
/// HTTP client for the public TVMaze API
/// No API key required.
/// </summary>
public class TvMazeService : ITvMazeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TvMazeService> _logger;
    private const string BaseUrl = "https://api.tvmaze.com/";

    public TvMazeService(HttpClient httpClient, ILogger<TvMazeService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TvShowCacheApi/1.0");
        _logger = logger;
    }

    public async Task<TvShow?> FetchShowByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Calling TVMaze API: GET shows/{Id}", id);

            var response = await _httpClient.GetAsync($"shows/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("TVMaze returned 404 for show ID {Id}.", id);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<TvMazeShowResponse>();
            return data is null ? null : MapToEntity(data);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching TV show ID {Id} from TVMaze.", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching TV show ID {Id} from TVMaze.", id);
            throw;
        }
    }

    public async Task<TvShow?> FetchShowByNameAsync(string name)
    {
        try
        {
            _logger.LogInformation("Calling TVMaze API: GET search/shows?q={Name}", name);

            
            var results = await _httpClient.GetFromJsonAsync<List<TvMazeSearchResult>>(
                $"search/shows?q={Uri.EscapeDataString(name)}");

            if (results is null || results.Count == 0)
            {
                _logger.LogWarning("TVMaze returned no results for '{Name}'.", name);
                return null;
            }

            
            return MapToEntity(results[0].Show);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error searching TVMaze for '{Name}'.", name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error searching TVMaze for '{Name}'.", name);
            throw;
        }
    }

    public async Task<IEnumerable<string>> FetchShowNamesAsync(int page = 0)
    {
        try
        {
            
            var shows = await _httpClient.GetFromJsonAsync<List<TvMazeShowResponse>>(
                $"shows?page={page}");

            return shows?.Select(s => s.Name) ?? Enumerable.Empty<string>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch show list from TVMaze (page {Page}).", page);
            return Enumerable.Empty<string>();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // map TVMaze API response to domain entity
    // ─────────────────────────────────────────────────────────────────────────
    private static TvShow MapToEntity(TvMazeShowResponse data)
    {
        return new TvShow
        {
            Id = data.Id,
            Name = data.Name,
            Status = data.Status ?? string.Empty,
            Language = data.Language ?? string.Empty,
            Genres = string.Join(",", data.Genres),
            Network = data.Network?.Name ?? string.Empty,
            Rating = data.Rating.Average,
            OfficialSite = data.OfficialSite ?? string.Empty,
            Summary = StripHtml(data.Summary ?? string.Empty),
            ImageUrl = data.Image?.Medium ?? data.Image?.Original ?? string.Empty,
            CachedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Removes HTML tags from TVMaze summary strings.
    /// </summary>
    private static string StripHtml(string html)
    {
        return Regex.Replace(html, "<.*?>", string.Empty).Trim();
    }
}