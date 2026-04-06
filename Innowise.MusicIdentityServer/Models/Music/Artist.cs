using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Innowise.MusicIdentityServer.Models.Music;

public class Artist
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Biography { get; set; }
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    public bool Verified { get; set; } = false;
    
    public long MonthlyListeners { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [JsonIgnore]
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
    [JsonIgnore]
    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
