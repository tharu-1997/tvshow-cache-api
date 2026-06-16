using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TvShowCacheApi.Domain.Entities;
using TvShowCacheApi.Domain.Interfaces;

namespace TvShowCacheApi.Infrastructure.Data;

public class TvShowRepository : ITvShowRepository
{
    private readonly string _connectionString;
    private readonly ILogger<TvShowRepository> _logger;

    public TvShowRepository(IConfiguration configuration, ILogger<TvShowRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    public async Task<IEnumerable<TvShow>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT Id, Name, Status, Language, Genres,
                   Network, Rating, OfficialSite, Summary, ImageUrl, CachedAt
            FROM dbo.TvShows ORDER BY Id;";

        var results = new List<TvShow>();
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                results.Add(MapFromReader(reader));
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while fetching all TV shows.");
            throw;
        }
        return results;
    }

    public async Task<TvShow?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT Id, Name, Status, Language, Genres,
                   Network, Rating, OfficialSite, Summary, ImageUrl, CachedAt
            FROM dbo.TvShows WHERE Id = @Id;";
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);
            await using var reader = await command.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
                return MapFromReader(reader);
            return null;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while fetching TV show by ID {Id}.", id);
            throw;
        }
    }

    public async Task<TvShow?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT Id, Name, Status, Language, Genres,
                   Network, Rating, OfficialSite, Summary, ImageUrl, CachedAt
            FROM dbo.TvShows WHERE LOWER(Name) = @Name;";
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Name", name.ToLower());
            await using var reader = await command.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
                return MapFromReader(reader);
            return null;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while fetching TV show by name '{Name}'.", name);
            throw;
        }
    }

    public async Task SaveAsync(TvShow show, CancellationToken ct = default)
    {
        const string sql = @"
            MERGE INTO dbo.TvShows AS target
            USING (SELECT @Id AS Id) AS source ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET Name=@Name, Status=@Status, Language=@Language,
                           Genres=@Genres, Network=@Network, Rating=@Rating,
                           OfficialSite=@OfficialSite, Summary=@Summary,
                           ImageUrl=@ImageUrl, CachedAt=@CachedAt
            WHEN NOT MATCHED THEN
                INSERT (Id,Name,Status,Language,Genres,Network,Rating,OfficialSite,Summary,ImageUrl,CachedAt)
                VALUES (@Id,@Name,@Status,@Language,@Genres,@Network,@Rating,@OfficialSite,@Summary,@ImageUrl,@CachedAt);";
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", show.Id);
            command.Parameters.AddWithValue("@Name", show.Name);
            command.Parameters.AddWithValue("@Status", show.Status);
            command.Parameters.AddWithValue("@Language", show.Language);
            command.Parameters.AddWithValue("@Genres", show.Genres);
            command.Parameters.AddWithValue("@Network", show.Network);
            command.Parameters.AddWithValue("@Rating", (object?)show.Rating ?? DBNull.Value);
            command.Parameters.AddWithValue("@OfficialSite", show.OfficialSite);
            command.Parameters.AddWithValue("@Summary", show.Summary);
            command.Parameters.AddWithValue("@ImageUrl", show.ImageUrl);
            command.Parameters.AddWithValue("@CachedAt", show.CachedAt);
            await command.ExecuteNonQueryAsync(ct);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while saving TV show ID {Id}.", show.Id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.TvShows WHERE Id = @Id;";
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);
            var count = (int)(await command.ExecuteScalarAsync(ct) ?? 0);
            return count > 0;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error checking existence of TV show ID {Id}.", id);
            throw;
        }
    }

    private static TvShow MapFromReader(SqlDataReader reader)
    {
        return new TvShow
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            Language = reader.GetString(reader.GetOrdinal("Language")),
            Genres = reader.GetString(reader.GetOrdinal("Genres")),
            Network = reader.GetString(reader.GetOrdinal("Network")),
            Rating = reader.IsDBNull(reader.GetOrdinal("Rating")) ? null : reader.GetDouble(reader.GetOrdinal("Rating")),
            OfficialSite = reader.IsDBNull(reader.GetOrdinal("OfficialSite")) ? string.Empty : reader.GetString(reader.GetOrdinal("OfficialSite")),
            Summary = reader.IsDBNull(reader.GetOrdinal("Summary")) ? string.Empty : reader.GetString(reader.GetOrdinal("Summary")),
            ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? string.Empty : reader.GetString(reader.GetOrdinal("ImageUrl")),
            CachedAt = reader.GetDateTime(reader.GetOrdinal("CachedAt"))
        };
    }
}