namespace Innowise.Music.View;

public partial class HomePage : ContentPage
{
	public HomePage(ViewModel.HomePageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModel.HomePageViewModel viewModel)
        {
            viewModel.RefreshUserName();
        }
    }
}