using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Services;
using Innowise.Music.Model;
using System.ComponentModel;
using System;

namespace Innowise.Music.ViewModel;

public partial class MiniPlayerViewModel : ObservableObject
{
    private readonly IAudioService _audioService;
    private readonly IStreamTokenService _streamTokenService;
    private readonly IHistoryService _historyService;
    private readonly IFavoriteService _favoriteService;

    [ObservableProperty]
    private Track _currentTrack;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private bool _isPlaying;

    [ObservableProperty]
    private bool _isFavorite;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private TimeSpan _position;

    [ObservableProperty]
    private TimeSpan _duration;

    public bool IsVisible => CurrentTrack != null;

    public MiniPlayerViewModel(
        IAudioService audioService,
        IStreamTokenService streamTokenService,
        IHistoryService historyService,
        IFavoriteService favoriteService)
    {
        _audioService = audioService;
        _streamTokenService = streamTokenService;
        _historyService = historyService;
        _favoriteService = favoriteService;
        _audioService.StateChanged += OnAudioServiceStateChanged;
        _audioService.PositionChanged += OnAudioServicePositionChanged;
    }

    private void OnAudioServiceStateChanged()
    {
        IsPlaying = _audioService.IsPlaying;
        Duration = _audioService.Duration;
        Position = _audioService.Position;
        UpdateProgress();
    }

    private void OnAudioServicePositionChanged()
    {
        Position = _audioService.Position;
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        if (_audioService.Duration > TimeSpan.Zero)
        {
            Progress = _audioService.Position.TotalSeconds / _audioService.Duration.TotalSeconds;
        }
        else
        {
            Progress = 0;
        }
    }

    [RelayCommand]
    private async Task TogglePlayPause()
    {
        if (IsPlaying)
        {
            await _audioService.Pause();
        }
        else
        {
            // Resume current track from paused position without resetting the source
            await _audioService.Resume();
        }
    }

    [RelayCommand]
    private async Task ToggleFavorite()
    {
        if (CurrentTrack?.Id == null || CurrentTrack.Id == Guid.Empty)
            return;

        var result = await _favoriteService.ToggleFavoriteAsync(CurrentTrack.Id);
        IsFavorite = result;
    }

    public async Task PlayTrack(Track track)
    {
        CurrentTrack = track;
        OnPropertyChanged(nameof(IsVisible));

        // Check favorite status for the new track
        _ = RefreshFavoriteStatusAsync(track.Id);

        // Record listening history and notify subscribers
        _ = RefreshHistoryAsync(track.Id);

        var streamUrl = track.FileUri;

        // If this is an API stream URL and we have a track ID, try to get a signed token
        if (track.Id != Guid.Empty && streamUrl.Contains("/stream", StringComparison.OrdinalIgnoreCase))
        {
            var signedToken = await _streamTokenService.GetStreamTokenAsync(track.Id);
            if (!string.IsNullOrEmpty(signedToken))
            {
                var separator = streamUrl.Contains('?') ? "&" : "?";
                streamUrl = $"{streamUrl}{separator}token={Uri.EscapeDataString(signedToken)}";
            }
        }

        await _audioService.Play(streamUrl);
    }

    private async Task RefreshHistoryAsync(Guid trackId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[MiniPlayer] Recording history for track {trackId}");
            await _historyService.RecordPlayAsync(trackId);
            // Notify any subscribers (e.g. HomePageViewModel) to refresh recent items
            MessagingCenter.Send(this, "RecentTracksChanged");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MiniPlayer] Error refreshing history: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"[MiniPlayer] Inner: {ex.InnerException.Message}");
            }
        }
    }

    private async Task RefreshFavoriteStatusAsync(Guid trackId)
    {
        try
        {
            IsFavorite = await _favoriteService.IsFavoriteAsync(trackId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MiniPlayer] Error checking favorite status: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
/*
 *  Track favorite info shall be extracted with the track aty the same time (like using join in SQL) to reduce the number of calls to the service.
 *  It should be like a common best practice pattern for mobile devewlopment
 */
