namespace Innowise.Music.View;

public partial class HomePage : ContentPage
{
	public HomePage(ViewModel.HomePageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}