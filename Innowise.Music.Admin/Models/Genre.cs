namespace Innowise.Music.Admin.Models;

public class Genre
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Color { get; set; }
    // Removed ICollection<Track> Tracks to prevent circular dependencies and simplify.
    // public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
