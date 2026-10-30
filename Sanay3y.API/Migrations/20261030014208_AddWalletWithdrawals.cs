using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanay3y.API.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletWithdrawals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WalletTransactions_Type",
                table: "WalletTransactions");

            migrationBuilder.AddColumn<int>(
                name: "WalletWithdrawalId",
                table: "WalletTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WalletWithdrawals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "EGP"),
                    PayoutMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PayoutProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountHolderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DestinationAccount = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    AdminNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransferReference = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletWithdrawals", x => x.Id);
                    table.CheckConstraint("CK_WalletWithdrawals_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_WalletWithdrawals_PayoutMethod", "[PayoutMethod] IN ('MobileWallet', 'BankTransfer')");
                    table.CheckConstraint("CK_WalletWithdrawals_Status", "[Status] IN ('Pending', 'Approved', 'Rejected', 'Paid', 'Failed', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_WalletWithdrawals_Users_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletWithdrawals_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletWithdrawalId",
                table: "WalletTransactions",
                column: "WalletWithdrawalId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WalletTransactions_Type",
                table: "WalletTransactions",
                sql: "[Type] IN ('TopUp', 'ServicePaymentDebit', 'ServicePaymentCredit', 'RefundCredit', 'WithdrawalDebit', 'WithdrawalRefund')");

            migrationBuilder.CreateIndex(
                name: "IX_WalletWithdrawals_ReviewedByAdminId",
                table: "WalletWithdrawals",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletWithdrawals_Status_RequestedAtUtc",
                table: "WalletWithdrawals",
                columns: new[] { "Status", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletWithdrawals_WalletId_RequestedAtUtc",
                table: "WalletWithdrawals",
                columns: new[] { "WalletId", "RequestedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_WalletWithdrawals_WalletWithdrawalId",
                table: "WalletTransactions",
                column: "WalletWithdrawalId",
                principalTable: "WalletWithdrawals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_WalletWithdrawals_WalletWithdrawalId",
                table: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "WalletWithdrawals");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_WalletWithdrawalId",
                table: "WalletTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WalletTransactions_Type",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "WalletWithdrawalId",
                table: "WalletTransactions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WalletTransactions_Type",
                table: "WalletTransactions",
                sql: "[Type] IN ('TopUp', 'ServicePaymentDebit', 'ServicePaymentCredit', 'RefundCredit')");
        }
    }
}
