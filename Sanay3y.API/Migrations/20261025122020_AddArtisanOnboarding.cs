using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanay3y.API.Migrations
{
    /// <inheritdoc />
    public partial class AddArtisanOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "ArtisanProfiles",
                type: "nvarchar(1500)",
                maxLength: 1500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Center",
                table: "ArtisanProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Governorate",
                table: "ArtisanProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdBackPath",
                table: "ArtisanProfiles",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdFrontPath",
                table: "ArtisanProfiles",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePath",
                table: "ArtisanProfiles",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TermsAccepted",
                table: "ArtisanProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TermsAcceptedAtUtc",
                table: "ArtisanProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsVersion",
                table: "ArtisanProfiles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Village",
                table: "ArtisanProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArtisanServiceAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtisanUserId = table.Column<int>(type: "int", nullable: false),
                    Governorate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Center = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Village = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtisanServiceAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtisanServiceAreas_ArtisanProfiles_ArtisanUserId",
                        column: x => x.ArtisanUserId,
                        principalTable: "ArtisanProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtisanServiceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtisanUserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtisanServiceTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtisanServiceTypes_ArtisanProfiles_ArtisanUserId",
                        column: x => x.ArtisanUserId,
                        principalTable: "ArtisanProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtisanSpecialties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtisanUserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtisanSpecialties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtisanSpecialties_ArtisanProfiles_ArtisanUserId",
                        column: x => x.ArtisanUserId,
                        principalTable: "ArtisanProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanServiceAreas_ArtisanUserId_Governorate_Center_Village",
                table: "ArtisanServiceAreas",
                columns: new[] { "ArtisanUserId", "Governorate", "Center", "Village" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanServiceTypes_ArtisanUserId_Name",
                table: "ArtisanServiceTypes",
                columns: new[] { "ArtisanUserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSpecialties_ArtisanUserId_Name",
                table: "ArtisanSpecialties",
                columns: new[] { "ArtisanUserId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtisanServiceAreas");

            migrationBuilder.DropTable(
                name: "ArtisanServiceTypes");

            migrationBuilder.DropTable(
                name: "ArtisanSpecialties");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "ArtisanProfiles");

            migrationBuilder.DropColumn(
                name: "Center",
                table: "ArtisanProfiles");

            migrationBuilder.DropColumn(
                name: "Governorate",
                table: "ArtisanProfiles");

            migrationBuilder.DropColumn(
                name: "NationalIdBackPath",
                table: "ArtisanProfiles");

            migrationBuilder.DropColumn(
                name: "NationalIdFrontPath",
                table: "ArtisanProfiles");

            migrationBuilder.DropColumn(
                name: "ProfileImagePath",
                table: "ArtisanProfiles");

            migrationBuilder.DropColumn(
                name: "TermsAccepted",
                table: "ArtisanProfiles");

            migrationBuilder.DropColumn(
                name: "TermsAcceptedAtUtc",
                table: "ArtisanProfiles");

            migrationBuilder.DropColumn(
                name: "TermsVersion",
                table: "ArtisanProfiles");

            migrationBuilder.DropColumn(
                name: "Village",
                table: "ArtisanProfiles");
        }
    }
}
