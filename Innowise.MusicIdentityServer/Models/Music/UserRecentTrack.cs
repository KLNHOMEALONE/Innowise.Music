using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Innowise.MusicIdentityServer.Data;

namespace Innowise.MusicIdentityServer.Models.Music;

public class UserRecentTrack
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual ApiUser User { get; set; } = null!;

    [Required]
    public Guid TrackId { get; set; }

    [ForeignKey(nameof(TrackId))]
    public virtual Track Track { get; set; } = null!;

    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}
