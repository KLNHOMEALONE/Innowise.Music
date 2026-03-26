using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Services;
using Innowise.Music.Validations;
using Innowise.Music.Validations.Rules;

namespace Innowise.Music.ViewModel;

public partial class LoginPageViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IGoogleAuthService _googleAuthService;

    [ObservableProperty]
    private ValidatableObject<string> _email;

    [ObservableProperty]
    private ValidatableObject<string> _password;

    public LoginPageViewModel(INavigationService navigationService, IAuthenticationService authenticationService, IGoogleAuthService googleAuthService)
    {
        _navigationService = navigationService;
        _authenticationService = authenticationService;
        _googleAuthService = googleAuthService;

        _email = new ValidatableObject<string>();
        _password = new ValidatableObject<string>();

        AddValidationRules();
    }

    private void AddValidationRules()
    {
        _email.Validations.Add(new EmailRule<string> { ValidationMessage = "Invalid email format." });
        _password.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Password cannot be empty." });
    }

    private bool Validate()
    {
        return _email.Validate() && _password.Validate();
    }

    [RelayCommand]
    private void ValidateEmail() => _email.Validate();

    [RelayCommand]
    private void ValidatePassword() => _password.Validate();

    [RelayCommand]
    private async Task Login()
    {
        if (!Validate())
        {
            return;
        }

        var success = await _authenticationService.LoginAsync(new Model.LoginUserDto
        {
            Email = Email.Value,
            Password = Password.Value
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
        //await _googleAuthService.SignOut();
        var token = await _googleAuthService.AcquireTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine("Google login failed");
            return;
        }
        //await _navigationService.NavigateToAsync($"///{nameof(View.HomePage)}");
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
