using Farmelo.Data.Write.EFContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farmelo.Data.Migrations;

[DbContext(typeof(FarmeloDbContext))]
[Migration("20260512083000_AddPerformanceIndexes")]
public partial class AddPerformanceIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Products_IsActive_IsBestseller_Name",
            table: "Products",
            columns: new[] { "IsActive", "IsBestseller", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_ProductPriceHistories_ProductId_ChangedOn",
            table: "ProductPriceHistories",
            columns: new[] { "ProductId", "ChangedOn" });

        migrationBuilder.CreateIndex(
            name: "IX_UserAccounts_Role_IsActive_CreatedOn",
            table: "UserAccounts",
            columns: new[] { "Role", "IsActive", "CreatedOn" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Products_IsActive_IsBestseller_Name",
            table: "Products");

        migrationBuilder.DropIndex(
            name: "IX_ProductPriceHistories_ProductId_ChangedOn",
            table: "ProductPriceHistories");

        migrationBuilder.DropIndex(
            name: "IX_UserAccounts_Role_IsActive_CreatedOn",
            table: "UserAccounts");
    }
}
