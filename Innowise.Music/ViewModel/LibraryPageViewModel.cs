using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Innowise.Music.ViewModel;

public partial class LibraryPageViewModel : ObservableObject
{
    public ObservableCollection<string> FilterChips { get; } = new()
    {
        "Playlists", "Artists", "Albums", "Podcasts & Shows"
    };

    public ObservableCollection<LibraryItem> LibraryItems { get; } = new()
    {
        new LibraryItem("Your Episodes", "Updated 2 days ago", "https://example.com/episodes.jpg", "Playlist", true),
        new LibraryItem("Invent Animate", "Artist", "https://example.com/artist1.jpg", "Artist", false),
        new LibraryItem("Heavener", "Invent Animate", "https://example.com/album1.jpg", "Album", false),
        new LibraryItem("Rock Mix", "Made for you", "https://example.com/mix1.jpg", "Playlist", false),
        new LibraryItem("Silent Planet", "Artist", "https://example.com/artist2.jpg", "Artist", false),
        new LibraryItem("SUPERBLOOM", "Silent Planet", "https://example.com/album3.jpg", "Album", false)
    };

    public LibraryPageViewModel()
    {
    }
}

public class LibraryItem
{
    public string Title { get; }
    public string Subtitle { get; }
    public string ImageUrl { get; }
    public string Type { get; }
    public bool IsPinned { get; }

    public LibraryItem(string title, string subtitle, string imageUrl, string type, bool isPinned)
    {
        Title = title;
        Subtitle = subtitle;
        ImageUrl = imageUrl;
        Type = type;
        IsPinned = isPinned;
    }
}
