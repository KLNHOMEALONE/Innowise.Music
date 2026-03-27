using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Innowise.Music.ViewModel;

public partial class HomePageViewModel : ObservableObject
{
    private readonly MiniPlayerViewModel _miniPlayerViewModel;

    [ObservableProperty]
    private string _userName = "John Doe";

    public ObservableCollection<HomeItem> QuickAccessItems { get; } = new();
    public ObservableCollection<HomeItem> RecommendedArtists { get; } = new();
    public ObservableCollection<HomeItem> FeaturedSongs { get; } = new();
    public ObservableCollection<HomeItem> RecentItems { get; } = new();

    public HomePageViewModel(MiniPlayerViewModel miniPlayerViewModel)
    {
        _miniPlayerViewModel = miniPlayerViewModel;
        LoadMockData();
    }

    [RelayCommand]
    private async Task PlayFeaturedSong(HomeItem song)
    {
        if (song == null) return;

        var track = new Track
        {
            Title = song.Title,
            ArtistName = song.Subtitle,
            ImageUrl = song.ImageUrl,
            // Using a sample public domain audio file for testing
            FileUri = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3"
        };

        await _miniPlayerViewModel.PlayTrack(track);
    }

    private void LoadMockData()
    {
        // Quick Access (Pills)
        QuickAccessItems.Add(new HomeItem(this) { Title = "Return to Forever", ImageUrl = "return_to_forever.png" });
        QuickAccessItems.Add(new HomeItem(this) { Title = "Chick Corea", ImageUrl = "chick_corea.png" });
        QuickAccessItems.Add(new HomeItem(this) { Title = "Ambient Chill", ImageUrl = "playlist_big.png" });
        QuickAccessItems.Add(new HomeItem(this) { Title = "Heavener", ImageUrl = "shade_astray.png" });

        // Recommended Artists
        RecommendedArtists.Add(new HomeItem(this) { Title = "Chick Corea", ImageUrl = "chick_corea.png", Subtitle = "Artist" });
        RecommendedArtists.Add(new HomeItem(this) { Title = "Invent Animate", ImageUrl = "shade_astray.png", Subtitle = "Artist" });
        RecommendedArtists.Add(new HomeItem(this) { Title = "Chick Corea", ImageUrl = "chick_corea.png", Subtitle = "Artist" });

        // Featured Songs
        FeaturedSongs.Add(new HomeItem(this) { Title = "Heavener", ImageUrl = "shade_astray.png", Subtitle = "Invent Animate" });
        FeaturedSongs.Add(new HomeItem(this) { Title = "Return to forever", ImageUrl = "return_to_forever.png", Subtitle = "Chick Corea" });
        FeaturedSongs.Add(new HomeItem(this) { Title = "Ambient", ImageUrl = "playlist_big.png", Subtitle = "Various Artists" });

        // Recent Items
        RecentItems.Add(new HomeItem(this) { Title = "Heavener", ImageUrl = "shade_astray.png", Subtitle = "Invent Animate" });
        RecentItems.Add(new HomeItem(this) { Title = "Return to forever", ImageUrl = "return_to_forever.png", Subtitle = "Chick Corea" });
        RecentItems.Add(new HomeItem(this) { Title = "Ambient", ImageUrl = "playlist_big.png", Subtitle = "Various Artists" });
    }
}

public class HomeItem
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public HomePageViewModel Parent { get; }

    public HomeItem(HomePageViewModel parent)
    {
        Parent = parent;
    }
}
