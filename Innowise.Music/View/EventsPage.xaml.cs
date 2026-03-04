namespace Innowise.Music.View;

public partial class EventsPage : ContentPage
{
	public EventsPage(ViewModel.EventsPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}