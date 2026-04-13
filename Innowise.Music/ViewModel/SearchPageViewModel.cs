using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Model;
using Innowise.Music.Services;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Innowise.Music.ViewModel;

public enum SearchResultType
{
    Track,
    Artist,
    Album
}

public partial class SearchPageViewModel : ObservableObject
{
    private readonly ISearchService _searchService;
    private readonly IAudioService _audioService;
    private readonly IFavoriteService _favoriteService;
    private readonly MiniPlayerViewModel _miniPlayerViewModel;
    private CancellationTokenSource? _searchCancellationTokenSource;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public ObservableCollection<string> FilterChips { get; } = new()
    {
        "Artists", "Songs", "Albums", "Playlists"
    };

    public ObservableCollection<SearchResultItem> SearchResults { get; } = new();

    public SearchPageViewModel(
        ISearchService searchService,
        IAudioService audioService,
        IFavoriteService favoriteService,
        MiniPlayerViewModel miniPlayerViewModel)
    {
        _searchService = searchService;
        _audioService = audioService;
        _favoriteService = favoriteService;
        _miniPlayerViewModel = miniPlayerViewModel;
    }

    async partial void OnSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
        {
            SearchResults.Clear();
            return;
        }

        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Delay(500, _searchCancellationTokenSource.Token);
            await PerformSearchAsync();
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation
        }
    }

    [RelayCommand]
    private async Task PerformSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchResults.Clear();
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[SearchVM] Performing unified search for: {SearchQuery}");
        var response = await _searchService.UnifiedSearchAsync(SearchQuery);

        if (response != null)
        {
            SearchResults.Clear();

            // Add Artists
            if (response.Artists != null)
            {
                foreach (var artist in response.Artists)
                {
                    SearchResults.Add(new SearchResultItem(this, artist));
                }
            }

            // Add Albums
            if (response.Albums != null)
            {
                foreach (var album in response.Albums)
                {
                    SearchResults.Add(new SearchResultItem(this, album));
                }
            }

            // Add Tracks
            if (response.Tracks != null)
            {
                foreach (var trackDto in response.Tracks)
                {
                    var isFavorited = await _favoriteService.IsFavoriteAsync(trackDto.Id);
                    SearchResults.Add(new SearchResultItem(this, trackDto, isFavorited));
                }
            }

            System.Diagnostics.Debug.WriteLine($"[SearchVM] Total results: {SearchResults.Count}");
        }
    }

    [RelayCommand]
    private async Task PlayTrack(SearchResultItem item)
    {
        if (item == null || item.Type != SearchResultType.Track || item.Track == null) return;

        // Use HTTP for stream URLs (MediaElement can't handle self-signed HTTPS certs)
        var streamBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5236"
            : "http://localhost:5236";

        var track = new Track
        {
            Id = item.Track.Id,
            Title = item.Track.Title,
            ArtistName = item.Track.Artist?.Name ?? "Unknown Artist",
            ImageUrl = item.Track.Album?.CoverImageUrl ?? item.Track.Artist?.ImageUrl ?? string.Empty,
            FileUri = $"{streamBaseUrl}/api/Music/tracks/{item.Track.Id}/stream"
        };

        await _miniPlayerViewModel.PlayTrack(track);
    }

    [RelayCommand]
    private async Task ToggleFavorite(SearchResultItem item)
    {
        if (item == null || item.Type != SearchResultType.Track || item.Track == null) return;

        var result = await _favoriteService.ToggleFavoriteAsync(item.Track.Id);
        item.IsFavorited = result;
    }
}

public partial class SearchResultItem : ObservableObject
{
    public SearchPageViewModel Parent { get; }
    public SearchResultType Type { get; }
    public TrackDto? Track { get; }
    public Artist? Artist { get; }
    public Album? Album { get; }
    
    public string Title => Type switch
    {
        SearchResultType.Track => Track?.Title ?? string.Empty,
        SearchResultType.Artist => Artist?.Name ?? string.Empty,
        SearchResultType.Album => Album?.Title ?? string.Empty,
        _ => string.Empty
    };

    public string Subtitle => Type switch
    {
        SearchResultType.Track => $"Song • {Track?.Artist?.Name}",
        SearchResultType.Artist => "Artist",
        SearchResultType.Album => $"Album • {Album?.Title}", // Note: Backend Album doesn't have Artist populated in SearchAlbumsAsync currently, but DTO might have it
        _ => string.Empty
    };

    public string ImageUrl => Type switch
    {
        SearchResultType.Track => Track?.Album?.CoverImageUrl ?? Track?.Artist?.ImageUrl ?? string.Empty,
        SearchResultType.Artist => Artist?.ImageUrl ?? string.Empty,
        SearchResultType.Album => Album?.CoverImageUrl ?? string.Empty,
        _ => string.Empty
    };

    public bool IsTrack => Type == SearchResultType.Track;

    [ObservableProperty]
    private bool _isFavorited;

    // Track Constructor
    public SearchResultItem(SearchPageViewModel parent, TrackDto track, bool isFavorited)
    {
        Parent = parent;
        Type = SearchResultType.Track;
        Track = track;
        IsFavorited = isFavorited;
    }

    // Artist Constructor
    public SearchResultItem(SearchPageViewModel parent, Artist artist)
    {
        Parent = parent;
        Type = SearchResultType.Artist;
        Artist = artist;
    }

    // Album Constructor
    public SearchResultItem(SearchPageViewModel parent, Album album)
    {
        Parent = parent;
        Type = SearchResultType.Album;
        Album = album;
    }
}
