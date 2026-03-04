using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Innowise.Music.ViewModel;

public partial class HomePageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _userName = "John Doe";

    public ObservableCollection<HomeItem> QuickAccessItems { get; } = new();
    public ObservableCollection<HomeItem> RecommendedArtists { get; } = new();
    public ObservableCollection<HomeItem> FeaturedSongs { get; } = new();
    public ObservableCollection<HomeItem> RecentItems { get; } = new();

    public HomePageViewModel()
    {
        LoadMockData();
    }

    private void LoadMockData()
    {
        // Quick Access (Pills)
        QuickAccessItems.Add(new HomeItem { Title = "Return to Forever", ImageUrl = "https://example.com/album1.jpg" });
        QuickAccessItems.Add(new HomeItem { Title = "Chick Corea", ImageUrl = "https://example.com/artist1.jpg" });
        QuickAccessItems.Add(new HomeItem { Title = "Ambient Chill", ImageUrl = "https://example.com/playlist1.jpg" });
        QuickAccessItems.Add(new HomeItem { Title = "Heavener", ImageUrl = "https://example.com/album2.jpg" });

        // Recommended Artists
        RecommendedArtists.Add(new HomeItem { Title = "Chick Corea", ImageUrl = "https://example.com/artist1.jpg", Subtitle = "Artist" });
        RecommendedArtists.Add(new HomeItem { Title = "Invent Animate", ImageUrl = "https://example.com/artist2.jpg", Subtitle = "Artist" });
        RecommendedArtists.Add(new HomeItem { Title = "Chick Corea", ImageUrl = "https://example.com/artist1.jpg", Subtitle = "Artist" });

        // Featured Songs
        FeaturedSongs.Add(new HomeItem { Title = "Heavener", ImageUrl = "https://example.com/album2.jpg", Subtitle = "Invent Animate" });
        FeaturedSongs.Add(new HomeItem { Title = "Return to forever", ImageUrl = "https://example.com/album1.jpg", Subtitle = "Chick Corea" });
        FeaturedSongs.Add(new HomeItem { Title = "Ambient", ImageUrl = "https://example.com/playlist1.jpg", Subtitle = "Various Artists" });

        // Recent Items
        RecentItems.Add(new HomeItem { Title = "Heavener", ImageUrl = "https://example.com/album2.jpg", Subtitle = "Invent Animate" });
        RecentItems.Add(new HomeItem { Title = "Return to forever", ImageUrl = "https://example.com/album1.jpg", Subtitle = "Chick Corea" });
        RecentItems.Add(new HomeItem { Title = "Ambient", ImageUrl = "https://example.com/playlist1.jpg", Subtitle = "Various Artists" });
    }
}

public class HomeItem
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}
