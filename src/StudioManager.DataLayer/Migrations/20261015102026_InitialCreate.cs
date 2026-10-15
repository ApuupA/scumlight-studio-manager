using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GearCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GearCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GearItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PurchaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PhotoPath = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    GearCategoryId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GearItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GearItems_GearCategories_GearCategoryId",
                        column: x => x.GearCategoryId,
                        principalTable: "GearCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "GearCategories",
                columns: new[] { "Id", "DateCreated", "DateUpdated", "Description", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Microphones", 10 },
                    { 2, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Preamps", 20 },
                    { 3, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Dynamics", 30 },
                    { 4, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Equalizers", 40 },
                    { 5, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Effects", 50 },
                    { 6, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Interfaces & Converters", 60 },
                    { 7, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Monitoring", 70 },
                    { 8, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Headphones", 80 },
                    { 9, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Instruments", 90 },
                    { 10, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Amplifiers", 100 },
                    { 11, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Drums & Percussion", 110 },
                    { 12, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "DI Boxes", 120 },
                    { 13, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cables", 130 },
                    { 14, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Stands", 140 },
                    { 15, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "Other", 150 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GearCategories_Name",
                table: "GearCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GearItems_GearCategoryId",
                table: "GearItems",
                column: "GearCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GearItems");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "GearCategories");
        }
    }
}
