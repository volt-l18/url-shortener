namespace UrlShortener.Api.Models;

public class ShortenedUrl
{
    public int Id { get; set; }
    
    // The 8-character unique base suffix discussed in the video
    public string ShortCode { get; set; } = string.Empty;
    
    public string LongUrl { get; set; } = string.Empty;
    
    public string UserId { get; set; } = string.Empty;
    
    // Stored as DateTime, matching the video's timestamp logic
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; }
}

