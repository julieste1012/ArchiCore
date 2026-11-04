using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchiCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentaryAggregates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Archives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DigitizationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitizationRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ArchiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpedientId = table.Column<Guid>(type: "uuid", nullable: false),
                    BoxId = table.Column<Guid>(type: "uuid", nullable: true),
                    LoanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AuthorizedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorizedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeliveredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeliveredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReturnedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReturnedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OverdueDetectedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveSections", x => new { x.ArchiveId, x.Id });
                    table.ForeignKey(
                        name: "FK_ArchiveSections_Archives_ArchiveId",
                        column: x => x.ArchiveId,
                        principalTable: "Archives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DigitizedFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DigitizationRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageObjectKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PageCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitizedFiles", x => new { x.DigitizationRequestId, x.Id });
                    table.ForeignKey(
                        name: "FK_DigitizedFiles_DigitizationRequests_DigitizationRequestId",
                        column: x => x.DigitizationRequestId,
                        principalTable: "DigitizationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FolderDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderDocuments", x => new { x.FolderId, x.Id });
                    table.ForeignKey(
                        name: "FK_FolderDocuments_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveSeries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveSeries", x => new { x.ArchiveId, x.SectionId, x.Id });
                    table.ForeignKey(
                        name: "FK_ArchiveSeries_ArchiveSections_ArchiveId_SectionId",
                        columns: x => new { x.ArchiveId, x.SectionId },
                        principalTable: "ArchiveSections",
                        principalColumns: new[] { "ArchiveId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentAnnexes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAnnexes", x => new { x.FolderId, x.DocumentId, x.Id });
                    table.ForeignKey(
                        name: "FK_DocumentAnnexes_FolderDocuments_FolderId_DocumentId",
                        columns: x => new { x.FolderId, x.DocumentId },
                        principalTable: "FolderDocuments",
                        principalColumns: new[] { "FolderId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentFolios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentFolios", x => new { x.FolderId, x.DocumentId, x.Id });
                    table.ForeignKey(
                        name: "FK_DocumentFolios_FolderDocuments_FolderId_DocumentId",
                        columns: x => new { x.FolderId, x.DocumentId },
                        principalTable: "FolderDocuments",
                        principalColumns: new[] { "FolderId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveSubseries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveSubseries", x => new { x.ArchiveId, x.SectionId, x.SeriesId, x.Id });
                    table.ForeignKey(
                        name: "FK_ArchiveSubseries_ArchiveSeries_ArchiveId_SectionId_SeriesId",
                        columns: x => new { x.ArchiveId, x.SectionId, x.SeriesId },
                        principalTable: "ArchiveSeries",
                        principalColumns: new[] { "ArchiveId", "SectionId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveExpedients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubseriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveExpedients", x => new { x.ArchiveId, x.SectionId, x.SeriesId, x.SubseriesId, x.Id });
                    table.ForeignKey(
                        name: "FK_ArchiveExpedients_ArchiveSubseries_ArchiveId_SectionId_Seri~",
                        columns: x => new { x.ArchiveId, x.SectionId, x.SeriesId, x.SubseriesId },
                        principalTable: "ArchiveSubseries",
                        principalColumns: new[] { "ArchiveId", "SectionId", "SeriesId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveExpedients_ArchiveId_SectionId_SeriesId_SubseriesId_~",
                table: "ArchiveExpedients",
                columns: new[] { "ArchiveId", "SectionId", "SeriesId", "SubseriesId", "Reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Archives_ProjectId",
                table: "Archives",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveSections_ArchiveId_Code",
                table: "ArchiveSections",
                columns: new[] { "ArchiveId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveSeries_ArchiveId_SectionId_Code",
                table: "ArchiveSeries",
                columns: new[] { "ArchiveId", "SectionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveSubseries_ArchiveId_SectionId_SeriesId_Code",
                table: "ArchiveSubseries",
                columns: new[] { "ArchiveId", "SectionId", "SeriesId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DigitizationRequests_FolderId",
                table: "DigitizationRequests",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitizationRequests_ProjectId",
                table: "DigitizationRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitizationRequests_Status",
                table: "DigitizationRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DigitizedFiles_DigitizationRequestId_StorageObjectKey",
                table: "DigitizedFiles",
                columns: new[] { "DigitizationRequestId", "StorageObjectKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnnexes_FolderId_DocumentId_Sequence",
                table: "DocumentAnnexes",
                columns: new[] { "FolderId", "DocumentId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFolios_FolderId_DocumentId_Number",
                table: "DocumentFolios",
                columns: new[] { "FolderId", "DocumentId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ArchiveId",
                table: "Folders",
                column: "ArchiveId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_BoxId",
                table: "Folders",
                column: "BoxId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ExpedientId",
                table: "Folders",
                column: "ExpedientId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_FolderCode",
                table: "Folders",
                column: "FolderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_LoanId",
                table: "Folders",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_FolderId",
                table: "Loans",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_ProjectId",
                table: "Loans",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_Status_DueAtUtc",
                table: "Loans",
                columns: new[] { "Status", "DueAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiveExpedients");

            migrationBuilder.DropTable(
                name: "DigitizedFiles");

            migrationBuilder.DropTable(
                name: "DocumentAnnexes");

            migrationBuilder.DropTable(
                name: "DocumentFolios");

            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropTable(
                name: "ArchiveSubseries");

            migrationBuilder.DropTable(
                name: "DigitizationRequests");

            migrationBuilder.DropTable(
                name: "FolderDocuments");

            migrationBuilder.DropTable(
                name: "ArchiveSeries");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "ArchiveSections");

            migrationBuilder.DropTable(
                name: "Archives");
        }
    }
}
