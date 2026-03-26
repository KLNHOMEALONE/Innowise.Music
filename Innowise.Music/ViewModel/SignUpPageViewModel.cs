using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Services;
using Innowise.Music.Validations;
using Innowise.Music.Validations.Rules;

namespace Innowise.Music.ViewModel;

public partial class SignUpPageViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    private ValidatableObject<string> _email;

    [ObservableProperty]
    private ValidatableObject<string> _password;

    [ObservableProperty]
    private ValidatableObject<string> _repeatPassword;

    [ObservableProperty]
    private ValidatableObject<string> _firstName;

    [ObservableProperty]
    private ValidatableObject<string> _lastName;

    public SignUpPageViewModel(INavigationService navigationService, IAuthenticationService authenticationService)
    {
        _navigationService = navigationService;
        _authenticationService = authenticationService;

        _email = new ValidatableObject<string>();
        _password = new ValidatableObject<string>();
        _repeatPassword = new ValidatableObject<string>();
        _firstName = new ValidatableObject<string>();
        _lastName = new ValidatableObject<string>();

        AddValidationRules();
    }

    private void AddValidationRules()
    {
        _email.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Email cannot be empty." });
        _email.Validations.Add(new EmailRule<string> { ValidationMessage = "Invalid email format." });
        _password.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Password cannot be empty." });
        _repeatPassword.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Repeat password cannot be empty." });
        _repeatPassword.Validations.Add(new CompareRule<string>(() => Password.Value) { ValidationMessage = "Passwords do not match." });
        _firstName.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "First name cannot be empty." });
        _lastName.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Last name cannot be empty." });
    }

    private bool Validate()
    {
        return _email.Validate() && _password.Validate() && _repeatPassword.Validate() && _firstName.Validate() && _lastName.Validate();
    }

    [RelayCommand]
    private void ValidateEmail() => _email.Validate();

    [RelayCommand]
    private void ValidatePassword() => _password.Validate();

    [RelayCommand]
    private void ValidateRepeatPassword() => _repeatPassword.Validate();

    [RelayCommand]
    private void ValidateFirstName() => _firstName.Validate();

    [RelayCommand]
    private void ValidateLastName() => _lastName.Validate();

    [RelayCommand]
    private async Task SignUp()
    {
        if (!Validate())
        {
            return;
        }

        var success = await _authenticationService.RegisterAsync(new Model.UserDto
        {
            Email = Email.Value,
            Password = Password.Value,
            FirstName = FirstName.Value,
            LastName = LastName.Value
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
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task GoogleLogin()
    {
        // TODO: Implement Google login
        await Task.CompletedTask;
    }
}
