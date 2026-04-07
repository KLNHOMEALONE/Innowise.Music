using System.ComponentModel.DataAnnotations;

namespace Innowise.Music.Admin.Models;

public class Genre
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Genre name is required")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Color { get; set; }
}
