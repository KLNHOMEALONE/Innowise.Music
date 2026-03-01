/*
 * File: AuthenticationService.cs
 * Description: Implementation of IAuthenticationService using HttpClient and SecureStorage.
 * Dependencies: Model\LoginUserDto, Model\UserDto, Model\AuthenticationResponse, Services\IAuthenticationService, Services\HttpHelper
 * Created: 2026-02-27
 */

using System.Net.Http.Json;
using Innowise.Music.Model;

namespace Innowise.Music.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private const string AuthTokenKey = "auth_token";

    public AuthenticationService(HttpHelper httpHelper)
    {
        var handler = httpHelper.GetInsecureHandler();
        _httpClient = new HttpClient(handler);
    }

    private string GetApiUrl(string endpoint)
    {
        var baseUrl = DeviceInfo.Platform == DevicePlatform.Android ? "https://10.0.2.2:7008" : "https://localhost:7008";
        return $"{baseUrl}/api/Authentication/{endpoint}";
    }

    public async Task<bool> LoginAsync(LoginUserDto loginUserDto)
    {
        try
        {
            var url = GetApiUrl("login");
            var response = await _httpClient.PostAsJsonAsync(url, loginUserDto);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
                if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
                {
                    await SecureStorage.Default.SetAsync(AuthTokenKey, authResponse.Token);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            // Log error (Rocket style: "Blasted API is down!")
            System.Diagnostics.Debug.WriteLine($"AuthenticationService Login Error: {ex.Message}");
        }
        return false;
    }

    public async Task<bool> RegisterAsync(UserDto userDto)
    {
        try
        {
            var url = GetApiUrl("register");
            var response = await _httpClient.PostAsJsonAsync(url, userDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AuthenticationService Register Error: {ex.Message}");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Default.Remove(AuthTokenKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(AuthTokenKey);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
        // Note: In a real app, we should also check if the token is expired using JwtSecurityTokenHandler
    }
}
