using System.Security.Claims;

namespace Innowise.Music.Admin.Services;

public interface IAuthService
{
    Task<(bool Success, ClaimsPrincipal? Principal, string? ErrorMessage)> LoginAndGetPrincipalAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> IsAdminAsync();
    Task<string?> GetTokenAsync();
}
