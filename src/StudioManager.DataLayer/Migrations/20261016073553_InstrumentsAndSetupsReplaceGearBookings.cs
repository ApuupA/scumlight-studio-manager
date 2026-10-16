using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class InstrumentsAndSetupsReplaceGearBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GearBookings");

            migrationBuilder.CreateTable(
                name: "ProjectInstruments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsChain = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    BandId = table.Column<int>(type: "integer", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInstruments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectInstruments_Bands_BandId",
                        column: x => x.BandId,
                        principalTable: "Bands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProjectInstruments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetupTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsChain = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentSetupItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GearItemId = table.Column<int>(type: "integer", nullable: true),
                    ExternalGearId = table.Column<int>(type: "integer", nullable: true),
                    CustomName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UsedFor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    NeedsInput = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProjectInstrumentId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentSetupItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrumentSetupItems_ExternalGear_ExternalGearId",
                        column: x => x.ExternalGearId,
                        principalTable: "ExternalGear",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstrumentSetupItems_GearItems_GearItemId",
                        column: x => x.GearItemId,
                        principalTable: "GearItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstrumentSetupItems_ProjectInstruments_ProjectInstrumentId",
                        column: x => x.ProjectInstrumentId,
                        principalTable: "ProjectInstruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionInstruments",
                columns: table => new
                {
                    InstrumentsId = table.Column<int>(type: "integer", nullable: false),
                    SessionsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionInstruments", x => new { x.InstrumentsId, x.SessionsId });
                    table.ForeignKey(
                        name: "FK_SessionInstruments_ProjectInstruments_InstrumentsId",
                        column: x => x.InstrumentsId,
                        principalTable: "ProjectInstruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionInstruments_Sessions_SessionsId",
                        column: x => x.SessionsId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetupTemplateItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GearItemId = table.Column<int>(type: "integer", nullable: true),
                    CustomName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UsedFor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    NeedsInput = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SetupTemplateId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetupTemplateItems_GearItems_GearItemId",
                        column: x => x.GearItemId,
                        principalTable: "GearItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SetupTemplateItems_SetupTemplates_SetupTemplateId",
                        column: x => x.SetupTemplateId,
                        principalTable: "SetupTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentSetupItems_ExternalGearId",
                table: "InstrumentSetupItems",
                column: "ExternalGearId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentSetupItems_GearItemId",
                table: "InstrumentSetupItems",
                column: "GearItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentSetupItems_ProjectInstrumentId",
                table: "InstrumentSetupItems",
                column: "ProjectInstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInstruments_BandId",
                table: "ProjectInstruments",
                column: "BandId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInstruments_ProjectId",
                table: "ProjectInstruments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionInstruments_SessionsId",
                table: "SessionInstruments",
                column: "SessionsId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupTemplateItems_GearItemId",
                table: "SetupTemplateItems",
                column: "GearItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupTemplateItems_SetupTemplateId",
                table: "SetupTemplateItems",
                column: "SetupTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstrumentSetupItems");

            migrationBuilder.DropTable(
                name: "SessionInstruments");

            migrationBuilder.DropTable(
                name: "SetupTemplateItems");

            migrationBuilder.DropTable(
                name: "ProjectInstruments");

            migrationBuilder.DropTable(
                name: "SetupTemplates");

            migrationBuilder.CreateTable(
                name: "GearBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GearItemId = table.Column<int>(type: "integer", nullable: false),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GearBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GearBookings_GearItems_GearItemId",
                        column: x => x.GearItemId,
                        principalTable: "GearItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GearBookings_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GearBookings_GearItemId",
                table: "GearBookings",
                column: "GearItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GearBookings_SessionId",
                table: "GearBookings",
                column: "SessionId");
        }
    }
}
