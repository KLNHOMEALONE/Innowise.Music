using System;
using System.Collections.Generic;

namespace Innowise.Music.Model;

public class UnifiedSearchResponse
{
    public List<TrackDto> Tracks { get; set; } = new();
    public List<Artist> Artists { get; set; } = new();
    public List<Album> Albums { get; set; } = new();
    public int TotalTracks { get; set; }
    public int TotalArtists { get; set; }
    public int TotalAlbums { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class TrackDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Duration { get; set; }
    public int? TrackNumber { get; set; }
    public long PlayCount { get; set; }
    public Artist? Artist { get; set; }
    public Album? Album { get; set; }
}
