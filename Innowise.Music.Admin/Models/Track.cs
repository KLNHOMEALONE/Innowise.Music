using System.ComponentModel.DataAnnotations;

namespace Innowise.Music.Admin.Models;

public class Track
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Track title is required")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Artist selection is required")]
    public Guid? ArtistId { get; set; }

    public Artist? Artist { get; set; }
    public Guid? AlbumId { get; set; }
    public Album? Album { get; set; }
    public int? TrackNumber { get; set; }
    public int Duration { get; set; }
    public string AudioFormat { get; set; } = "MP3";
    public int? Bitrate { get; set; }
    public int? SampleRate { get; set; }
    public long? FileSize { get; set; }
    public string? ISRC { get; set; }
    public bool Explicit { get; set; }
    public long PlayCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
