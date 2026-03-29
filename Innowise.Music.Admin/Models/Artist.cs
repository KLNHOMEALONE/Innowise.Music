namespace Innowise.Music.Admin.Models;

public class Artist
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public string? ImageUrl { get; set; }
    public bool Verified { get; set; }
    public long MonthlyListeners { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Removed ICollection<Album> Albums and ICollection<Track> Tracks to prevent circular dependencies and simplify.
    // public ICollection<Album> Albums { get; set; } = new List<Album>();
    // public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
