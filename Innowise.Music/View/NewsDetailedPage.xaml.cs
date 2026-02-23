using NewsDetailedPageViewModel = Innowise.Music.ViewModel.NewsDetailedPageViewModel;

namespace Innowise.Music.View;

public partial class NewsDetailedPage : ContentPage
{
	public NewsDetailedPage(NewsDetailedPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}