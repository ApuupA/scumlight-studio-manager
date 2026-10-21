using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class DemoVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDemo",
                table: "MixVersions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDemo",
                table: "MixVersions");
        }
    }
}
