using Innowise.Music.ViewModel;

namespace Innowise.Music.View;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LoginPageViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}
