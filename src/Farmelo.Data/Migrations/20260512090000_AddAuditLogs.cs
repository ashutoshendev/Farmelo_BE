using Farmelo.Data.Write.EFContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Farmelo.Data.Migrations;

[DbContext(typeof(FarmeloDbContext))]
[Migration("20260512090000_AddAuditLogs")]
public partial class AddAuditLogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: true),
                UserFullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                UserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                UserRole = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                EventType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                Module = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Action = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                HttpMethod = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                StatusCode = table.Column<int>(type: "int", nullable: true),
                DurationMs = table.Column<long>(type: "bigint", nullable: true),
                IpAddress = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                OccurredOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_EventType_OccurredOn",
            table: "AuditLogs",
            columns: new[] { "EventType", "OccurredOn" });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_Module_OccurredOn",
            table: "AuditLogs",
            columns: new[] { "Module", "OccurredOn" });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_OccurredOn",
            table: "AuditLogs",
            column: "OccurredOn");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_UserId_OccurredOn",
            table: "AuditLogs",
            columns: new[] { "UserId", "OccurredOn" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AuditLogs");
    }
}
