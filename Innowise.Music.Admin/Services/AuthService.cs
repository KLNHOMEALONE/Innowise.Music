using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Innowise.Music.Admin.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;
    private string? _token;
    private ClaimsPrincipal? _claimsPrincipal;

    public event Action? OnAuthenticationStateChanged;

    public AuthService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        LoadToken();
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("AuthService.LoginAsync called with email={Email}", email);
            _logger.LogInformation("HTTP Client BaseAddress: {BaseAddress}", _httpClient.BaseAddress);

            // Use PascalCase property names to match LoginUserDto on the backend
            var payload = new
            {
                Email = email,
                Password = password
            };
            
            _logger.LogInformation("Login payload: Email={Email}, Password length={PasswordLength}", payload.Email, payload.Password?.Length);

            var response = await _httpClient.PostAsJsonAsync("authentication/login", payload, 
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // Preserve PascalCase
                });

            // Log response for debugging
            _logger.LogInformation("Login response status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("Login response reason: {ReasonPhrase}", response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Login response content length: {Length}", content.Length);
                _logger.LogInformation("Login response content: {Content}", content);

                var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (authResponse?.Token != null)
                {
                    _logger.LogInformation("Token received, length: {Length}", authResponse.Token.Length);
                    _token = authResponse.Token;
                    SaveToken(_token);
                    ParseToken();
                    OnAuthenticationStateChanged?.Invoke();
                    _logger.LogInformation("Login successful, token saved and parsed.");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Login response missing token. authResponse is null or token is null.");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed. Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
                
                // Try to get more details from response headers
                foreach (var header in response.Headers)
                {
                    _logger.LogWarning("Response header: {Key} = {Value}", header.Key, string.Join(", ", header.Value));
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login exception");
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

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(_token);
            var identity = new ClaimsIdentity(token.Claims, "JWT");
            _claimsPrincipal = new ClaimsPrincipal(identity);
        }
        catch (SecurityTokenException)
        {
            // Token is invalid or expired, clear it
            _token = null;
            _claimsPrincipal = null;
            ClearToken();
        }
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
