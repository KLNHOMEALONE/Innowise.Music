using CommunityToolkit.Mvvm.ComponentModel;

namespace Innowise.Music.ViewModel
{
    public partial class AppShellViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _selectedRoute;
    }
}
