using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Services;

namespace Innowise.Music.ViewModel;

public partial class LoginPageViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IGoogleAuthService _googleAuthService;

    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private string _password;

    public LoginPageViewModel(INavigationService navigationService, IAuthenticationService authenticationService, IGoogleAuthService googleAuthService)
    {
        _navigationService = navigationService;
        _authenticationService = authenticationService;
        _googleAuthService = googleAuthService;
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            // Rocket: "You gotta fill in the blanks, kid!"
            return;
        }

        var success = await _authenticationService.LoginAsync(new Model.LoginUserDto
        {
            Email = Email,
            Password = Password
        });

        if (success)
        {
            await _navigationService.NavigateToAsync($"///{nameof(View.HomePage)}");
        }
        else
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine("Login failed");
        }
    }

    [RelayCommand]
    private async Task SignUp()
    {
        await _navigationService.NavigateToAsync(nameof(View.SignUpPage));
    }

    [RelayCommand]
    private async Task GoogleLogin()
    {
        var token = await _googleAuthService.AcquireTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine("Google login failed");
            return;
        }

        var success = await _authenticationService.GoogleLoginAsync(token);
        if (success)
        {
            await _navigationService.NavigateToAsync($"///{nameof(View.HomePage)}");
        }
        else
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine("Google login failed");
        }
    }
}
