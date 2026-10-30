using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanay3y.API.Migrations
{
    /// <inheritdoc />
    public partial class AddServicePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServicePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceRequestId = table.Column<int>(type: "int", nullable: false),
                    CustomerUserId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "EGP"),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "Pending"),
                    ProviderOrderId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ProviderTransactionId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CheckoutUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomerCashConfirmedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArtisanCashConfirmedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePayments", x => x.Id);
                    table.CheckConstraint("CK_ServicePayments_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_ServicePayments_Status", "[Status] IN ('Pending', 'AwaitingCustomerAction', 'AwaitingCashConfirmation', 'Paid', 'Failed', 'Cancelled', 'Expired', 'Refunded')");
                    table.ForeignKey(
                        name: "FK_ServicePayments_ServiceRequests_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServicePayments_Users_CustomerUserId",
                        column: x => x.CustomerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_CustomerUserId_CreatedAtUtc",
                table: "ServicePayments",
                columns: new[] { "CustomerUserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_ProviderTransactionId",
                table: "ServicePayments",
                column: "ProviderTransactionId",
                unique: true,
                filter: "[ProviderTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePayments_ServiceRequestId",
                table: "ServicePayments",
                column: "ServiceRequestId",
                unique: true,
                filter: "[Status] IN ('Pending', 'AwaitingCustomerAction', 'AwaitingCashConfirmation')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicePayments");
        }
    }
}
