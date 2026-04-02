namespace Innowise.Music.Admin.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> IsAdminAsync();
    Task<string?> GetTokenAsync();
    string? GetToken();
}
