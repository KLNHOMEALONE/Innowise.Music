using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Model;
using Innowise.Music.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Innowise.Music.ViewModel;

public partial class HomePageViewModel : ObservableObject
{
    private readonly MiniPlayerViewModel _miniPlayerViewModel;
    private readonly IAuthenticationService _authenticationService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _userName = "John Doe";

    public ObservableCollection<HomeItem> QuickAccessItems { get; } = new();
    public ObservableCollection<HomeItem> RecommendedArtists { get; } = new();
    public ObservableCollection<HomeItem> FeaturedSongs { get; } = new();
    public ObservableCollection<HomeItem> RecentItems { get; } = new();

    public HomePageViewModel(
        MiniPlayerViewModel miniPlayerViewModel,
        IAuthenticationService authenticationService,
        IGoogleAuthService googleAuthService,
        INavigationService navigationService)
    {
        _miniPlayerViewModel = miniPlayerViewModel;
        _authenticationService = authenticationService;
        _googleAuthService = googleAuthService;
        _navigationService = navigationService;
        LoadMockData();
        LoadUserName();
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
            FileUri = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3"
        };

        await _miniPlayerViewModel.PlayTrack(track);
    }

    [RelayCommand]
    private async Task Logout()
    {
        try
        {
            if (_googleAuthService.IsSignedIn)
            {
                await _googleAuthService.SignOut();
            }

            await _authenticationService.LogoutAsync();
        }
        catch
        {
        }

        await _navigationService.NavigateAndClearStackAsync("LoginPage");
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

    private void LoadUserName()
    {
        var userName = _authenticationService.GetUserName();
        if (!string.IsNullOrEmpty(userName))
        {
            UserName = userName;
        }
    }

    public void RefreshUserName()
    {
        LoadUserName();
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
