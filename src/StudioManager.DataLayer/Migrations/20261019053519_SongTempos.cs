using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class SongTempos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SongTempos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Bpm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    FromBar = table.Column<int>(type: "integer", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    SongId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongTempos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongTempos_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SongTempos_SongId",
                table: "SongTempos",
                column: "SongId");

            // Keep the existing BPM values: every song with a BPM gets it as its first tempo
            migrationBuilder.Sql(@"
                INSERT INTO ""SongTempos"" (""Bpm"", ""SortOrder"", ""SongId"", ""DateCreated"", ""DateUpdated"")
                SELECT ""Bpm"", 1, ""Id"", now(), now() FROM ""Songs"" WHERE ""Bpm"" IS NOT NULL;");

            migrationBuilder.DropColumn(
                name: "Bpm",
                table: "Songs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Bpm",
                table: "Songs",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            // Back to one BPM per song: the first tempo
            migrationBuilder.Sql(@"
                UPDATE ""Songs"" SET ""Bpm"" = (
                    SELECT t.""Bpm"" FROM ""SongTempos"" t WHERE t.""SongId"" = ""Songs"".""Id"" ORDER BY t.""SortOrder"" LIMIT 1);");

            migrationBuilder.DropTable(
                name: "SongTempos");
        }
    }
}
