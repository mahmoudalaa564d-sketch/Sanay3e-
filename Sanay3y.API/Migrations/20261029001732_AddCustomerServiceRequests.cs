using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanay3y.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerServiceRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerUserId = table.Column<int>(type: "int", nullable: false),
                    SpecialtyId = table.Column<int>(type: "int", nullable: false),
                    ServiceTypeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProblemDescription = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsAsSoonAsPossible = table.Column<bool>(type: "bit", nullable: false),
                    ScheduledAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AddressTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Governorate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BuildingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FloorNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApartmentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Landmark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "PendingOffers"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.Id);
                    table.CheckConstraint("CK_ServiceRequests_LatitudeRange", "[Latitude] IS NULL OR ([Latitude] >= -90 AND [Latitude] <= 90)");
                    table.CheckConstraint("CK_ServiceRequests_LongitudeRange", "[Longitude] IS NULL OR ([Longitude] >= -180 AND [Longitude] <= 180)");
                    table.CheckConstraint("CK_ServiceRequests_ScheduleChoice", "([IsAsSoonAsPossible] = 1 AND [ScheduledAtUtc] IS NULL) OR ([IsAsSoonAsPossible] = 0 AND [ScheduledAtUtc] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_CustomerUserId",
                        column: x => x.CustomerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequestAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceRequestId = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestAttachments", x => x.Id);
                    table.CheckConstraint("CK_ServiceRequestAttachments_SizeBytes", "[SizeBytes] >= 0");
                    table.ForeignKey(
                        name: "FK_ServiceRequestAttachments_ServiceRequests_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestAttachments_ServiceRequestId",
                table: "ServiceRequestAttachments",
                column: "ServiceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CustomerUserId_CreatedAtUtc",
                table: "ServiceRequests",
                columns: new[] { "CustomerUserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequestNumber",
                table: "ServiceRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_SpecialtyId_Status",
                table: "ServiceRequests",
                columns: new[] { "SpecialtyId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRequestAttachments");

            migrationBuilder.DropTable(
                name: "ServiceRequests");
        }
    }
}
