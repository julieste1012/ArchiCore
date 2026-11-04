using ArchiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiCore.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ArchiCoreDbContext))]
[Migration("20261030040000_AddDocumentaryReceptions")]
public partial class AddDocumentaryReceptions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DocumentaryReceptions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                ReceivedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                InitialObservations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                InspectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                InspectionCompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                SafeToAccept = table.Column<bool>(type: "boolean", nullable: true),
                InspectionObservations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                DeclaredBoxCount = table.Column<int>(type: "integer", nullable: true),
                DeclaredFolderCount = table.Column<int>(type: "integer", nullable: true),
                CountedBoxCount = table.Column<int>(type: "integer", nullable: true),
                CountedFolderCount = table.Column<int>(type: "integer", nullable: true),
                DiscrepancyReport = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                CountedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                CountedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                ApprovedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                SuspendedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                SuspendedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                SuspensionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DocumentaryReceptions", reception => reception.Id);
                table.ForeignKey(
                    name: "FK_DocumentaryReceptions_Contracts_ContractId",
                    column: reception => reception.ContractId,
                    principalTable: "Contracts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DocumentaryReceptions_Customers_CustomerId",
                    column: reception => reception.CustomerId,
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DocumentaryReceptions_ContractId_ReceivedAtUtc",
            table: "DocumentaryReceptions",
            columns: new[] { "ContractId", "ReceivedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_DocumentaryReceptions_Status",
            table: "DocumentaryReceptions",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_DocumentaryReceptions_ReceivedAtUtc",
            table: "DocumentaryReceptions",
            column: "ReceivedAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_DocumentaryReceptions_CustomerId",
            table: "DocumentaryReceptions",
            column: "CustomerId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DocumentaryReceptions");
    }
}
