using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class DemoVersionNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MixVersions_ProjectId_Number",
                table: "MixVersions");

            migrationBuilder.CreateIndex(
                name: "IX_MixVersions_ProjectId_IsDemo_Number",
                table: "MixVersions",
                columns: new[] { "ProjectId", "IsDemo", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MixVersions_ProjectId_IsDemo_Number",
                table: "MixVersions");

            migrationBuilder.CreateIndex(
                name: "IX_MixVersions_ProjectId_Number",
                table: "MixVersions",
                columns: new[] { "ProjectId", "Number" },
                unique: true);
        }
    }
}
