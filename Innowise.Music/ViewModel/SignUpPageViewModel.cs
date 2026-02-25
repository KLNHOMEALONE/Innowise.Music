using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Services;

namespace Innowise.Music.ViewModel;

public partial class SignUpPageViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public SignUpPageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task Login()
    {
        await _navigationService.NavigateAndClearStackAsync(nameof(View.LoginPage));
    }
}
