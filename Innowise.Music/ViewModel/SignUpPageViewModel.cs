using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Innowise.Music.ViewModel;

public partial class SignUpPageViewModel : ObservableObject
{
    [RelayCommand]
    private async Task Login()
    {
        await Shell.Current.GoToAsync($"//{nameof(View.LoginPage)}");
    }
}
