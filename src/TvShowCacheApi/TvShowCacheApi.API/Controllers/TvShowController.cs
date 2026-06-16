using Microsoft.AspNetCore.Mvc;
using TvShowCacheApi.API.DTOs;
using TvShowCacheApi.API.Mappers;
using TvShowCacheApi.Application.Interfaces;

namespace TvShowCacheApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TvShowController : ControllerBase
{
    private readonly ITvShowService _tvShowService;
    private readonly ILogger<TvShowController> _logger;

    public TvShowController(ITvShowService tvShowService, ILogger<TvShowController> logger)
    {
        _tvShowService = tvShowService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TvShowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        try
        {
            var shows = await _tvShowService.GetAllCachedShowsAsync(ct);
            return Ok(TvShowMapper.ToDtoList(shows, "cache"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all TV shows.");
            return StatusCode(500, new { error = "An error occurred while retrieving TV shows." });
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TvShowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        if (id <= 0)
            return BadRequest(new { error = "TV show ID must be a positive integer." });

        try
        {
            bool wasInCache = await _tvShowService.IsShowInCacheAsync(id, ct);
            var show = await _tvShowService.GetShowByIdAsync(id, ct);

            if (show is null)
                return NotFound(new { error = $"TV show with ID {id} was not found." });

            var source = wasInCache ? "cache" : "tvmaze";
            return Ok(TvShowMapper.ToDto(show, source));
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

    [HttpGet("name/{name}")]
    [ProducesResponseType(typeof(TvShowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByName(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "TV show name cannot be empty." });

        try
        {
            var show = await _tvShowService.GetShowByNameAsync(name, ct);

            if (show is null)
                return NotFound(new { error = $"TV show '{name}' was not found." });

            return Ok(TvShowMapper.ToDto(show, "tvmaze-or-cache"));
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
}