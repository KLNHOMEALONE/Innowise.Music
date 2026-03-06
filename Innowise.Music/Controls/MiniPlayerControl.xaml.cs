using Innowise.Music.ViewModel;
using System;

namespace Innowise.Music.Controls;

public partial class MiniPlayerControl : ContentView
{
	public MiniPlayerControl()
	{
		InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, EventArgs e)
    {
        BindingContext = this.Handler.MauiContext.Services.GetService<MiniPlayerViewModel>();
    }
}
