using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Innowise.Music.Admin.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string? _token;
    private ClaimsPrincipal? _claimsPrincipal;

    public event Action? OnAuthenticationStateChanged;

    public AuthService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        LoadToken();
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("authentication/login", new
            {
                email,
                password
            });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (authResponse?.Token != null)
                {
                    _token = authResponse.Token;
                    SaveToken(_token);
                    ParseToken();
                    OnAuthenticationStateChanged?.Invoke();
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public Task LogoutAsync()
    {
        _token = null;
        _claimsPrincipal = null;
        ClearToken();
        OnAuthenticationStateChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_token));
    }

    public async Task<bool> IsAdminAsync()
    {
        if (_claimsPrincipal == null)
        {
            ParseToken();
        }

        if (_claimsPrincipal == null)
        {
            return false;
        }

        return await Task.FromResult(_claimsPrincipal.IsInRole("Administrator"));
    }

    public string? GetToken()
    {
        return _token;
    }

    private void ParseToken()
    {
        if (string.IsNullOrEmpty(_token))
        {
            _claimsPrincipal = null;
            return;
        }

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(_token);
        var identity = new ClaimsIdentity(token.Claims, "JWT");
        _claimsPrincipal = new ClaimsPrincipal(identity);
    }

    private void SaveToken(string token)
    {
        // In Blazor Server, we can use session or cookies
        // For simplicity, using session
        if (_httpContextAccessor.HttpContext?.Session != null)
        {
            _httpContextAccessor.HttpContext.Session.SetString("auth_token", token);
        }
    }

    private void LoadToken()
    {
        if (_httpContextAccessor.HttpContext?.Session != null)
        {
            _token = _httpContextAccessor.HttpContext.Session.GetString("auth_token");
            if (!string.IsNullOrEmpty(_token))
            {
                ParseToken();
            }
        }
    }

    private void ClearToken()
    {
        if (_httpContextAccessor.HttpContext?.Session != null)
        {
            _httpContextAccessor.HttpContext.Session.Remove("auth_token");
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
