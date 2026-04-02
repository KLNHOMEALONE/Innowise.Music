using System.Security.Claims;

namespace Innowise.Music.Admin.Services;

public interface IAuthService
{
    Task<(bool, ClaimsPrincipal?)> LoginAndGetPrincipalAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> IsAdminAsync();
    Task<string?> GetTokenAsync();
}
