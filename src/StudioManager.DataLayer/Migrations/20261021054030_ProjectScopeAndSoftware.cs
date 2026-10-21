using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class ProjectScopeAndSoftware : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludesMastering",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: true); // existing projects keep all tabs

            migrationBuilder.AddColumn<bool>(
                name: "IncludesMixing",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: true); // existing projects keep all tabs

            migrationBuilder.AddColumn<bool>(
                name: "IncludesRecording",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: true); // existing projects keep all tabs

            migrationBuilder.AddColumn<bool>(
                name: "IsSoftware",
                table: "GearCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 10,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 11,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 12,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 13,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 14,
                column: "IsSoftware",
                value: false);

            migrationBuilder.UpdateData(
                table: "GearCategories",
                keyColumn: "Id",
                keyValue: 15,
                column: "IsSoftware",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludesMastering",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IncludesMixing",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IncludesRecording",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsSoftware",
                table: "GearCategories");
        }
    }
}
