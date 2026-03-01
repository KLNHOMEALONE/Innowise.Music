using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innowise.MusicIdentityServer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "610268a8-2b23-494e-856c-6bba84e7ebcc",
                columns: new[] { "ConcurrencyStamp", "Email", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "d9a59dbf-96e8-4996-8fae-601f4fe5aa2e", "admin@innowisemusic.com", "ADMIN@INNOWISEMUSIC.COM", "ADMIN@INNOWISEMUSIC.COM", "AQAAAAIAAYagAAAAEBrNDfcIvKyzIQ8pIGzo3rmzv59b7z40eNu3ftSSGZNBgHe4J0ds106S9p7SPZnQaA==", "986ce90c-fa8e-4582-870b-e7cbdf7de073", "admin@innowisemusic.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "cf833103-d733-4402-b00c-1263ca230e72",
                columns: new[] { "ConcurrencyStamp", "Email", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "9e85bfaf-2e8b-4984-9687-99cc5494802d", "user@innowisemusic.com", "USER@INNOWISEMUSIC.COM", "USER@INNOWISEMUSIC.COM", "AQAAAAIAAYagAAAAEHfYsi7zCQXTnhhE/0IAu1sTY4380lG5Q5jnTo3XAJ0UEUjfEMg2tuSu3BaT2l1aOg==", "0bf0a667-428d-4329-b0e7-e43bba4e84f9", "user@innowisemusic.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "610268a8-2b23-494e-856c-6bba84e7ebcc",
                columns: new[] { "ConcurrencyStamp", "Email", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "db7d5203-2559-4d82-8ccd-ed380d5090bb", "admin@inowisemusic.com", "AQAAAAIAAYagAAAAEEAKq4ZzIyht+Uk7YhaRaNB3LaUDAHjq5uM0o6wOEump/SIw5X0Db3JnQ/FIzRywrQ==", "2f9ec1a0-f10f-4218-881c-10a631b0c803", "admin@inowisemusic.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "cf833103-d733-4402-b00c-1263ca230e72",
                columns: new[] { "ConcurrencyStamp", "Email", "NormalizedEmail", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "64063d43-1b7e-4419-a74e-3ae7d62634e4", "user@inowisemusic.com", "USER@BINNOWISEMUSIC.COM", "AQAAAAIAAYagAAAAEGzjpMIaJMbeqbYixfwFJr5Fpqcqq1T2SePXAuMBR4ygbMrvdg4Uvf6wNwBQ6lZ/0A==", "28aa5441-a242-4fbf-ba15-1f3ef161cdfd", "user@inowisemusic.com" });
        }
    }
}
