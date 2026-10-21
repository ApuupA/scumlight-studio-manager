using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class MixVersionsPerProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-written order (EF wanted to rename SongId to ProjectId, which would keep the song ids as project ids,
            // and to drop the song columns before copying). Every step keeps the data of the first mix version table.

            migrationBuilder.DropForeignKey(
                name: "FK_MixVersions_Songs_SongId",
                table: "MixVersions");

            migrationBuilder.DropIndex(
                name: "IX_MixVersions_SongId_Number",
                table: "MixVersions");

            // ---------- 1. Material moves from the songs to the project ----------
            migrationBuilder.AddColumn<DateOnly>(name: "MaterialReceivedOn", table: "Projects", type: "date", nullable: true);
            migrationBuilder.AddColumn<string>(name: "MaterialLink", table: "Projects", type: "character varying(2000)", maxLength: 2000, nullable: true);
            migrationBuilder.AddColumn<int>(name: "MaterialTrackCount", table: "Projects", type: "integer", nullable: true);
            migrationBuilder.AddColumn<string>(name: "MaterialNotes", table: "Projects", type: "character varying(2000)", maxLength: 2000, nullable: true);

            // Take the values of the first song that has them (they were only filled in for tests so far)
            migrationBuilder.Sql("""
                UPDATE "Projects" p SET
                    "MaterialReceivedOn" = (SELECT s."MaterialReceivedOn" FROM "Songs" s WHERE s."ProjectId" = p."Id" AND s."MaterialReceivedOn" IS NOT NULL ORDER BY s."TrackNumber" LIMIT 1),
                    "MaterialLink" = (SELECT s."MaterialLink" FROM "Songs" s WHERE s."ProjectId" = p."Id" AND s."MaterialLink" IS NOT NULL ORDER BY s."TrackNumber" LIMIT 1),
                    "MaterialTrackCount" = (SELECT s."MaterialTrackCount" FROM "Songs" s WHERE s."ProjectId" = p."Id" AND s."MaterialTrackCount" IS NOT NULL ORDER BY s."TrackNumber" LIMIT 1);
                """);

            migrationBuilder.DropColumn(name: "MaterialLink", table: "Songs");
            migrationBuilder.DropColumn(name: "MaterialReceivedOn", table: "Songs");
            migrationBuilder.DropColumn(name: "MaterialTrackCount", table: "Songs");

            // ---------- 2. Feedback gets its song (the song of its version so far) ----------
            migrationBuilder.AddColumn<int>(name: "SongId", table: "MixFeedback", type: "integer", nullable: true);
            migrationBuilder.Sql("""
                UPDATE "MixFeedback" f SET "SongId" = v."SongId" FROM "MixVersions" v WHERE v."Id" = f."MixVersionId";
                """);

            // ---------- 3. Versions get a list of songs (so far exactly one) ----------
            migrationBuilder.CreateTable(
                name: "MixVersionSongs",
                columns: table => new
                {
                    MixVersionsId = table.Column<int>(type: "integer", nullable: false),
                    SongsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MixVersionSongs", x => new { x.MixVersionsId, x.SongsId });
                    table.ForeignKey(
                        name: "FK_MixVersionSongs_MixVersions_MixVersionsId",
                        column: x => x.MixVersionsId,
                        principalTable: "MixVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MixVersionSongs_Songs_SongsId",
                        column: x => x.SongsId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.Sql("""
                INSERT INTO "MixVersionSongs" ("MixVersionsId", "SongsId") SELECT "Id", "SongId" FROM "MixVersions";
                """);

            // ---------- 4. Versions belong to the project: name from the old number, then renumber per project ----------
            migrationBuilder.AddColumn<string>(name: "Name", table: "MixVersions", type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<int>(name: "ProjectId", table: "MixVersions", type: "integer", nullable: false, defaultValue: 0);
            migrationBuilder.Sql("""
                UPDATE "MixVersions" v SET "Name" = 'v' || v."Number", "ProjectId" = s."ProjectId" FROM "Songs" s WHERE s."Id" = v."SongId";
                UPDATE "MixVersions" v SET "Number" = n.rn
                    FROM (SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "ProjectId" ORDER BY "Date", "Id") AS rn FROM "MixVersions") n
                    WHERE n."Id" = v."Id";
                """);
            migrationBuilder.DropColumn(name: "SongId", table: "MixVersions");

            // ---------- 5. Indexes and keys ----------
            migrationBuilder.CreateIndex(
                name: "IX_MixVersions_ProjectId_Number",
                table: "MixVersions",
                columns: new[] { "ProjectId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MixFeedback_SongId",
                table: "MixFeedback",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_MixVersionSongs_SongsId",
                table: "MixVersionSongs",
                column: "SongsId");

            migrationBuilder.AddForeignKey(
                name: "FK_MixFeedback_Songs_SongId",
                table: "MixFeedback",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MixVersions_Projects_ProjectId",
                table: "MixVersions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MixFeedback_Songs_SongId",
                table: "MixFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_MixVersions_Projects_ProjectId",
                table: "MixVersions");

            migrationBuilder.DropTable(
                name: "MixVersionSongs");

            migrationBuilder.DropIndex(
                name: "IX_MixFeedback_SongId",
                table: "MixFeedback");

            migrationBuilder.DropColumn(
                name: "MaterialLink",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "MaterialNotes",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "MaterialReceivedOn",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "MaterialTrackCount",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "MixVersions");

            migrationBuilder.DropColumn(
                name: "SongId",
                table: "MixFeedback");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "MixVersions",
                newName: "SongId");

            migrationBuilder.RenameIndex(
                name: "IX_MixVersions_ProjectId_Number",
                table: "MixVersions",
                newName: "IX_MixVersions_SongId_Number");

            migrationBuilder.AddColumn<string>(
                name: "MaterialLink",
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

            migrationBuilder.AddForeignKey(
                name: "FK_MixVersions_Songs_SongId",
                table: "MixVersions",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
