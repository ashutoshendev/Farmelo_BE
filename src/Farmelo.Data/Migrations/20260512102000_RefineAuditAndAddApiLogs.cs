using Farmelo.Data.Write.EFContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Farmelo.Data.Migrations;

[DbContext(typeof(FarmeloDbContext))]
[Migration("20260512102000_RefineAuditAndAddApiLogs")]
public partial class RefineAuditAndAddApiLogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "NewValue",
            table: "AuditLogs",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "OldValue",
            table: "AuditLogs",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "TargetId",
            table: "AuditLogs",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TargetLabel",
            table: "AuditLogs",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ApiLogs",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Method = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                Endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                StatusCode = table.Column<int>(type: "int", nullable: false),
                ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false),
                ActorId = table.Column<int>(type: "int", nullable: true),
                Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApiLogs", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ApiLogs_ActorId_Timestamp",
            table: "ApiLogs",
            columns: new[] { "ActorId", "Timestamp" });

        migrationBuilder.CreateIndex(
            name: "IX_ApiLogs_Timestamp",
            table: "ApiLogs",
            column: "Timestamp");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_TargetLabel_OccurredOn",
            table: "AuditLogs",
            columns: new[] { "TargetLabel", "OccurredOn" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ApiLogs");

        migrationBuilder.DropIndex(
            name: "IX_AuditLogs_TargetLabel_OccurredOn",
            table: "AuditLogs");

        migrationBuilder.DropColumn(
            name: "NewValue",
            table: "AuditLogs");

        migrationBuilder.DropColumn(
            name: "OldValue",
            table: "AuditLogs");

        migrationBuilder.DropColumn(
            name: "TargetId",
            table: "AuditLogs");

        migrationBuilder.DropColumn(
            name: "TargetLabel",
            table: "AuditLogs");
    }
}
