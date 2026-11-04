using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiCore.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddDomainEventAuditEntries : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DomainEventAuditEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EventType = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                RecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Payload = table.Column<string>(type: "jsonb", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DomainEventAuditEntries", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DomainEventAuditEntries_OccurredOnUtc",
            table: "DomainEventAuditEntries",
            column: "OccurredOnUtc");

        migrationBuilder.CreateIndex(
            name: "IX_DomainEventAuditEntries_EventType",
            table: "DomainEventAuditEntries",
            column: "EventType");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DomainEventAuditEntries");
    }
}
