/*
 * @file: WebPageViewModel.cs
 * @description: ViewModel for the WebPage view.
 * @dependencies: CommunityToolkit.Mvvm
 * @created: 2026-03-18
 */
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace Innowise.Music.ViewModel;

[QueryProperty(nameof(Url), "url")]
public partial class WebPageViewModel : ObservableObject
{
    private readonly TaskCompletionSource<string> _taskCompletionSource = new TaskCompletionSource<string>();

    [ObservableProperty]
    private string _url;

    [ObservableProperty]
    private string _authResult;

    partial void OnAuthResultChanged(string value)
    {
        _taskCompletionSource.SetResult(value);
    }

    public Task<string> GetAuthResultAsync()
    {
        return _taskCompletionSource.Task;
    }
}
