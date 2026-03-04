using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Innowise.Music.ViewModel;

public partial class SearchPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchQuery;

    public ObservableCollection<string> FilterChips { get; } = new()
    {
        "Artists", "Songs", "Albums", "Playlists", "Podcasts"
    };

    public ObservableCollection<GenreItem> Genres { get; } = new()
    {
        new GenreItem("Pop", "#E13300", "https://example.com/pop.jpg"),
        new GenreItem("Hip-Hop", "#1E3264", "https://example.com/hiphop.jpg"),
        new GenreItem("Rock", "#7358FF", "https://example.com/rock.jpg"),
        new GenreItem("Electronic", "#E8115B", "https://example.com/electronic.jpg"),
        new GenreItem("Jazz", "#FF4632", "https://example.com/jazz.jpg"),
        new GenreItem("Classical", "#503750", "https://example.com/classical.jpg")
    };

    public ObservableCollection<RecentSearchItem> RecentSearches { get; } = new()
    {
        new RecentSearchItem("Invent Animate", "Artist", "https://example.com/artist1.jpg"),
        new RecentSearchItem("Heavener", "Album", "https://example.com/album1.jpg"),
        new RecentSearchItem("Shade Astray", "Song", "https://example.com/album2.jpg")
    };

    public SearchPageViewModel()
    {
    }
}

public class GenreItem
{
    public string Name { get; }
    public string Color { get; }
    public string ImageUrl { get; }

    public GenreItem(string name, string color, string imageUrl)
    {
        Name = name;
        Color = color;
        ImageUrl = imageUrl;
    }
}

public class RecentSearchItem
{
    public string Title { get; }
    public string Type { get; }
    public string ImageUrl { get; }

    public RecentSearchItem(string title, string type, string imageUrl)
    {
        Title = title;
        Type = type;
        ImageUrl = imageUrl;
    }
}
