/*
 * File: LoginUserDto.cs
 * Description: Data transfer object for user login.
 * Dependencies: None
 * Created: 2026-02-27
 */

using System.ComponentModel.DataAnnotations;

namespace Innowise.Music.Model;

public class LoginUserDto 
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}
