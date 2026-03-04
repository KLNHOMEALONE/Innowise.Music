using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Maui;

namespace Innowise.Music.ViewModel;

public partial class LibraryPageViewModel : ObservableObject
{
    public ObservableCollection<string> FilterChips { get; } = new()
    {
        "Playlists", "Albums", "Songs", "Artists", "Local"
    };

    public ObservableCollection<LibraryItem> LibraryItems { get; } = new()
    {
        new LibraryItem("Local files", "45 songs", "local_files.png", "", "#2A1F1F", false) { TitlePrefix = "↓ " },
                new LibraryItem("Liked songs", "112 songs", "liked_songs.png", "", "#D90429", false),
                new LibraryItem("Return to Fore...", "Album / Chick C...", "return_to_forever.png", "", "", false),
                new LibraryItem("Ambient Chill", "Playlist / John...", "playlist_big.png", "", "", false),
                new LibraryItem("What Game S...", "Song / Chick C...", "return_to_forever.png", "", "", false),
                new LibraryItem("Chick Corea", "Artist", "chick_corea.png", "", "", true),
                new LibraryItem("Return to Fore...", "Album / Chick C...", "return_to_forever.png", "", "", false),
                new LibraryItem("Return to Fore...", "Album / Chick C...", "return_to_forever.png", "", "", false)
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
    public string Icon { get; }
    public string IconBackgroundColor { get; }
    public bool IsArtist { get; }
    public string TitlePrefix { get; set; } = "";

    public string DisplayTitle => TitlePrefix + Title;

    public bool HasImage => !string.IsNullOrEmpty(ImageUrl);
    public bool HasIcon => !string.IsNullOrEmpty(Icon);
    public CornerRadius ItemCornerRadius => IsArtist ? new CornerRadius(57.5) : new CornerRadius(4);

    public LibraryItem(string title, string subtitle, string imageUrl, string icon, string iconBackgroundColor, bool isArtist)
    {
        Title = title;
        Subtitle = subtitle;
        ImageUrl = imageUrl;
        Icon = icon;
        IconBackgroundColor = string.IsNullOrEmpty(iconBackgroundColor) ? "Transparent" : iconBackgroundColor;
        IsArtist = isArtist;
    }
}
