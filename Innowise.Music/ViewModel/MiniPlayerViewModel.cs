using CommunityToolkit.Mvvm.ComponentModel;

namespace Innowise.Music.ViewModel;

public partial class MiniPlayerViewModel : ObservableObject
{
    [ObservableProperty]
    private string _trackTitle = "Shade Astray";

    [ObservableProperty]
    private string _artistName = "Invent Animate";

    [ObservableProperty]
    private string _imageUrl = "shade_astray.png";

    [ObservableProperty]
    private bool _isPlaying = true;
}
