using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Innowise.Music.Admin.Auth;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string TokenKey = "accessToken";
    private readonly ILocalStorageService _localStorage;

    public ApiAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        try
        {
            var savedToken = await _localStorage.GetItemAsync<string>(TokenKey);

            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return new AuthenticationState(user);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenContent = tokenHandler.ReadJwtToken(savedToken);

            if (tokenContent.ValidTo < DateTime.UtcNow)
            {
                await _localStorage.RemoveItemAsync(TokenKey);
                return new AuthenticationState(user);
            }

            var claims = tokenContent.Claims.ToList();
            claims.Add(new Claim(ClaimTypes.Name, tokenContent.Subject ?? string.Empty));

            // Map role claims to ClaimTypes.Role
            foreach (var roleClaim in tokenContent.Claims.Where(c => c.Type == "role" || c.Type == "roles"))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
            }

            user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(user);
        }
    }

    public async Task LoggedIn()
    {
        var savedToken = await _localStorage.GetItemAsync<string>(TokenKey);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenContent = tokenHandler.ReadJwtToken(savedToken);

        var claims = tokenContent.Claims.ToList();
        claims.Add(new Claim(ClaimTypes.Name, tokenContent.Subject ?? string.Empty));

        foreach (var roleClaim in tokenContent.Claims.Where(c => c.Type == "role" || c.Type == "roles"))
        {
            claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
        }

        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        var authState = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(authState);
    }

    public async Task LoggedOut()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        var nobody = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(nobody));
        NotifyAuthenticationStateChanged(authState);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<string>(TokenKey);
        }
        catch
        {
            return null;
        }
    }
}
