using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Innowise.Music.Admin.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace Innowise.Music.Admin.Services;

public class AuthService : IAuthService
{
    private const string TokenKey = "accessToken";

    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ApiAuthenticationStateProvider _authStateProvider;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        ApiAuthenticationStateProvider authStateProvider,
        ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Clear();
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("AuthService.LoginAsync called with email={Email}", email);

            var payload = new
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/authentication/login", payload,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                });

            _logger.LogInformation("Login response status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Login response content length: {Length}", content.Length);

                AuthResponse? authResponse;
                try
                {
                    authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize response. Raw content: {Content}", content);
                    return false;
                }

                if (authResponse?.Token != null)
                {
                    _logger.LogInformation("Token received, length: {Length}", authResponse.Token.Length);

                    // Store token in localStorage
                    await _localStorage.SetItemAsync(TokenKey, authResponse.Token);

                    // Notify the auth state provider
                    await _authStateProvider.LoggedIn();

                    _logger.LogInformation("Login successful, token saved.");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Login response missing token.");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed. Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login exception");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _authStateProvider.LoggedOut();
        _logger.LogInformation("LogoutAsync: Token cleared, auth state notified.");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsync<string>(TokenKey);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<bool> IsAdminAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            _logger.LogWarning("IsAdminAsync: User is not authenticated");
            return false;
        }

        var isAdmin = user.IsInRole("Administrator");
        _logger.LogInformation("IsAdminAsync: IsInRole('Administrator') = {IsAdmin}", isAdmin);
        return isAdmin;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(TokenKey);
    }

    public string? GetToken()
    {
        // Synchronous fallback - use GetTokenAsync when possible
        try
        {
            return _localStorage.GetItemAsync<string>(TokenKey).GetAwaiter().GetResult();
        }
        catch
        {
            return null;
        }
    }

    private class AuthResponse
    {
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? RefreshToken { get; set; }
        public string? UserId { get; set; }
    }
}
