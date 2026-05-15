using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farmelo.Data.Migrations
{
    /// <inheritdoc />
    public partial class GradePurchasesInvoicesReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SuttaGrade",
                table: "RawStockMovements",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Makhana 5 Sutta");

            migrationBuilder.AddColumn<int>(
                name: "SellerPartyId",
                table: "RawStockEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuttaGrade",
                table: "RawStockEntries",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Makhana 5 Sutta");

            migrationBuilder.AddColumn<string>(
                name: "SuttaGrade",
                table: "BoxStockMovements",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Makhana 5 Sutta");

            migrationBuilder.AddColumn<string>(
                name: "SuttaGrade",
                table: "B2BOrders",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Makhana 5 Sutta");

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    InvoiceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    B2BOrderId = table.Column<int>(type: "int", nullable: true),
                    B2CAssignmentId = table.Column<int>(type: "int", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    PdfFileName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    PdfPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmailError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmailSentOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_B2BOrders_B2BOrderId",
                        column: x => x.B2BOrderId,
                        principalTable: "B2BOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_B2CAssignments_B2CAssignmentId",
                        column: x => x.B2CAssignmentId,
                        principalTable: "B2CAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawStockEntries_SellerPartyId_EntryDate",
                table: "RawStockEntries",
                columns: new[] { "SellerPartyId", "EntryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RawStockEntries_SuttaGrade_AvailableKg_EntryDate",
                table: "RawStockEntries",
                columns: new[] { "SuttaGrade", "AvailableKg", "EntryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_B2BOrders_SuttaGrade_OrderDate",
                table: "B2BOrders",
                columns: new[] { "SuttaGrade", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_B2BOrderId",
                table: "Invoices",
                column: "B2BOrderId",
                unique: true,
                filter: "[B2BOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_B2CAssignmentId",
                table: "Invoices",
                column: "B2CAssignmentId",
                unique: true,
                filter: "[B2CAssignmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceType_InvoiceDate",
                table: "Invoices",
                columns: new[] { "InvoiceType", "InvoiceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PartyId_InvoiceDate",
                table: "Invoices",
                columns: new[] { "PartyId", "InvoiceDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_RawStockEntries_Parties_SellerPartyId",
                table: "RawStockEntries",
                column: "SellerPartyId",
                principalTable: "Parties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawStockEntries_Parties_SellerPartyId",
                table: "RawStockEntries");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_RawStockEntries_SellerPartyId_EntryDate",
                table: "RawStockEntries");

            migrationBuilder.DropIndex(
                name: "IX_RawStockEntries_SuttaGrade_AvailableKg_EntryDate",
                table: "RawStockEntries");

            migrationBuilder.DropIndex(
                name: "IX_B2BOrders_SuttaGrade_OrderDate",
                table: "B2BOrders");

            migrationBuilder.DropColumn(
                name: "SuttaGrade",
                table: "RawStockMovements");

            migrationBuilder.DropColumn(
                name: "SellerPartyId",
                table: "RawStockEntries");

            migrationBuilder.DropColumn(
                name: "SuttaGrade",
                table: "RawStockEntries");

            migrationBuilder.DropColumn(
                name: "SuttaGrade",
                table: "BoxStockMovements");

            migrationBuilder.DropColumn(
                name: "SuttaGrade",
                table: "B2BOrders");
        }
    }
}
