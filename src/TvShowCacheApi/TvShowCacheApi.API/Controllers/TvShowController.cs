using Microsoft.AspNetCore.Mvc;
using TvShowCacheApi.API.DTOs;
using TvShowCacheApi.Application.Services;
using TvShowCacheApi.Domain.Entities;

namespace TvShowCacheApi.API.Controllers;

/// <summary>
/// RESTful API for TV show data with local DB caching.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TvShowController : ControllerBase
{
    private readonly TvShowService _tvShowService;
    private readonly ILogger<TvShowController> _logger;

    public TvShowController(TvShowService tvShowService, ILogger<TvShowController> logger)
    {
        _tvShowService = tvShowService;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/tvshow
    // Returns all TV shows currently stored in the local database cache.
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Get all TV shows from the local cache.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TvShowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var shows = await _tvShowService.GetAllCachedShowsAsync();
            var dtos = shows.Select(s => MapToDto(s, "cache"));
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all TV shows.");
            return StatusCode(500, new { error = "An error occurred while retrieving TV shows." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/tvshow/{id}
    // Cache-first: returns from DB if cached, otherwise fetches from TVMaze.
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Get a TV show by its TVMaze ID. Fetches from TVMaze and caches if not already stored.
    /// </summary>
  
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TvShowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest(new { error = "TV show ID must be a positive integer." });

        try
        {
           
            bool wasInCache = await IsInCacheAsync(id);

            var show = await _tvShowService.GetShowByIdAsync(id);

            if (show is null)
                return NotFound(new { error = $"TV show with ID {id} was not found." });

            var source = wasInCache ? "cache" : "tvmaze";
            return Ok(MapToDto(show, source));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "External API error for TV show ID {Id}.", id);
            return StatusCode(503, new { error = "Unable to reach TVMaze API. Please try again later." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving TV show ID {Id}.", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the TV show." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/tvshow/name/{name}
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Search for a TV show by name. Fetches from TVMaze and caches if not already stored.
    /// </summary>
    
    [HttpGet("name/{name}")]
    [ProducesResponseType(typeof(TvShowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "TV show name cannot be empty." });

        try
        {
            var show = await _tvShowService.GetShowByNameAsync(name);

            if (show is null)
                return NotFound(new { error = $"TV show '{name}' was not found." });

            return Ok(MapToDto(show, "tvmaze-or-cache"));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "External API error for TV show name '{Name}'.", name);
            return StatusCode(503, new { error = "Unable to reach TVMaze API. Please try again later." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving TV show '{Name}'.", name);
            return StatusCode(500, new { error = "An error occurred while retrieving the TV show." });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ─────────────────────────────────────────────────────────────────────────
    private async Task<bool> IsInCacheAsync(int id)
    {
        try
        {
            var existing = await _tvShowService.GetAllCachedShowsAsync();
            return existing.Any(s => s.Id == id);
        }
        catch
        {
            return false;
        }
    }

    private static TvShowDto MapToDto(TvShow show, string source)
    {
        return new TvShowDto
        {
            Id = show.Id,
            Name = show.Name,
            Status = show.Status,
            Language = show.Language,
            Genres = show.Genres.Split(',', StringSplitOptions.RemoveEmptyEntries),
            Network = show.Network,
            Rating = show.Rating,
            OfficialSite = show.OfficialSite,
            Summary = show.Summary,
            ImageUrl = show.ImageUrl,
            CachedAt = show.CachedAt,
            Source = source
        };
    }
}