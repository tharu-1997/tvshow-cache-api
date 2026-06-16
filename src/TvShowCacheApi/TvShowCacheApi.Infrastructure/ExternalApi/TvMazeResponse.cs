using System.Text.Json.Serialization;

namespace TvShowCacheApi.Infrastructure.ExternalApi;

// ─────────────────────────────────────────────────────────────────────────────
//  JSON structure returned by {id}
// ─────────────────────────────────────────────────────────────────────────────

public class TvMazeShowResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = [];

    [JsonPropertyName("network")]
    public TvMazeNetwork? Network { get; set; }

    [JsonPropertyName("rating")]
    public TvMazeRating Rating { get; set; } = new();

    [JsonPropertyName("officialSite")]
    public string? OfficialSite { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("image")]
    public TvMazeImage? Image { get; set; }
}

public class TvMazeNetwork
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TvMazeRating
{
    [JsonPropertyName("average")]
    public double? Average { get; set; }
}

public class TvMazeImage
{
    [JsonPropertyName("medium")]
    public string? Medium { get; set; }

    [JsonPropertyName("original")]
    public string? Original { get; set; }
}

// searching shows by name
public class TvMazeSearchResult
{
    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("show")]
    public TvMazeShowResponse Show { get; set; } = new();
}