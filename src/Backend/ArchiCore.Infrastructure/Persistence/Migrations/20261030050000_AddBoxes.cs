using ArchiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiCore.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ArchiCoreDbContext))]
[Migration("20261030050000_AddBoxes")]
public partial class AddBoxes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Boxes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                BoxCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Boxes", box => box.Id);
            });

        migrationBuilder.CreateIndex(
            name: "UX_Boxes_BoxCode",
            table: "Boxes",
            column: "BoxCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_Boxes_Barcode",
            table: "Boxes",
            column: "Barcode",
            unique: true,
            filter: "\"Barcode\" IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Boxes");
    }
}
