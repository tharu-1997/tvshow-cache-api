namespace TvShowCacheApi.Domain.Entities;


public class TvShow
{
    public int Id { get; set; }                         
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;  
    public string Language { get; set; } = string.Empty;
    public string Genres { get; set; } = string.Empty;  
    public string Network { get; set; } = string.Empty;
    public double? Rating { get; set; }                 
    public string OfficialSite { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty; 
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CachedAt { get; set; }              
}