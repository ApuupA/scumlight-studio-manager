using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class StandardTasksBusinessData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StandardTaskId",
                table: "TimeEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "BusinessSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bic",
                table: "BusinessSettings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "BusinessSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "BusinessSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "BusinessSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "BusinessSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // The defaults are the real defaults (EF would set 0 / "" / false for an existing settings row)
            migrationBuilder.AddColumn<int>(
                name: "InvoiceNumberDigits",
                table: "BusinessSettings",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumberPrefix",
                table: "BusinessSettings",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "{YYYY}-");

            migrationBuilder.AddColumn<bool>(
                name: "IsSmallBusiness",
                table: "BusinessSettings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "NextInvoiceNumber",
                table: "BusinessSettings",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "BusinessSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermDays",
                table: "BusinessSettings",
                type: "integer",
                nullable: false,
                defaultValue: 14);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "BusinessSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "BusinessSettings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "BusinessSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "BusinessSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatId",
                table: "BusinessSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "BusinessSettings",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 19m);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "BusinessSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StandardTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    HoursPerUnit = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardTasks", x => x.Id);
                });

            // A few standard tasks to start with - they can be changed, archived or deleted in the settings
            migrationBuilder.Sql(@"
                INSERT INTO ""StandardTasks"" (""Name"", ""Unit"", ""HoursPerUnit"", ""SortOrder"", ""IsArchived"", ""DateCreated"", ""DateUpdated"") VALUES
                ('Recording / Tag', 'Day', 8, 1, false, now(), now()),
                ('Recording / Stunde', 'Hour', 1, 2, false, now(), now()),
                ('Mixing / Stunde', 'Hour', 1, 3, false, now(), now()),
                ('Editing / Stunde', 'Hour', 1, 4, false, now(), now());");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_StandardTaskId",
                table: "TimeEntries",
                column: "StandardTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_StandardTasks_StandardTaskId",
                table: "TimeEntries",
                column: "StandardTaskId",
                principalTable: "StandardTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_StandardTasks_StandardTaskId",
                table: "TimeEntries");

            migrationBuilder.DropTable(
                name: "StandardTasks");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_StandardTaskId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "StandardTaskId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "Bic",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "City",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "InvoiceNumberDigits",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "InvoiceNumberPrefix",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "IsSmallBusiness",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "NextInvoiceNumber",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "PaymentTermDays",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "VatId",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "BusinessSettings");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "BusinessSettings");
        }
    }
}
