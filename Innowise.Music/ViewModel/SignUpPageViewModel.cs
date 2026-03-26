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

        Email = new ValidatableObject<string>();
        Password = new ValidatableObject<string>();
        RepeatPassword = new ValidatableObject<string>();
        FirstName = new ValidatableObject<string>();
        LastName = new ValidatableObject<string>();

        AddValidationRules();
    }

    private void AddValidationRules()
    {
        Email.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Email cannot be empty." });
        Email.Validations.Add(new EmailRule<string> { ValidationMessage = "Invalid email format." });
        Password.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Password cannot be empty." });
        RepeatPassword.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Repeat password cannot be empty." });
        RepeatPassword.Validations.Add(new CompareRule<string>(() => Password.Value) { ValidationMessage = "Passwords do not match." });
        FirstName.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "First name cannot be empty." });
        LastName.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Last name cannot be empty." });
    }

    private bool Validate()
    {
        return Email.Validate() && Password.Validate() && RepeatPassword.Validate() && FirstName.Validate() && LastName.Validate();
    }

    [RelayCommand]
    private void ValidateEmail() => Email.Validate();

    [RelayCommand]
    private void ValidatePassword() => Password.Validate();

    [RelayCommand]
    private void ValidateRepeatPassword() => RepeatPassword.Validate();

    [RelayCommand]
    private void ValidateFirstName() => FirstName.Validate();

    [RelayCommand]
    private void ValidateLastName() => LastName.Validate();

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
