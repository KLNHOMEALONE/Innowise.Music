using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Services;

namespace Innowise.Music.ViewModel;

public partial class LoginPageViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public LoginPageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task Login()
    {
        // TODO: Implement login logic
    }

    [RelayCommand]
    private async Task SignUp()
    {
        await _navigationService.NavigateToAsync(nameof(View.SignUpPage));
    }
}
