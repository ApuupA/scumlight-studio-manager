using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudioManager.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class StudioInputsAndInputLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InputListId",
                table: "Sessions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InputGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InputLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputLists_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudioInputs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    SnakeChannel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Preamp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DawChannel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    InputGroupId = table.Column<int>(type: "integer", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudioInputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudioInputs_InputGroups_InputGroupId",
                        column: x => x.InputGroupId,
                        principalTable: "InputGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InputListRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Microphone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Stand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phantom = table.Column<bool>(type: "boolean", nullable: false),
                    Hpf = table.Column<bool>(type: "boolean", nullable: false),
                    Tone = table.Column<bool>(type: "boolean", nullable: false),
                    Eq = table.Column<bool>(type: "boolean", nullable: false),
                    Insert = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Comment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    InputListId = table.Column<int>(type: "integer", nullable: false),
                    StudioInputId = table.Column<int>(type: "integer", nullable: false),
                    SetupItemId = table.Column<int>(type: "integer", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputListRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputListRows_InputLists_InputListId",
                        column: x => x.InputListId,
                        principalTable: "InputLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InputListRows_InstrumentSetupItems_SetupItemId",
                        column: x => x.SetupItemId,
                        principalTable: "InstrumentSetupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InputListRows_StudioInputs_StudioInputId",
                        column: x => x.StudioInputId,
                        principalTable: "StudioInputs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_InputListId",
                table: "Sessions",
                column: "InputListId");

            migrationBuilder.CreateIndex(
                name: "IX_InputListRows_InputListId_StudioInputId",
                table: "InputListRows",
                columns: new[] { "InputListId", "StudioInputId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InputListRows_SetupItemId",
                table: "InputListRows",
                column: "SetupItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InputListRows_StudioInputId",
                table: "InputListRows",
                column: "StudioInputId");

            migrationBuilder.CreateIndex(
                name: "IX_InputLists_ProjectId",
                table: "InputLists",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StudioInputs_InputGroupId",
                table: "StudioInputs",
                column: "InputGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_InputLists_InputListId",
                table: "Sessions",
                column: "InputListId",
                principalTable: "InputLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_InputLists_InputListId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "InputListRows");

            migrationBuilder.DropTable(
                name: "InputLists");

            migrationBuilder.DropTable(
                name: "StudioInputs");

            migrationBuilder.DropTable(
                name: "InputGroups");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_InputListId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "InputListId",
                table: "Sessions");
        }
    }
}
