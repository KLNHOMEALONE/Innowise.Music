using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Innowise.MusicIdentityServer.Models.Music;

public class Track
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    public Guid? ArtistId { get; set; }
    
    [ForeignKey(nameof(ArtistId))]
    public virtual Artist? Artist { get; set; }
    
    public Guid? AlbumId { get; set; }
    
    [ForeignKey(nameof(AlbumId))]
    public virtual Album? Album { get; set; }
    
    public int? TrackNumber { get; set; }
    
    [Required]
    public int Duration { get; set; } // in seconds
    
    // Audio data storage - using chunking for large files
    // For files > 10MB, consider splitting into multiple rows or using Azure Blob Storage
    public byte[]? AudioData { get; set; }
    
    [MaxLength(50)]
    public string AudioFormat { get; set; } = "MP3"; // MP3, FLAC, WAV, etc.
    
    public int? Bitrate { get; set; } // kbps
    
    public int? SampleRate { get; set; } // Hz
    
    public long? FileSize { get; set; } // bytes
    
    [MaxLength(12)]
    public string? ISRC { get; set; } // International Standard Recording Code
    
    public bool Explicit { get; set; } = false;
    
    public long PlayCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Many-to-many relationship with Genres
    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
