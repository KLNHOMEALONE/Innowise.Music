using Innowise.Music.ViewModel;

namespace Innowise.Music.View;

public partial class SignUpPage : ContentPage
{
    public SignUpPage(SignUpPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
