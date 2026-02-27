/*
 * File: UserDto.cs
 * Description: Data transfer object for user registration.
 * Dependencies: None
 * Created: 2026-02-27
 */

using System.ComponentModel.DataAnnotations;

namespace Innowise.Music.Model;

public class UserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Role { get; set; } = "User";
}
