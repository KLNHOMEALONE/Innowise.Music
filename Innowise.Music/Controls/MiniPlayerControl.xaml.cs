using Innowise.Music.ViewModel;

namespace Innowise.Music.Controls;

public partial class MiniPlayerControl : ContentView
{
	public MiniPlayerControl()
	{
		InitializeComponent();
		BindingContext = new MiniPlayerViewModel();
	}
}
