using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Innowise.MusicIdentityServer.Data;

public partial class MusicIdentityDbContext : IdentityDbContext<ApiUser>
{
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

}