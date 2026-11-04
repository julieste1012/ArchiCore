using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBoxWarehousePhysicalStorage : Migration
    {
        /// <inheritdoc />
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
                    WarehouseLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ParentLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseLocations", x => new { x.WarehouseId, x.Id });
                    table.ForeignKey(
                        name: "FK_WarehouseLocations_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoxFolderReferences",
                columns: table => new
                {
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BoxId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoxFolderReferences", x => new { x.BoxId, x.FolderId });
                    table.ForeignKey(
                        name: "FK_BoxFolderReferences_Boxes_BoxId",
                        column: x => x.BoxId,
                        principalTable: "Boxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseLocationBoxReferences",
                columns: table => new
                {
                    BoxId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseLocationBoxReferences", x => new { x.WarehouseId, x.LocationId, x.BoxId });
                    table.ForeignKey(
                        name: "FK_WarehouseLocationBoxReferences_WarehouseLocations_Warehouse~",
                        columns: x => new { x.WarehouseId, x.LocationId },
                        principalTable: "WarehouseLocations",
                        principalColumns: new[] { "WarehouseId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Boxes_Barcode",
                table: "Boxes",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boxes_BoxCode",
                table: "Boxes",
                column: "BoxCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoxFolderReferences_FolderId",
                table: "BoxFolderReferences",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseLocationBoxReferences_BoxId",
                table: "WarehouseLocationBoxReferences",
                column: "BoxId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseLocations_WarehouseId_Code",
                table: "WarehouseLocations",
                columns: new[] { "WarehouseId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoxFolderReferences");

            migrationBuilder.DropTable(
                name: "WarehouseLocationBoxReferences");

            migrationBuilder.DropTable(
                name: "Boxes");

            migrationBuilder.DropTable(
                name: "WarehouseLocations");
        }
    }
}
