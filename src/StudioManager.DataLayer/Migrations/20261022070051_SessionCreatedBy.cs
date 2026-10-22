using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class SessionCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Sessions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CreatedByUserId",
                table: "Sessions",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Users_CreatedByUserId",
                table: "Sessions",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_CreatedByUserId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_CreatedByUserId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Sessions");
        }
    }
}
