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

    [ObservableProperty]
    private Track _currentTrack;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private bool _isPlaying;

    [ObservableProperty]
    private double _progress;

    public bool IsVisible => CurrentTrack != null;

    public MiniPlayerViewModel(IAudioService audioService)
    {
        _audioService = audioService;
        _audioService.StateChanged += OnAudioServiceStateChanged;
        _audioService.PositionChanged += OnAudioServicePositionChanged;
    }

    private void OnAudioServiceStateChanged()
    {
        IsPlaying = _audioService.IsPlaying;
    }

    private void OnAudioServicePositionChanged()
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
                await _audioService.Play(CurrentTrack.FileUri);
            }
        }
    }
    
    public async Task PlayTrack(Track track)
    {
        CurrentTrack = track;
        OnPropertyChanged(nameof(IsVisible));
        await _audioService.Play(track.FileUri);
    }
}
