using ArchiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiCore.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ArchiCoreDbContext))]
[Migration("20261030030000_AddSecurityAuditEntries")]
public partial class AddSecurityAuditEntries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SecurityAuditEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                ActorRole = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                TargetUserId = table.Column<Guid>(type: "uuid", nullable: true),
                TargetRole = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SecurityAuditEntries", entry => entry.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SecurityAuditEntries_OccurredOnUtc",
            table: "SecurityAuditEntries",
            column: "OccurredOnUtc");

        migrationBuilder.CreateIndex(
            name: "IX_SecurityAuditEntries_Action",
            table: "SecurityAuditEntries",
            column: "Action");

        migrationBuilder.CreateIndex(
            name: "IX_SecurityAuditEntries_ActorUserId",
            table: "SecurityAuditEntries",
            column: "ActorUserId");

        migrationBuilder.CreateIndex(
            name: "IX_SecurityAuditEntries_TargetUserId",
            table: "SecurityAuditEntries",
            column: "TargetUserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SecurityAuditEntries");
    }
}
