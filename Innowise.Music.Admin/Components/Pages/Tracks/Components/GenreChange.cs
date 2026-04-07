using Innowise.Music.Admin.Models;

namespace Innowise.Music.Admin.Components.Pages.Tracks.Components;

public class GenreChange
{
    public ExtractedTrackMetadata Track { get; set; } = default!;
    public Guid GenreId { get; set; }
    public bool IsChecked { get; set; }
}
