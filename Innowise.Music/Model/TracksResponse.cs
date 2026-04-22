using System.Collections.Generic;

namespace Innowise.Music.Model;

public class TracksResponse
{
    public List<TrackDto> Tracks { get; set; } = new();
}
