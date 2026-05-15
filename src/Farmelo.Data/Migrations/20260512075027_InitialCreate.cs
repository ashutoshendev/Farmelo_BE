using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Farmelo.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    Weight = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "INR"),
                    Ingredients = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Nutrition = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsBestseller = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccentColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BackgroundColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductPriceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OldPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    NewPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    ChangedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPriceHistories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPriceHistories_UserAccounts_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AccentColor", "BackgroundColor", "CreatedBy", "CreatedOn", "Currency", "CurrentPrice", "Description", "ImageUrl", "Ingredients", "IsActive", "IsBestseller", "ModifiedBy", "ModifiedOn", "Name", "Nutrition", "Slug", "Weight" },
                values: new object[,]
                {
                    { 1, "#0d6042", "#eef6e4", "Admin", new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Utc), "INR", 249m, "Clean roasted crunch for daily snacking.", null, "Roasted makhana, olive oil, rock salt.", true, true, null, null, "Plain Makhana", "High protein, gluten-free, light roasted snack.", "plain", "200g" },
                    { 2, "#b83224", "#fff3ec", "Admin", new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Utc), "INR", 269m, "Fiery chilli, garlic and tang.", null, "Roasted makhana, olive oil, peri peri seasoning.", true, true, null, null, "Peri Peri", "Roasted not fried, no palm oil, no added preservatives.", "peri-peri", "200g" },
                    { 3, "#25231d", "#f4ede0", "Admin", new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Utc), "INR", 269m, "Classic seasoning with a sharp pepper finish.", null, "Roasted makhana, olive oil, salt and pepper seasoning.", true, false, null, null, "Salt and Pepper", "Roasted not fried, no palm oil, no added preservatives.", "salt-and-pepper", "200g" },
                    { 4, "#326f2b", "#edf6dc", "Admin", new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Utc), "INR", 269m, "Smooth onion with herby creaminess.", null, "Roasted makhana, olive oil, cream and onion seasoning.", true, true, null, null, "Cream and Onion", "Roasted not fried, no palm oil, no added preservatives.", "cream-and-onion", "200g" },
                    { 5, "#d99622", "#fff5d7", "Admin", new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Utc), "INR", 269m, "Creamy, savoury and snackable.", null, "Roasted makhana, olive oil, cheesy delight seasoning.", true, true, null, null, "Cheesy Delight", "Roasted not fried, no palm oil, no added preservatives.", "cheesy-delight", "200g" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceHistories_ChangedByUserId",
                table: "ProductPriceHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceHistories_ProductId",
                table: "ProductPriceHistories",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Email",
                table: "UserAccounts",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPriceHistories");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "UserAccounts");
        }
    }
}
