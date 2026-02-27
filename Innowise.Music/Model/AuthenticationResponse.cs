/*
 * File: AuthenticationResponse.cs
 * Description: Data transfer object for the authentication response from the server.
 * Dependencies: None
 * Created: 2026-02-27
 */

namespace Innowise.Music.Model;

public class AuthenticationResponse
{
    public string UserId { get; set; }
    public string Token { get; set; }
    public string Email { get; set; }
}
