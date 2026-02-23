using System.ComponentModel.DataAnnotations;

namespace Innowise.MusicIdentityServer.Models.User;

public class LoginUserDto 
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}