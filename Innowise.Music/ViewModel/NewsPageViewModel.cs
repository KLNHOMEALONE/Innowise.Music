using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Innowise.Music.Model;
using Innowise.Music.Services;
using NewsDetailedPage = Innowise.Music.View.NewsDetailedPage;

namespace Innowise.Music.ViewModel;

public partial class NewsPageViewModel : ObservableObject
{
    private readonly WebNewsService _newsService;
    public ObservableCollection<News> NewsCollection { get; set; } = new ObservableCollection<News>();
    [ObservableProperty]
    private News _selectedNews;
    public NewsPageViewModel(WebNewsService newsService)
    {
        _newsService = newsService;
        GetNewsList();
    }

    private async void GetNewsList()
    {
        var news = await _newsService.GetNewsAsync();
        foreach (var newsItem in news)
        {
            NewsCollection.Add(newsItem);
        }
    }
    [RelayCommand]
    private void GoToDetails()
    {
        if (SelectedNews == null) return;

        Shell.Current.GoToAsync($"{nameof(NewsDetailedPage)}", new Dictionary<string, object>()
        {
            {"News", SelectedNews}
        });
        SelectedNews = null;
    }
}