using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class MixAndMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GapBeforeSeconds",
                table: "Songs",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isrc",
                table: "Songs",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialLink",
                table: "Songs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialNotes",
                table: "Songs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "MaterialReceivedOn",
                table: "Songs",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialTrackCount",
                table: "Songs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MeasuredLufs",
                table: "Songs",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MixNotes",
                table: "Songs",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TruePeak",
                table: "Songs",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraRevisionPrice",
                table: "Projects",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IncludedRevisions",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MixVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Link = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SongId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MixVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MixVersions_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDeliverables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsDone = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDeliverables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDeliverables_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MixFeedback",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timecode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsDone = table.Column<bool>(type: "boolean", nullable: false),
                    MixVersionId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MixFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MixFeedback_MixVersions_MixVersionId",
                        column: x => x.MixVersionId,
                        principalTable: "MixVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MixFeedback_MixVersionId",
                table: "MixFeedback",
                column: "MixVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_MixVersions_SongId_Number",
                table: "MixVersions",
                columns: new[] { "SongId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDeliverables_ProjectId",
                table: "ProjectDeliverables",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MixFeedback");

            migrationBuilder.DropTable(
                name: "ProjectDeliverables");

            migrationBuilder.DropTable(
                name: "MixVersions");

            migrationBuilder.DropColumn(
                name: "GapBeforeSeconds",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "Isrc",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "MaterialLink",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "MaterialNotes",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "MaterialReceivedOn",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "MaterialTrackCount",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "MeasuredLufs",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "MixNotes",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "TruePeak",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "ExtraRevisionPrice",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IncludedRevisions",
                table: "Projects");
        }
    }
}
