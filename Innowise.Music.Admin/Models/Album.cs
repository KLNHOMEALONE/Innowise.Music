using System.ComponentModel.DataAnnotations;

namespace Innowise.Music.Admin.Models;

public class Album
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Album title is required")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Artist selection is required")]
    public Guid ArtistId { get; set; }

    public Artist? Artist { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Genre { get; set; }
    public string? Label { get; set; }
    public int TotalTracks { get; set; }
    public int? Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
