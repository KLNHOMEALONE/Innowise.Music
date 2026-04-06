namespace Innowise.MusicIdentityServer.Models.User;

public class AuthenticationResponse
{
    public string UserId { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}