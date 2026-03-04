namespace Innowise.Music.View;

public partial class LibraryPage : ContentPage
{
	public LibraryPage(ViewModel.LibraryPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}