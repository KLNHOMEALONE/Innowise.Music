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
        private bool _disposed;

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
            // Clean up existing instance before reinitializing
            if (_mediaElement != null)
            {
                _mediaElement.StateChanged -= OnMediaElementStateChanged;
                _mediaElement.PositionChanged -= OnMediaElementPositionChanged;
            }

            _mediaElement = player;
            _mediaElement.StateChanged += OnMediaElementStateChanged;
            _mediaElement.PositionChanged += OnMediaElementPositionChanged;
        }

        public Task Play(string mediaUrl)
        {
            if (_mediaElement == null)
                return Task.CompletedTask;

            // If paused AND the source URL is the same, just resume — don't reset the source
            var currentUrl = (_mediaElement.Source as UriMediaSource)?.Uri?.ToString();
            if (_mediaElement.CurrentState == MediaElementState.Paused &&
                !string.IsNullOrEmpty(currentUrl) &&
                currentUrl == mediaUrl)
            {
                _mediaElement.Play();
                return Task.CompletedTask;
            }

            // Stop and clean up before playing to release native audio buffers
            // On Android, failing to stop before reassigning the source causes
            // buffer accumulation leading to distortion after multiple plays
            if (_mediaElement.CurrentState == MediaElementState.Playing ||
                _mediaElement.CurrentState == MediaElementState.Paused)
            {
                _mediaElement.Stop();
            }

            // If MediaElement is in Failed state, reset it
            if (_mediaElement.CurrentState == MediaElementState.Failed)
            {
                _mediaElement.Source = null;
            }

            _mediaElement.ShouldAutoPlay = true;
            _mediaElement.Source = MediaSource.FromUri(mediaUrl);

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

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_mediaElement != null)
            {
                _mediaElement.StateChanged -= OnMediaElementStateChanged;
                _mediaElement.PositionChanged -= OnMediaElementPositionChanged;

                if (_mediaElement.CurrentState == MediaElementState.Playing ||
                    _mediaElement.CurrentState == MediaElementState.Paused)
                {
                    _mediaElement.Stop();
                }

                _mediaElement.Source = null;
                _mediaElement = null;
            }

            _disposed = true;
        }
    }
}
