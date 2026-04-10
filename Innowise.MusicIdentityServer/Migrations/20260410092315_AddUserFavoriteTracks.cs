using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innowise.MusicIdentityServer.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFavoriteTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFavoriteTracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    TrackId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavoriteTracks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteTracks_Tracks_TrackId",
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
                values: new object[] { "17c3a66a-371b-4f59-aee8-10fde42cba90", "AQAAAAIAAYagAAAAEBxP6RwpPSBPoALVdILcrPVDWsosIgAYCuYcmi91GwZrs7/oSkEgYk1keijEf2quwQ==", "e171d5e0-7da6-4135-9f3f-d67466de1b1d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "cf833103-d733-4402-b00c-1263ca230e72",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c5c8475d-2e6a-4248-8bc5-617dcae97ab6", "AQAAAAIAAYagAAAAEPlaEdXC04hmFvYuj7Pbxyd+tODDDxjiyp+dROdHNe8D47tsVggXNzkcXNNmyyHb7g==", "20de64ff-b194-45f3-83e8-b3f284d94b3a" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteTracks_TrackId",
                table: "UserFavoriteTracks",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteTracks_UserId",
                table: "UserFavoriteTracks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteTracks_UserId_TrackId",
                table: "UserFavoriteTracks",
                columns: new[] { "UserId", "TrackId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavoriteTracks");

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
        }
    }
}
