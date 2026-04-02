using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace Innowise.Music.Admin.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authenticationStateProvider,
        IMemoryCache memoryCache,
        ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _authenticationStateProvider = authenticationStateProvider;
        _memoryCache = memoryCache;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Clear();
    }

    public async Task<(bool Success, ClaimsPrincipal? Principal, string? ErrorMessage)> LoginAndGetPrincipalAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting to log in user with email {Email}", email);

            var payload = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("authentication/login", payload);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed. Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
                return (false, null, null);
            }

            var content = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (string.IsNullOrWhiteSpace(authResponse?.Token))
            {
                _logger.LogError("Login response did not contain a token.");
                return (false, null, null);
            }

            var claimsPrincipal = CreateClaimsPrincipalFromToken(authResponse.Token);

            if (!claimsPrincipal.IsInRole("Administrator"))
            {
                _logger.LogWarning("User {Email} attempted to log in but is not an administrator.", email);
                return (false, null, "Access denied. Admin privileges required.");
            }

            var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (!string.IsNullOrEmpty(userId))
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(8));
                _memoryCache.Set(GetCacheKey(userId), authResponse.Token, cacheEntryOptions);
                 _logger.LogInformation("Token for user {userId} cached successfully.", userId);
            }

            return (true, claimsPrincipal, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred during login.");
            return (false, null, null);
        }
    }

    public async Task LogoutAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId))
        {
            _memoryCache.Remove(GetCacheKey(userId));
             _logger.LogInformation("Token for user {userId} removed from cache.", userId);
        }

        if (_httpContextAccessor.HttpContext != null)
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out.");
        }
        else
        {
             _logger.LogError("HttpContext is null, cannot sign out.");
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return _memoryCache.Get<string>(GetCacheKey(userId));
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.Identity?.IsAuthenticated ?? false;
    }

    public async Task<bool> IsAdminAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.IsInRole("Administrator");
    }
    
    private ClaimsPrincipal CreateClaimsPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenContent = tokenHandler.ReadJwtToken(token);

        var claims = tokenContent.Claims.ToList();
        
        if (claims.All(c => c.Type != ClaimTypes.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, tokenContent.Subject ?? string.Empty));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }

    private string GetCacheKey(string userId) => $"AuthToken_{userId}";

    private class AuthResponse
    {
        public string? Token { get; set; }
    }
}
