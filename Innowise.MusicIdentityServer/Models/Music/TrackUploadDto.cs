/*
 * @file: TrackUploadDto.cs
 * @description: DTO for batch track upload with metadata
 * @dependencies: None
 * @created: 2026-04-01
 */

namespace Innowise.MusicIdentityServer.Models.Music;

public class TrackUploadDto
{
    public string FileName { get; set; } = string.Empty;
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public Guid? ArtistId { get; set; }
    public string? AlbumName { get; set; }
    public Guid? AlbumId { get; set; }
    public string[]? Genres { get; set; }
    public Guid[]? GenreIds { get; set; }
    public int Duration { get; set; }
    public int? TrackNumber { get; set; }
    public int? Bitrate { get; set; }
    public int? SampleRate { get; set; }
    public long FileSize { get; set; }
    public string AudioFormat { get; set; } = "MP3";
    public uint? Year { get; set; }
}

public class BatchUploadResult
{
    public bool Success { get; set; }
    public int UploadedCount { get; set; }
    public int FailedCount { get; set; }
    public List<TrackUploadResult> Results { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class TrackUploadResult
{
    public string FileName { get; set; } = string.Empty;
    public Guid? TrackId { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}
