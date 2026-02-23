using NewsPageViewModel = Innowise.Music.ViewModel.NewsPageViewModel;

namespace Innowise.Music.View;

public partial class NewsPage : ContentPage
{
	public NewsPage(NewsPageViewModel newsViewModel)
	{
		InitializeComponent();
		BindingContext = newsViewModel;
    }
}