using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class SessionEndDateGearGoodForRemoveSongKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "Songs");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "Sessions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "GoodFor",
                table: "GearItems",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "GoodFor",
                table: "GearItems");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Songs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
