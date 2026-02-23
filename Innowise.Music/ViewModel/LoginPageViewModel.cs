using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Innowise.Music.ViewModel;

public partial class LoginPageViewModel : ObservableObject
{
    [RelayCommand]
    private async Task SignUp()
    {
        await Shell.Current.GoToAsync(nameof(View.SignUpPage));
    }
}
