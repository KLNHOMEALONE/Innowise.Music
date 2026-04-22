/*
 * @file: DialogService.cs
 * @description: Implementation of IDialogService using MAUI's DisplayAlert.
 * @dependencies: IDialogService
 * @created: 2026-04-22
 */
namespace Innowise.Music.Services;

public class DialogService : IDialogService
{
    public Task ShowAlertAsync(string title, string message, string cancel)
    {
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(title, message, cancel);
            }
        });
    }

    public Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
    {
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Shell.Current != null)
            {
                return await Shell.Current.DisplayAlert(title, message, accept, cancel);
            }
            return false;
        });
    }
}
