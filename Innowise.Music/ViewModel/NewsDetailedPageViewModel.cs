using CommunityToolkit.Mvvm.ComponentModel;
using Innowise.Music.Model;

namespace Innowise.Music.ViewModel;

[QueryProperty(nameof(News), "News")]
public partial class NewsDetailedPageViewModel : ObservableObject
{
    [ObservableProperty]
    News news;
}

