/*
 * @file: IGoogleAuthService.cs
 * @description: Interface for Google authentication service.
 * @dependencies: -
 * @created: 2026-03-18
 */
namespace Innowise.Music.Services
{
    public interface IGoogleAuthService
    {
        Task<string> AcquireTokenAsync();
    }
}
