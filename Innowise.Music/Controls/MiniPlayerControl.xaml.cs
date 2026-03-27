using Innowise.Music.Services;
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
        var audioService = this.Handler.MauiContext.Services.GetService<IAudioService>();
        BindingContext = this.Handler.MauiContext.Services.GetService<MiniPlayerViewModel>();
        audioService.Initialize(mediaElement);
    }
}
