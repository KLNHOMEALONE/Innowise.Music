/*
 * File: AuthenticationService.cs
 * Description: Implementation of IAuthenticationService using HttpClient and SecureStorage.
 * Dependencies: Model\LoginUserDto, Model\UserDto, Model\AuthenticationResponse, Services\IAuthenticationService, Services\HttpHelper
 * Created: 2026-02-27
 */

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Innowise.Music.Configuration;
using Innowise.Music.Model;
using Microsoft.Extensions.Options;

namespace Innowise.Music.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;
    private const string AuthTokenKey = "auth_token";
    private const string RefreshTokenKey = "refresh_token";

    public AuthenticationService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
    {
        _apiSettings = apiSettings.Value;
        _httpClient = httpClient;
    }

    private string GetApiUrl(string endpoint)
    {
        var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? _apiSettings.AndroidBaseUrl
            : _apiSettings.BaseUrl;
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
                    if (!string.IsNullOrEmpty(authResponse.RefreshToken))
                    {
                        await SecureStorage.Default.SetAsync(RefreshTokenKey, authResponse.RefreshToken);
                    }
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
        SecureStorage.Default.Remove(RefreshTokenKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(AuthTokenKey);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(RefreshTokenKey);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        
        // If token expires in more than 1 minute, we are good
        if (jwtSecurityToken.ValidTo > DateTime.UtcNow.AddMinutes(1))
        {
            return true;
        }

        // Token is expired or about to expire, let's try to refresh it
        var refreshToken = await GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
        {
            await LogoutAsync();
            return false;
        }

        try
        {
            var request = new TokenRequestDto
            {
                Token = token,
                RefreshToken = refreshToken
            };

            var url = GetApiUrl("refresh");
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
                if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
                {
                    await SecureStorage.Default.SetAsync(AuthTokenKey, authResponse.Token);
                    if (!string.IsNullOrEmpty(authResponse.RefreshToken))
                    {
                        await SecureStorage.Default.SetAsync(RefreshTokenKey, authResponse.RefreshToken);
                    }
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AuthenticationService Refresh Error: {ex.Message}");
        }

        // If we reach here, refresh failed, so force logout
        await LogoutAsync();
        return false;
    }
}
