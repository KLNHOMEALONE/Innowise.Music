/*
 * File: IAuthService.cs
 * Description: Interface for authentication service handling login, registration, and token management.
 * Dependencies: Model\LoginUserDto, Model\UserDto, Model\AuthenticationResponse
 * Created: 2026-02-27
 */

using Innowise.Music.Model;

namespace Innowise.Music.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginUserDto loginUserDto);
    Task<bool> RegisterAsync(UserDto userDto);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<bool> IsAuthenticatedAsync();
}
