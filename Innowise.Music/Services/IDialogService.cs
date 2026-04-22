/*
 * @file: IDialogService.cs
 * @description: Interface for cross-platform dialog services.
 * @dependencies: None
 * @created: 2026-04-22
 */
namespace Innowise.Music.Services;

public interface IDialogService
{
    Task ShowAlertAsync(string title, string message, string cancel);
    Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel);
}
