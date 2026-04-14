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
    private readonly int _pageSize;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    private bool _isSearching;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    [NotifyPropertyChangedFor(nameof(IsPaginationVisible))]
    private int _totalPages = 1;

    [ObservableProperty]
    private bool _hasResults;

    public bool IsPaginationVisible => TotalPages > 1;

    public ObservableCollection<string> FilterChips { get; } = new()
    {
        "Artists", "Songs", "Albums", "Playlists"
    };

    public ObservableCollection<SearchResultItem> SearchResults { get; } = new();

    public SearchPageViewModel(
        ISearchService searchService,
        IAudioService audioService,
        IFavoriteService favoriteService,
        MiniPlayerViewModel miniPlayerViewModel,
        Microsoft.Extensions.Options.IOptions<Innowise.Music.Configuration.ApiSettings> apiSettings)
    {
        _searchService = searchService;
        _audioService = audioService;
        _favoriteService = favoriteService;
        _miniPlayerViewModel = miniPlayerViewModel;
        _pageSize = apiSettings.Value.SearchPageSize;
    }

    async partial void OnSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
        {
            SearchResults.Clear();
            CurrentPage = 1;
            TotalPages = 1;
            HasResults = false;
            return;
        }

        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Delay(500, _searchCancellationTokenSource.Token);
            CurrentPage = 1;
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
            HasResults = false;
            return;
        }

        IsSearching = true;
        
        try
        {
            System.Diagnostics.Debug.WriteLine($"[SearchVM] Searching: {SearchQuery}, Page: {CurrentPage}");
            var response = await _searchService.UnifiedSearchAsync(SearchQuery, CurrentPage, _pageSize);

            if (response != null)
            {
                SearchResults.Clear();
                ProcessResponse(response);
                
                int totalItems = response.TotalTracks + response.TotalArtists + response.TotalAlbums;
                TotalPages = (int)Math.Ceiling((double)totalItems / _pageSize);
                if (TotalPages == 0) TotalPages = 1;
                
                HasResults = SearchResults.Count > 0;
                System.Diagnostics.Debug.WriteLine($"[SearchVM] Results: {SearchResults.Count}, Total Items: {totalItems}, Total Pages: {TotalPages}");
            }
            else
            {
                SearchResults.Clear();
                HasResults = false;
                TotalPages = 1;
            }
        }
        finally
        {
            IsSearching = false;
            // Force refresh of button states
            NextPageCommand.NotifyCanExecuteChanged();
            PreviousPageCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextPage()
    {
        System.Diagnostics.Debug.WriteLine($"[SearchVM] NextPage clicked. Current: {CurrentPage}, Total: {TotalPages}");
        CurrentPage++;
        await PerformSearchAsync();
    }

    private bool CanGoNext() => CurrentPage < TotalPages && !IsSearching;

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private async Task PreviousPage()
    {
        System.Diagnostics.Debug.WriteLine($"[SearchVM] PreviousPage clicked. Current: {CurrentPage}");
        CurrentPage--;
        await PerformSearchAsync();
    }

    private bool CanGoPrevious() => CurrentPage > 1 && !IsSearching;

    private void ProcessResponse(UnifiedSearchResponse response)
    {
        int tracksAdded = 0, artistsAdded = 0, albumsAdded = 0;

        // Add Artists
        if (response.Artists != null)
        {
            foreach (var artist in response.Artists)
            {
                SearchResults.Add(new SearchResultItem(this, artist));
                artistsAdded++;
            }
        }

        // Add Albums
        if (response.Albums != null)
        {
            foreach (var album in response.Albums)
            {
                SearchResults.Add(new SearchResultItem(this, album));
                albumsAdded++;
            }
        }

        // Add Tracks
        if (response.Tracks != null)
        {
            foreach (var trackDto in response.Tracks)
            {
                var item = new SearchResultItem(this, trackDto, false);
                SearchResults.Add(item);
                tracksAdded++;
                
                _ = UpdateFavoriteStatus(item, trackDto.Id);
            }
        }

        System.Diagnostics.Debug.WriteLine($"[SearchVM] Processed: {tracksAdded} tracks, {artistsAdded} artists, {albumsAdded} albums. Total Results: {SearchResults.Count}");
    }

    private async Task UpdateFavoriteStatus(SearchResultItem item, Guid trackId)
    {
        item.IsFavorited = await _favoriteService.IsFavoriteAsync(trackId);
    }

    [RelayCommand]
    private async Task PlayTrack(SearchResultItem item)
    {
        if (item == null || item.Type != SearchResultType.Track || item.Track == null) return;

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
        SearchResultType.Album => $"Album • {Album?.Title}",
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
