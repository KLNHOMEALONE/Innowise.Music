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

            _mediaElement.Source = MediaSource.FromUri(mediaUrl);
            _mediaElement.Play();
            return Task.CompletedTask;
        }

        public Task Pause()
        {
            //if (_mediaElement?.CanPause ?? false)
            //{
            //    _mediaElement.Pause();
            //}
            _mediaElement.Pause();
            return Task.CompletedTask;
        }
    }
}
