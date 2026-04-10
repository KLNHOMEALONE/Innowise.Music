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
    private readonly IRecommendationService _recommendationService;
    private readonly IHistoryService _historyService;
    private readonly IFavoriteService _favoriteService;

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
        INavigationService navigationService,
        IRecommendationService recommendationService,
        IHistoryService historyService,
        IFavoriteService favoriteService)
    {
        _miniPlayerViewModel = miniPlayerViewModel;
        _authenticationService = authenticationService;
        _googleAuthService = googleAuthService;
        _navigationService = navigationService;
        _recommendationService = recommendationService;
        _historyService = historyService;
        _favoriteService = favoriteService;
        System.Diagnostics.Debug.WriteLine("[HomeVM] Constructor: loading mock data");
        LoadMockData();
        LoadUserName();

        // Subscribe to history refresh messages from MiniPlayer
        MessagingCenter.Subscribe<MiniPlayerViewModel>(this, "RecentTracksChanged", async _ =>
        {
            await LoadRecentItemsAsync();
        });
    }

    public async Task LoadRecommendationsAsync()
    {
        System.Diagnostics.Debug.WriteLine("[HomeVM] LoadRecommendationsAsync started");
        try
        {
            var tracks = await _recommendationService.GetRecommendationsAsync();
            System.Diagnostics.Debug.WriteLine($"[HomeVM] Got {tracks.Count} recommendation tracks");
            if (tracks.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[HomeVM] No recommendations, keeping mock data");
            }
            else
            {
                FeaturedSongs.Clear();
                foreach (var track in tracks)
                {
                    FeaturedSongs.Add(new HomeItem(this)
                    {
                        Id = track.Id,
                        Title = track.Title,
                        Subtitle = track.ArtistName,
                        ImageUrl = track.ImageUrl,
                        FileUri = track.FileUri
                    });
                }
                System.Diagnostics.Debug.WriteLine($"[HomeVM] FeaturedSongs now has {FeaturedSongs.Count} items");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeVM] LoadRecommendationsAsync error: {ex.Message}");
        }

        // Load user's favorite tracks for Quick Access
        await LoadFavoriteTracksAsync();

        // Load recommended artists from user's listening history
        try
        {
            var artists = await _recommendationService.GetRecommendedArtistsAsync();
            System.Diagnostics.Debug.WriteLine($"[HomeVM] Got {artists.Count} recommended artists");
            if (artists.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[HomeVM] No recommended artists, keeping mock data");
                return;
            }

            RecommendedArtists.Clear();
            foreach (var artist in artists)
            {
                RecommendedArtists.Add(new HomeItem(this)
                {
                    Id = artist.Id,
                    Title = artist.Name,
                    ImageUrl = artist.ImageUrl ?? string.Empty,
                    Subtitle = "Artist"
                });
            }
            System.Diagnostics.Debug.WriteLine($"[HomeVM] RecommendedArtists now has {RecommendedArtists.Count} items");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeVM] LoadRecommendedArtistsAsync error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task PlayFeaturedSong(HomeItem song)
    {
        if (song == null) return;

        var track = new Track
        {
            Id = song.Id,
            Title = song.Title,
            ArtistName = song.Subtitle,
            ImageUrl = song.ImageUrl,
            FileUri = !string.IsNullOrEmpty(song.FileUri)
                ? song.FileUri
                : "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3"
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

        // Clear user-specific recent items so next login doesn't show previous user's data
        RecentItems.Clear();

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

        // Featured Songs (mock data - replaced by LoadRecommendationsAsync after auth)
        FeaturedSongs.Add(new HomeItem(this) { Title = "Heavener", ImageUrl = "shade_astray.png", Subtitle = "Invent Animate" });
        FeaturedSongs.Add(new HomeItem(this) { Title = "Return to forever", ImageUrl = "return_to_forever.png", Subtitle = "Chick Corea" });
        FeaturedSongs.Add(new HomeItem(this) { Title = "Ambient", ImageUrl = "playlist_big.png", Subtitle = "Various Artists" });
    }

    public async Task LoadRecentItemsAsync()
    {
        System.Diagnostics.Debug.WriteLine("[HomeVM] LoadRecentItemsAsync started");
        try
        {
            var tracks = await _historyService.GetRecentTracksAsync(5);
            System.Diagnostics.Debug.WriteLine($"[HomeVM] Got {tracks.Count} recent tracks");

            RecentItems.Clear();
            foreach (var track in tracks)
            {
                RecentItems.Add(new HomeItem(this)
                {
                    Id = track.Id,
                    Title = track.Title,
                    Subtitle = track.ArtistName,
                    ImageUrl = track.ImageUrl,
                    FileUri = track.FileUri
                });
            }
            System.Diagnostics.Debug.WriteLine($"[HomeVM] RecentItems now has {RecentItems.Count} items");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeVM] LoadRecentItemsAsync error: {ex.Message}");
        }
    }

    public async Task LoadFavoriteTracksAsync()
    {
        System.Diagnostics.Debug.WriteLine("[HomeVM] LoadFavoriteTracksAsync started");
        try
        {
            var favorites = await _favoriteService.GetAllFavoritesAsync();
            System.Diagnostics.Debug.WriteLine($"[HomeVM] Got {favorites.Count} favorite tracks");

            if (favorites.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[HomeVM] No favorites, keeping mock data");
                return;
            }

            QuickAccessItems.Clear();

            var selectedTracks = favorites.Count > 6
                ? favorites.OrderBy(_ => Random.Shared.Next()).Take(6).ToList()
                : favorites;

            foreach (var track in selectedTracks)
            {
                QuickAccessItems.Add(new HomeItem(this)
                {
                    Id = track.Id,
                    Title = track.Title,
                    Subtitle = track.ArtistName,
                    ImageUrl = track.ImageUrl,
                    FileUri = track.FileUri
                });
            }
            System.Diagnostics.Debug.WriteLine($"[HomeVM] QuickAccessItems now has {QuickAccessItems.Count} items");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeVM] LoadFavoriteTracksAsync error: {ex.Message}");
        }
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
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string FileUri { get; set; } = string.Empty;
    public HomePageViewModel Parent { get; }

    public HomeItem(HomePageViewModel parent)
    {
        Parent = parent;
    }
}
