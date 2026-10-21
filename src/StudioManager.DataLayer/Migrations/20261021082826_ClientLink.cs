using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class ClientLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "ProjectLinks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnread",
                table: "ProjectLinks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "MixVersions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "MixFeedback",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFromClient",
                table: "MixFeedback",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnread",
                table: "MixFeedback",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "ProjectLinks");

            migrationBuilder.DropColumn(
                name: "IsUnread",
                table: "ProjectLinks");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "MixVersions");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "MixFeedback");

            migrationBuilder.DropColumn(
                name: "IsFromClient",
                table: "MixFeedback");

            migrationBuilder.DropColumn(
                name: "IsUnread",
                table: "MixFeedback");
        }
    }
}
