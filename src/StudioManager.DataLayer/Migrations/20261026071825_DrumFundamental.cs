using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class DrumFundamental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FundamentalHz",
                table: "DrumTunings",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FundamentalHz",
                table: "DrumTunings");
        }
    }
}
