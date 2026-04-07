using System.ComponentModel.DataAnnotations;

namespace Innowise.Music.Admin.Models;

public class Artist
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Artist name is required")]
    public string Name { get; set; } = string.Empty;

    public string? Biography { get; set; }
    public string? ImageUrl { get; set; }
    public bool Verified { get; set; }
    public long MonthlyListeners { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
