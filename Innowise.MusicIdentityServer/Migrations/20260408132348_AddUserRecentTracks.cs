using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innowise.MusicIdentityServer.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRecentTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRecentTracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    TrackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecentTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRecentTracks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRecentTracks_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "610268a8-2b23-494e-856c-6bba84e7ebcc",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "521dbdd2-b1fd-4771-afc7-0b40cdf02461", "AQAAAAIAAYagAAAAEFSUt4CgV1nORxG9KGsxIHvab7mumh4V8fR0tOxObQnPjmK/SojDZRmBlA2h6OEM1g==", "5a949e85-3abd-465e-b35d-af03c0bdd427" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "cf833103-d733-4402-b00c-1263ca230e72",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "de39c30d-d8d1-4bd1-bf9b-194e0dccea21", "AQAAAAIAAYagAAAAEJKXHCLWCfC9nCuYTuxOSc1CrDoL92Rok/IVsuFxXw+ADp7qyQrZ44yWHUAT/cL3Gg==", "450029df-0f31-4949-b7cf-599fc078cacb" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRecentTracks_TrackId",
                table: "UserRecentTracks",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRecentTracks_UserId",
                table: "UserRecentTracks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRecentTracks_UserId_PlayedAt",
                table: "UserRecentTracks",
                columns: new[] { "UserId", "PlayedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRecentTracks");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "610268a8-2b23-494e-856c-6bba84e7ebcc",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "df636591-3690-40b5-adf6-f46473650ac7", "AQAAAAIAAYagAAAAELl2N5XQTjYDlDYG2EOHmLMOqAY0peh4S40bqbpyLun5f/q8Hm8YItuF5MDdgA4ZzA==", "428d7ef9-017a-4f04-8a56-5cf7666574a9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "cf833103-d733-4402-b00c-1263ca230e72",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "caba9527-54c9-48a3-87b2-16417fe4540b", "AQAAAAIAAYagAAAAEEMHJ7tq1kbU/4SlT6Qhljq92nsO81AZlr9GcUUGwh8qPqLVJFw5ge4yOE+wH7X6Qw==", "41080c43-5971-4bec-912d-98552de4ec73" });
        }
    }
}
