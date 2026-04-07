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

    [ObservableProperty]
    private Track _currentTrack;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private bool _isPlaying;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private TimeSpan _position;

    [ObservableProperty]
    private TimeSpan _duration;

    public bool IsVisible => CurrentTrack != null;

    public MiniPlayerViewModel(
        IAudioService audioService,
        IStreamTokenService streamTokenService)
    {
        _audioService = audioService;
        _streamTokenService = streamTokenService;
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
            if (CurrentTrack != null)
            {
                var streamUrl = CurrentTrack.FileUri;

                if (CurrentTrack.Id != Guid.Empty && streamUrl.Contains("/stream", StringComparison.OrdinalIgnoreCase))
                {
                    var signedToken = await _streamTokenService.GetStreamTokenAsync(CurrentTrack.Id);
                    if (!string.IsNullOrEmpty(signedToken))
                    {
                        var separator = streamUrl.Contains('?') ? "&" : "?";
                        streamUrl = $"{streamUrl}{separator}token={Uri.EscapeDataString(signedToken)}";
                    }
                }

                await _audioService.Play(streamUrl);
            }
        }
    }
    
    public async Task PlayTrack(Track track)
    {
        CurrentTrack = track;
        OnPropertyChanged(nameof(IsVisible));

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
}
