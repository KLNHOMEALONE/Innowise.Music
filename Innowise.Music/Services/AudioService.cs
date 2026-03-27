using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

namespace Innowise.Music.Services
{
    public class AudioService : IAudioService
    {
        private MediaElement _mediaElement;

        public bool IsPlaying => _mediaElement?.CurrentState == MediaElementState.Playing;
        public TimeSpan Duration => _mediaElement?.Duration ?? TimeSpan.Zero;
        public TimeSpan Position => _mediaElement?.Position ?? TimeSpan.Zero;

        public event Action StateChanged;
        public event Action PositionChanged;


        private void OnMediaElementStateChanged(object sender, MediaStateChangedEventArgs e)
        {
            StateChanged?.Invoke();
        }

        private void OnMediaElementPositionChanged(object sender, MediaPositionChangedEventArgs e)
        {
            PositionChanged?.Invoke();
        }


        public void Initialize(MediaElement player)
        {
            _mediaElement = player;
            _mediaElement.StateChanged += OnMediaElementStateChanged;
            _mediaElement.PositionChanged += OnMediaElementPositionChanged;
        }

        public Task Play(string mediaUrl)
        {
            if (_mediaElement == null)
                return Task.CompletedTask;

            if (_mediaElement.Source != null && _mediaElement.Source is UriMediaSource uriMediaSource && uriMediaSource.Uri.ToString() == mediaUrl)
            {
                if (_mediaElement.CurrentState == MediaElementState.Paused)
                {
                    _mediaElement.Play();
                }
            }
            else
            {
                _mediaElement.ShouldAutoPlay = true;
                _mediaElement.Source = MediaSource.FromUri(mediaUrl);
            }

            return Task.CompletedTask;
        }
        public Task Pause()
        {
            if (_mediaElement?.CurrentState == MediaElementState.Playing)
            {
                _mediaElement.Pause();
            }
            return Task.CompletedTask;
        }
        public Task Stop()
        {
            if (_mediaElement != null && _mediaElement.CurrentState != MediaElementState.Stopped)
            {
                 _mediaElement.Stop();
            }
            return Task.CompletedTask;
        }
    }
}
