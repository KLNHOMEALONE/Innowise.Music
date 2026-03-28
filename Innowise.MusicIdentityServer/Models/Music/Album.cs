using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Innowise.MusicIdentityServer.Models.Music;

public class Album
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    public Guid ArtistId { get; set; }
    
    [ForeignKey(nameof(ArtistId))]
    public virtual Artist? Artist { get; set; }
    
    public DateOnly? ReleaseDate { get; set; }
    
    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }
    
    [MaxLength(100)]
    public string? Genre { get; set; }
    
    [MaxLength(255)]
    public string? Label { get; set; }
    
    public int TotalTracks { get; set; } = 0;
    
    public int? Duration { get; set; } // in seconds
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
