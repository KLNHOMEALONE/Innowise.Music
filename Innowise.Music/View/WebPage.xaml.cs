/*
 * @file: WebPage.xaml.cs
 * @description: Code-behind for the WebPage view.
 * @dependencies: WebPageViewModel
 * @created: 2026-03-18
 */
using Innowise.Music.ViewModel;

namespace Innowise.Music.View;

public partial class WebPage : ContentPage
{
    public WebPage(WebPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void WebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("myapp://oauth2redirect"))
        {
            if (Shell.Current is AppShell appShell)
            {
                appShell.SetAuthResult(e.Url);
            }
            await Navigation.PopModalAsync();
        }
    }
}
