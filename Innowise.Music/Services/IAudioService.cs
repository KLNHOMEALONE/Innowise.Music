using System;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;

namespace Innowise.Music.Services
{
    public interface IAudioService
    {
        bool IsPlaying { get; }
        TimeSpan Duration { get; }
        TimeSpan Position { get; }

        event Action StateChanged;
        event Action PositionChanged;

        void Initialize(MediaElement player);
        Task Play(string mediaUrl);
        Task Pause();
    }
}
