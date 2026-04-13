namespace Innowise.Music.Model;

public class Album
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public DateOnly? ReleaseDate { get; set; }
}
