using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanay3y.API.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceRequestId = table.Column<int>(type: "int", nullable: false),
                    ArtisanUserId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstimatedArrivalMinutes = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOffers", x => x.Id);
                    table.CheckConstraint("CK_ServiceOffers_EstimatedArrival", "[EstimatedArrivalMinutes] IS NULL OR [EstimatedArrivalMinutes] > 0");
                    table.CheckConstraint("CK_ServiceOffers_Price", "[Price] > 0");
                    table.CheckConstraint("CK_ServiceOffers_Status", "[Status] IN ('Pending', 'Accepted', 'Rejected', 'Withdrawn')");
                    table.ForeignKey(
                        name: "FK_ServiceOffers_ServiceRequests_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceOffers_Users_ArtisanUserId",
                        column: x => x.ArtisanUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOffers_ArtisanUserId",
                table: "ServiceOffers",
                column: "ArtisanUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOffers_ServiceRequestId",
                table: "ServiceOffers",
                column: "ServiceRequestId",
                unique: true,
                filter: "[Status] = 'Accepted'");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOffers_ServiceRequestId_ArtisanUserId",
                table: "ServiceOffers",
                columns: new[] { "ServiceRequestId", "ArtisanUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOffers_ServiceRequestId_Status",
                table: "ServiceOffers",
                columns: new[] { "ServiceRequestId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceOffers");
        }
    }
}
