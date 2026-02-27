using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Services;

namespace Innowise.Music.ViewModel;

public partial class SignUpPageViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private string _repeatPassword;

    [ObservableProperty]
    private string _firstName;

    [ObservableProperty]
    private string _lastName;

    public SignUpPageViewModel(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;
    }

    [RelayCommand]
    private async Task SignUp()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || Password != RepeatPassword)
        {
            // Rocket: "Check your numbers, kid! Passwords don't match or something's empty."
            return;
        }

        var success = await _authService.RegisterAsync(new Model.UserDto
        {
            Email = Email,
            Password = Password,
            FirstName = FirstName ?? "User",
            LastName = LastName ?? "Name"
        });

        if (success)
        {
            await _navigationService.NavigateAndClearStackAsync(nameof(View.LoginPage));
        }
        else
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine("Registration failed");
        }
    }

    [RelayCommand]
    private async Task Login()
    {
        await _navigationService.NavigateAndClearStackAsync(nameof(View.LoginPage));
    }
}
