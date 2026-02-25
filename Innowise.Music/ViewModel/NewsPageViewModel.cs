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
    private readonly INavigationService _navigationService;
    public ObservableCollection<News> NewsCollection { get; set; } = new ObservableCollection<News>();
    [ObservableProperty]
    private News _selectedNews;
    public NewsPageViewModel(WebNewsService newsService, INavigationService navigationService)
    {
        _newsService = newsService;
        _navigationService = navigationService;
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

        _navigationService.NavigateToAsync(nameof(NewsDetailedPage), new Dictionary<string, object>()
        {
            {"News", SelectedNews}
        });
        SelectedNews = null;
    }
}