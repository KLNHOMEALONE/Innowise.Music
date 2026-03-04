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
        QuickAccessItems.Add(new HomeItem { Title = "Return to Forever", ImageUrl = "return_to_forever.png" });
        QuickAccessItems.Add(new HomeItem { Title = "Chick Corea", ImageUrl = "chick_corea.png" });
        QuickAccessItems.Add(new HomeItem { Title = "Ambient Chill", ImageUrl = "playlist_big.png" });
        QuickAccessItems.Add(new HomeItem { Title = "Heavener", ImageUrl = "shade_astray.png" });

        // Recommended Artists
        RecommendedArtists.Add(new HomeItem { Title = "Chick Corea", ImageUrl = "chick_corea.png", Subtitle = "Artist" });
        RecommendedArtists.Add(new HomeItem { Title = "Invent Animate", ImageUrl = "shade_astray.png", Subtitle = "Artist" });
        RecommendedArtists.Add(new HomeItem { Title = "Chick Corea", ImageUrl = "chick_corea.png", Subtitle = "Artist" });

        // Featured Songs
        FeaturedSongs.Add(new HomeItem { Title = "Heavener", ImageUrl = "shade_astray.png", Subtitle = "Invent Animate" });
        FeaturedSongs.Add(new HomeItem { Title = "Return to forever", ImageUrl = "return_to_forever.png", Subtitle = "Chick Corea" });
        FeaturedSongs.Add(new HomeItem { Title = "Ambient", ImageUrl = "playlist_big.png", Subtitle = "Various Artists" });

        // Recent Items
        RecentItems.Add(new HomeItem { Title = "Heavener", ImageUrl = "shade_astray.png", Subtitle = "Invent Animate" });
        RecentItems.Add(new HomeItem { Title = "Return to forever", ImageUrl = "return_to_forever.png", Subtitle = "Chick Corea" });
        RecentItems.Add(new HomeItem { Title = "Ambient", ImageUrl = "playlist_big.png", Subtitle = "Various Artists" });
    }
}

public class HomeItem
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}
