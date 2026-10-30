using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanay3y.API.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletTopUps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalletTopUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "EGP"),
                    Provider = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SpecialReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProviderOrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProviderIntentionId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ProviderTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTopUps", x => x.Id);
                    table.CheckConstraint("CK_WalletTopUps_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_WalletTopUps_Status", "[Status] IN ('Pending', 'Paid', 'Failed', 'Cancelled', 'Expired')");
                    table.ForeignKey(
                        name: "FK_WalletTopUps_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTopUps_ProviderOrderId",
                table: "WalletTopUps",
                column: "ProviderOrderId",
                unique: true,
                filter: "[ProviderOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTopUps_ProviderTransactionId",
                table: "WalletTopUps",
                column: "ProviderTransactionId",
                unique: true,
                filter: "[ProviderTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTopUps_SpecialReference",
                table: "WalletTopUps",
                column: "SpecialReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTopUps_WalletId_CreatedAtUtc",
                table: "WalletTopUps",
                columns: new[] { "WalletId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletTopUps");
        }
    }
}
