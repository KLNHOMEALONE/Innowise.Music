/*
 * @file: ExtractedTrackMetadata.cs
 * @description: Model for extracted metadata from audio files
 * @dependencies: None
 * @created: 2026-04-01
 */

namespace Innowise.Music.Admin.Models;

public class ExtractedTrackMetadata
{
    public string FileName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string[] Performers { get; set; } = Array.Empty<string>();
    public string Album { get; set; } = string.Empty;
    public string[] Genres { get; set; } = Array.Empty<string>();
    public uint Year { get; set; }
    public uint TrackNumber { get; set; }
    public int Duration { get; set; } // in seconds
    public int Bitrate { get; set; }
    public int SampleRate { get; set; }
    public long FileSize { get; set; }
    public string AudioFormat { get; set; } = string.Empty;
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    
    // Helper property for comma-separated genres input
    public string GenresString
    {
        get => string.Join(", ", Genres);
        set => Genres = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(g => g.Trim())
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .ToArray();
    }
    
    // UI-specific properties for matching with existing entities
    public Guid? MatchedArtistId { get; set; }
    public string? MatchedArtistName { get; set; }
    public Guid? MatchedAlbumId { get; set; }
    public string? MatchedAlbumName { get; set; }
    public Guid[] MatchedGenreIds { get; set; } = Array.Empty<Guid>();
    public string[] MatchedGenreNames { get; set; } = Array.Empty<string>();
    
    // Validation state
    public bool IsValid => !string.IsNullOrWhiteSpace(Title) && 
                           Duration > 0 && 
                           AudioData.Length > 0;
}
