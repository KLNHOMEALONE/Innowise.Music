using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=MusicIdentity;User Id=admin;Password=admin");

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
                Email = "admin@inowisemusic.com",
                NormalizedEmail = "ADMIN@INNOWISEMUSIC.COM",
                UserName = "admin@inowisemusic.com",
                NormalizedUserName = "ADMIN@INNOWISEMUSIC.COM",
                FirstName = "System",
                LastName = "Admin",
                PasswordHash = hasher.HashPassword(null, "P@ssword1")

            },
            new ApiUser
            {
                Id = "cf833103-d733-4402-b00c-1263ca230e72",
                Email = "user@inowisemusic.com",
                NormalizedEmail = "USER@BINNOWISEMUSIC.COM",
                UserName = "user@inowisemusic.com",
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