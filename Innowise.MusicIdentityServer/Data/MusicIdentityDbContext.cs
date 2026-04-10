using Innowise.MusicIdentityServer.Models.Music;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Innowise.MusicIdentityServer.Data;

public partial class MusicIdentityDbContext : IdentityDbContext<ApiUser>
{
    // Music tables DbSets
    public DbSet<Artist> Artists { get; set; }
    public DbSet<Album> Albums { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<UserRecentTrack> UserRecentTracks { get; set; }
    public DbSet<UserFavoriteTrack> UserFavoriteTracks { get; set; }

    public MusicIdentityDbContext()
    {
        
    }

    public MusicIdentityDbContext(DbContextOptions<MusicIdentityDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=MusicIdentity;User Id=admin;Password=admin");
        }
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Name = "User",
                NormalizedName = "USER",
                Id = "0e543f8c-0093-4aa1-ad0b-18368c9b099d"
            },
            new IdentityRole
            {
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR",
                Id = "95c93ace-7651-44c4-8737-52851d614f32"
            }
            );

        var hasher = new PasswordHasher<ApiUser>();

        modelBuilder.Entity<ApiUser>().HasData(
            new ApiUser
            {
                Id = "610268a8-2b23-494e-856c-6bba84e7ebcc",
                Email = "admin@innowisemusic.com",
                NormalizedEmail = "ADMIN@INNOWISEMUSIC.COM",
                UserName = "admin@innowisemusic.com",
                NormalizedUserName = "ADMIN@INNOWISEMUSIC.COM",
                FirstName = "System",
                LastName = "Admin",
                PasswordHash = hasher.HashPassword(null, "P@ssword1")

            },
            new ApiUser
            {
                Id = "cf833103-d733-4402-b00c-1263ca230e72",
                Email = "user@innowisemusic.com",
                NormalizedEmail = "USER@INNOWISEMUSIC.COM",
                UserName = "user@innowisemusic.com",
                NormalizedUserName = "USER@INNOWISEMUSIC.COM",
                FirstName = "System",
                LastName = "User",
                PasswordHash = hasher.HashPassword(null, "P@ssword1")
            }
            );

        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = "0e543f8c-0093-4aa1-ad0b-18368c9b099d",
                UserId = "cf833103-d733-4402-b00c-1263ca230e72"
            },
            new IdentityUserRole<string>
            {
                RoleId = "95c93ace-7651-44c4-8737-52851d614f32",
                UserId = "610268a8-2b23-494e-856c-6bba84e7ebcc"
            }
        );

        // Configure Music entities
        ConfigureMusicEntities(modelBuilder);

        OnModelCreatingPartial(modelBuilder);
    }

    private void ConfigureMusicEntities(ModelBuilder modelBuilder)
    {
        // Artist configuration
        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasIndex(e => e.Name).HasMethod("GIN")
                .HasOperators("gin_trgm_ops");
        });

        // Album configuration
        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasIndex(e => e.Title).HasMethod("GIN")
                .HasOperators("gin_trgm_ops");
            
            entity.HasIndex(e => e.ArtistId);
        });

        // Track configuration
        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasIndex(e => e.Title).HasMethod("GIN")
                .HasOperators("gin_trgm_ops");
            
            entity.HasIndex(e => e.ArtistId);
            entity.HasIndex(e => e.AlbumId);
            entity.HasIndex(e => e.PlayCount).IsDescending(true);
        });

        // Genre configuration
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Track-Genre many-to-many relationship
        modelBuilder.Entity<Track>()
            .HasMany(t => t.Genres)
            .WithMany(g => g.Tracks)
            .UsingEntity<Dictionary<string, object>>(
                "TrackGenres",
                j => j.HasOne<Genre>().WithMany().HasForeignKey("GenreId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Track>().WithMany().HasForeignKey("TrackId").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("TrackId", "GenreId");
                });

        // UserRecentTrack configuration
        modelBuilder.Entity<UserRecentTrack>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.PlayedAt });
        });

        // UserFavoriteTrack configuration
        modelBuilder.Entity<UserFavoriteTrack>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.TrackId }).IsUnique();
        });
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

}
