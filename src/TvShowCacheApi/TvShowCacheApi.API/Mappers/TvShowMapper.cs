using TvShowCacheApi.API.DTOs;
using TvShowCacheApi.Domain.Entities;

namespace TvShowCacheApi.API.Mappers;

/// <summary>
/// Separated from controller so controller only handles HTTP concerns.
/// </summary>
public static class TvShowMapper
{
    public static TvShowDto ToDto(TvShow show, string source)
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

    public static IEnumerable<TvShowDto> ToDtoList(IEnumerable<TvShow> shows, string source)
    {
        return shows.Select(s => ToDto(s, source));
    }
}