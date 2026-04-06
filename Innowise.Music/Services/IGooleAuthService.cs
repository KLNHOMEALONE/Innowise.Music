/*
 * @file: IGoogleAuthService.cs
 * @description: Interface for Google authentication service.
 * @dependencies: -
 * @created: 2026-03-18
 */

using Innowise.Music.Model;

namespace Innowise.Music.Services
{
    public interface IGoogleAuthService
    {
        bool IsSignedIn { get; }
        Task<UserInfoDto> AcquireTokenAsync();
        Task SignOut();

    }
}
