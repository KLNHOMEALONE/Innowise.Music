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
    private readonly IDialogService _dialogService;
    private readonly IHealthCheckService _healthCheckService;

    [ObservableProperty]
    private ValidatableObject<string> _email;

    [ObservableProperty]
    private ValidatableObject<string> _password;

    [ObservableProperty]
    private bool _isBusy;

    private bool _initialHealthCheckDone;

    public LoginPageViewModel(
        INavigationService navigationService, 
        IAuthenticationService authenticationService, 
        IGoogleAuthService googleAuthService,
        IDialogService dialogService,
        IHealthCheckService healthCheckService)
    {
        _navigationService = navigationService;
        _authenticationService = authenticationService;
        _googleAuthService = googleAuthService;
        _dialogService = dialogService;
        _healthCheckService = healthCheckService;

        Email = new ValidatableObject<string>();
        Password = new ValidatableObject<string>();

        AddValidationRules();
    }

    public async Task InitializeAsync()
    {
        if (!_initialHealthCheckDone)
        {
            await EnsureServerIsReachableAsync(showAlert: true);
            _initialHealthCheckDone = true;
        }
    }

    private async Task<bool> EnsureServerIsReachableAsync(bool showAlert = true)
    {
        System.Diagnostics.Debug.WriteLine("[LoginVM] Performing health check...");
        var isHealthy = await _healthCheckService.CheckIdentityServerHealthAsync();
        
        if (!isHealthy && showAlert)
        {
            System.Diagnostics.Debug.WriteLine("[LoginVM] Health check failed, showing alert.");
            await _dialogService.ShowAlertAsync("Connection Issue", 
                "The Identity Server is unreachable. Please check your connection or try again later.", "OK");
        }
        
        System.Diagnostics.Debug.WriteLine($"[LoginVM] Health check result: {isHealthy}");
        return isHealthy;
    }

    private void AddValidationRules()
    {
        Email.Validations.Add(new EmailRule<string> { ValidationMessage = "Invalid email format." });
        Password.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Password cannot be empty." });
    }

    private bool Validate()
    {
        return Email.Validate() && Password.Validate();
    }

    [RelayCommand]
    private void ValidateEmail() => _email.Validate();

    [RelayCommand]
    private void ValidatePassword() => _password.Validate();

    [RelayCommand]
    private async Task Login()
    {
        if (IsBusy) return;
        
        if (!Validate())
        {
            return;
        }

        try
        {
            IsBusy = true;

            // Check health before proceeding
            if (!await EnsureServerIsReachableAsync(showAlert: true))
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
                await _dialogService.ShowAlertAsync("Login Failed", "Invalid email or password. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
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
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Check health before proceeding
            if (!await EnsureServerIsReachableAsync(showAlert: true))
            {
                return;
            }

            var userInfoDto = await _googleAuthService.AcquireTokenAsync();
            if (string.IsNullOrEmpty(userInfoDto.Token))
            {
                await _dialogService.ShowAlertAsync("Google Login", "Failed to retrieve information from Google.", "OK");
                return;
            }

            var success = await _authenticationService.GoogleLoginAsync(userInfoDto);
            if (success)
            {
                await _navigationService.NavigateToAsync($"///{nameof(View.HomePage)}");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Login Failed", "Failed to authenticate with Identity Server using Google.", "OK");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Google login error: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
