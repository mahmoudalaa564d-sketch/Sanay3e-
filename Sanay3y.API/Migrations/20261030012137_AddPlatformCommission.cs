
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sanay3y.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformCommission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ArtisanNetAmount",
                table: "ServicePayments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionAmount",
                table: "ServicePayments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate",
                table: "ServicePayments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Preserve historical paid payments.
            // Before platform commission was introduced,
            // the artisan received the full service amount.
            migrationBuilder.Sql(
                "UPDATE [ServicePayments] " +
                "SET [ArtisanNetAmount] = CASE " +
                "WHEN [Status] = N'Paid' THEN [Amount] " +
                "ELSE 0 END;");

            migrationBuilder.CreateTable(
                name: "PlatformCommissions",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    ServicePaymentId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    ArtisanUserId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Amount = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),

                    Status = table.Column<string>(
                        type: "nvarchar(20)",
                        maxLength: 20,
                        nullable: false,
                        defaultValue: "Collected"),

                    CreatedAtUtc = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false,
                        defaultValueSql: "SYSUTCDATETIME()"),

                    CollectedAtUtc = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PlatformCommissions",
                        x => x.Id);

                    table.CheckConstraint(
                        "CK_PlatformCommissions_Amount",
                        "[Amount] > 0");

                    table.CheckConstraint(
                        "CK_PlatformCommissions_Status",
                        "[Status] IN ('Collected', 'Due', 'Reversed')");

                    table.ForeignKey(
                        name: "FK_PlatformCommissions_ServicePayments_ServicePaymentId",
                        column: x => x.ServicePaymentId,
                        principalTable: "ServicePayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_PlatformCommissions_Users_ArtisanUserId",
                        column: x => x.ArtisanUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServicePayments_ArtisanNetAmount",
                table: "ServicePayments",
                sql: "[ArtisanNetAmount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServicePayments_CommissionAmount",
                table: "ServicePayments",
                sql: "[CommissionAmount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ServicePayments_CommissionRate",
                table: "ServicePayments",
                sql: "[CommissionRate] >= 0 AND [CommissionRate] <= 100");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformCommissions_ArtisanUserId_Status_CreatedAtUtc",
                table: "PlatformCommissions",
                columns: new[]
                {
                    "ArtisanUserId",
                    "Status",
                    "CreatedAtUtc"
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformCommissions_ServicePaymentId",
                table: "PlatformCommissions",
                column: "ServicePaymentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformCommissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ServicePayments_ArtisanNetAmount",
                table: "ServicePayments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ServicePayments_CommissionAmount",
                table: "ServicePayments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ServicePayments_CommissionRate",
                table: "ServicePayments");

            migrationBuilder.DropColumn(
                name: "ArtisanNetAmount",
                table: "ServicePayments");

            migrationBuilder.DropColumn(
                name: "CommissionAmount",
                table: "ServicePayments");

            migrationBuilder.DropColumn(
                name: "CommissionRate",
                table: "ServicePayments");
        }
    }
}
