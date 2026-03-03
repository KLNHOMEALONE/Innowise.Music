using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innowise.MusicIdentityServer.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "610268a8-2b23-494e-856c-6bba84e7ebcc",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "RefreshToken", "RefreshTokenExpiryTime", "SecurityStamp" },
                values: new object[] { "e0bd77c0-d01e-47e1-b789-2f2c9f1fe7f2", "AQAAAAIAAYagAAAAEHIVCC3J1iJlxqAk5Hj+bg2wf9gjH42cxfVawh3tPyvrsAyaWl5zpg2m5pRCw2dByQ==", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "89d786fa-13ce-4f1c-9f14-401118238c60" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "cf833103-d733-4402-b00c-1263ca230e72",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "RefreshToken", "RefreshTokenExpiryTime", "SecurityStamp" },
                values: new object[] { "25100c89-0a45-4519-bfef-1263cbde8da2", "AQAAAAIAAYagAAAAEPRLpRaMGqUF7uPSxR1TqHQHdkxKBvoC9vPw/q4iMNqX2HK/j6emiZ86NLC9epQzlQ==", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "a76deaeb-0d90-4886-8306-2c764340d76a" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "610268a8-2b23-494e-856c-6bba84e7ebcc",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d9a59dbf-96e8-4996-8fae-601f4fe5aa2e", "AQAAAAIAAYagAAAAEBrNDfcIvKyzIQ8pIGzo3rmzv59b7z40eNu3ftSSGZNBgHe4J0ds106S9p7SPZnQaA==", "986ce90c-fa8e-4582-870b-e7cbdf7de073" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "cf833103-d733-4402-b00c-1263ca230e72",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9e85bfaf-2e8b-4984-9687-99cc5494802d", "AQAAAAIAAYagAAAAEHfYsi7zCQXTnhhE/0IAu1sTY4380lG5Q5jnTo3XAJ0UEUjfEMg2tuSu3BaT2l1aOg==", "0bf0a667-428d-4329-b0e7-e43bba4e84f9" });
        }
    }
}
