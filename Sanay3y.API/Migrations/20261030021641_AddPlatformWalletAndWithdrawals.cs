
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanay3y.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformWalletAndWithdrawals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformWallets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "EGP"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformWallets", x => x.Id);
                    table.CheckConstraint(
                        "CK_PlatformWallets_Balance",
                        "[Balance] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "PlatformWithdrawals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlatformWalletId = table.Column<int>(type: "int", nullable: false),
                    RequestedByAdminId = table.Column<int>(type: "int", nullable: false),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
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
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformWithdrawals", x => x.Id);

                    table.CheckConstraint(
                        "CK_PlatformWithdrawals_Amount",
                        "[Amount] > 0");

                    table.CheckConstraint(
                        "CK_PlatformWithdrawals_PayoutMethod",
                        "[PayoutMethod] IN ('MobileWallet', 'BankTransfer')");

                    table.CheckConstraint(
                        "CK_PlatformWithdrawals_Status",
                        "[Status] IN ('Pending', 'Approved', 'Rejected', 'Paid', 'Failed')");

                    table.ForeignKey(
                        name: "FK_PlatformWithdrawals_PlatformWallets_PlatformWalletId",
                        column: x => x.PlatformWalletId,
                        principalTable: "PlatformWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_PlatformWithdrawals_Users_RequestedByAdminId",
                        column: x => x.RequestedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_PlatformWithdrawals_Users_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlatformWalletTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    PlatformWalletId = table.Column<int>(type: "int", nullable: false),

                    PlatformCommissionId = table.Column<int>(type: "int", nullable: true),

                    PlatformWithdrawalId = table.Column<int>(type: "int", nullable: true),

                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),

                    Direction = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),

                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),

                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),

                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Completed"),

                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),

                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformWalletTransactions", x => x.Id);

                    table.CheckConstraint(
                        "CK_PlatformWalletTransactions_Amount",
                        "[Amount] > 0");

                    table.CheckConstraint(
                        "CK_PlatformWalletTransactions_BalanceAfter",
                        "[BalanceAfter] >= 0");

                    table.CheckConstraint(
                        "CK_PlatformWalletTransactions_Direction",
                        "[Direction] IN ('Credit', 'Debit')");

                    table.CheckConstraint(
                        "CK_PlatformWalletTransactions_Status",
                        "[Status] IN ('Pending', 'Completed', 'Failed', 'Cancelled')");

                    table.CheckConstraint(
                        "CK_PlatformWalletTransactions_Type",
                        "[Type] IN ('CommissionCredit', 'WithdrawalDebit', 'WithdrawalRefund')");

                    table.ForeignKey(
                        name: "FK_PlatformWalletTransactions_PlatformCommissions_PlatformCommissionId",
                        column: x => x.PlatformCommissionId,
                        principalTable: "PlatformCommissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_PlatformWalletTransactions_PlatformWallets_PlatformWalletId",
                        column: x => x.PlatformWalletId,
                        principalTable: "PlatformWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_PlatformWalletTransactions_PlatformWithdrawals_PlatformWithdrawalId",
                        column: x => x.PlatformWithdrawalId,
                        principalTable: "PlatformWithdrawals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWallets_AccountCode",
                table: "PlatformWallets",
                column: "AccountCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWalletTransactions_PlatformCommissionId",
                table: "PlatformWalletTransactions",
                column: "PlatformCommissionId",
                unique: true,
                filter: "[PlatformCommissionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWalletTransactions_PlatformWalletId_CreatedAtUtc",
                table: "PlatformWalletTransactions",
                columns: new[]
                {
                    "PlatformWalletId",
                    "CreatedAtUtc"
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWalletTransactions_PlatformWithdrawalId",
                table: "PlatformWalletTransactions",
                column: "PlatformWithdrawalId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWithdrawals_PlatformWalletId_RequestedAtUtc",
                table: "PlatformWithdrawals",
                columns: new[]
                {
                    "PlatformWalletId",
                    "RequestedAtUtc"
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWithdrawals_RequestedByAdminId",
                table: "PlatformWithdrawals",
                column: "RequestedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWithdrawals_ReviewedByAdminId",
                table: "PlatformWithdrawals",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWithdrawals_Status_RequestedAtUtc",
                table: "PlatformWithdrawals",
                columns: new[]
                {
                    "Status",
                    "RequestedAtUtc"
                });

            // Create the main platform wallet and migrate
            // all previously collected commissions into it.
            migrationBuilder.Sql(@"
                DECLARE @OpeningBalance DECIMAL(18,2);
                DECLARE @PlatformWalletId INT;

                SELECT @OpeningBalance =
                    COALESCE(SUM([Amount]), CONVERT(DECIMAL(18,2), 0))
                FROM [PlatformCommissions]
                WHERE [Status] = N'Collected';

                INSERT INTO [PlatformWallets]
                (
                    [AccountCode],
                    [Balance],
                    [Currency],
                    [CreatedAtUtc],
                    [UpdatedAtUtc]
                )
                VALUES
                (
                    N'MAIN',
                    @OpeningBalance,
                    N'EGP',
                    SYSUTCDATETIME(),
                    SYSUTCDATETIME()
                );

                SET @PlatformWalletId = CONVERT(INT, SCOPE_IDENTITY());

                INSERT INTO [PlatformWalletTransactions]
                (
                    [PlatformWalletId],
                    [PlatformCommissionId],
                    [PlatformWithdrawalId],
                    [Type],
                    [Direction],
                    [Amount],
                    [BalanceAfter],
                    [Status],
                    [Description],
                    [CreatedAtUtc]
                )
                SELECT
                    @PlatformWalletId,
                    pc.[Id],
                    NULL,
                    N'CommissionCredit',
                    N'Credit',
                    pc.[Amount],
                    SUM(pc.[Amount]) OVER
                    (
                        ORDER BY pc.[CreatedAtUtc], pc.[Id]
                        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
                    ),
                    N'Completed',
                    N'إضافة عمولة محصلة سابقة إلى رصيد المنصة',
                    pc.[CreatedAtUtc]
                FROM [PlatformCommissions] AS pc
                WHERE pc.[Status] = N'Collected';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformWalletTransactions");

            migrationBuilder.DropTable(
                name: "PlatformWithdrawals");

            migrationBuilder.DropTable(
                name: "PlatformWallets");
        }
    }
}
