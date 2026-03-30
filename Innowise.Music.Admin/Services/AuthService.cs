using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Concurrent;

namespace Innowise.Music.Admin.Services;

public class AuthService : IAuthService
{
    // Static token cache for development (per user session)
    private static readonly ConcurrentDictionary<string, string> TokenCache = new();
    
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
        
        // Log the full exception for debugging
        _httpClient.DefaultRequestHeaders.Clear();
        
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

            // Log the full URL for debugging
            var requestUrl = new Uri(_httpClient.BaseAddress!, "authentication/login").ToString();
            _logger.LogInformation("Posting to URL: {Url}", requestUrl);
            
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

                AuthResponse? authResponse = null;
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
                    
                    // Clear any existing token and claims before saving new one
                    ClearToken();
                    _token = null;
                    _claimsPrincipal = null;
                    
                    _token = authResponse.Token;
                    SaveToken(_token);
                    ParseToken();
                    OnAuthenticationStateChanged?.Invoke();
                    _logger.LogInformation("Login successful, token saved and parsed. IsAdmin: {IsAdmin}", _claimsPrincipal?.IsInRole("Administrator"));
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
            _logger.LogWarning("IsAdminAsync: _claimsPrincipal is null after ParseToken");
            return false;
        }

        var isAdmin = _claimsPrincipal.IsInRole("Administrator");
        _logger.LogInformation("IsAdminAsync: IsInRole('Administrator') = {IsAdmin}", isAdmin);
        
        // Log all identity names for debugging
        foreach (var identity in _claimsPrincipal.Identities)
        {
            _logger.LogInformation("Identity: AuthenticationType={AuthType}, IsAuthenticated={IsAuth}, RoleClaimType={RoleType}",
                identity.AuthenticationType,
                identity.IsAuthenticated,
                identity.RoleClaimType);
            
            foreach (var claim in identity.Claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role))
            {
                _logger.LogInformation("  Role Claim: Type={Type}, Value={Value}", claim.Type, claim.Value);
            }
        }
        
        return await Task.FromResult(isAdmin);
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
            
            // Log all claims for debugging
            _logger.LogInformation("Token claims:");
            foreach (var claim in token.Claims)
            {
                _logger.LogInformation("  {Type}: {Value}", claim.Type, claim.Value);
            }
            
            // Create identity with role claim type set to ClaimTypes.Role for IsInRole compatibility
            var identity = new ClaimsIdentity(token.Claims, "JWT", "name", ClaimTypes.Role);
            
            // Also add role claims from the token if they exist with "role" type
            foreach (var roleClaim in token.Claims.Where(c => c.Type == "role" || c.Type == "roles"))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
            }
            
            _claimsPrincipal = new ClaimsPrincipal(identity);
            
            _logger.LogInformation("Token parsed successfully. Claims count: {Count}, IsInRole('Administrator'): {IsAdmin}",
                token.Claims.Count(),
                _claimsPrincipal.IsInRole("Administrator"));
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "Failed to parse token");
            // Token is invalid or expired, clear it
            _token = null;
            _claimsPrincipal = null;
            ClearToken();
        }
    }

    private void SaveToken(string token)
    {
        // Use in-memory cache instead of session to avoid size limitations
        var sessionId = GetSessionId();
        if (!string.IsNullOrEmpty(sessionId))
        {
            TokenCache[sessionId] = token;
            _logger.LogInformation("Token saved to cache for session {SessionId}", sessionId);
        }
    }

    private void LoadToken()
    {
        var sessionId = GetSessionId();
        if (!string.IsNullOrEmpty(sessionId) && TokenCache.TryGetValue(sessionId, out var token))
        {
            _token = token;
            if (!string.IsNullOrEmpty(_token))
            {
                ParseToken();
            }
        }
    }

    private void ClearToken()
    {
        var sessionId = GetSessionId();
        if (!string.IsNullOrEmpty(sessionId))
        {
            TokenCache.TryRemove(sessionId, out _);
        }
    }

    private string? GetSessionId()
    {
        // Use the session ID if available, otherwise use a circuit ID
        if (_httpContextAccessor.HttpContext?.Session?.Id != null)
        {
            return _httpContextAccessor.HttpContext.Session.Id;
        }
        
        // For Blazor Server, use the circuit ID as fallback
        return _httpContextAccessor.HttpContext?.Connection.Id;
    }

    private class AuthResponse
    {
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? RefreshToken { get; set; }
        public string? UserId { get; set; }
    }
}
