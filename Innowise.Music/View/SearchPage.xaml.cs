namespace Innowise.Music.View;

public partial class SearchPage : ContentPage
{
	public SearchPage(ViewModel.SearchPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}