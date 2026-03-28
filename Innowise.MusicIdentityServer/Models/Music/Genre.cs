using System.ComponentModel.DataAnnotations;

namespace Innowise.MusicIdentityServer.Models.Music;

public class Genre
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    [MaxLength(7)]
    public string? Color { get; set; } // Hex color for UI
    
    // Navigation properties
    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
