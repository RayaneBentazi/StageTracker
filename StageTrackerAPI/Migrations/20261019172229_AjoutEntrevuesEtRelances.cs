using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StageTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AjoutEntrevuesEtRelances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateProchaineRelance",
                table: "Candidatures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Entrevues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatureId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DureeMinutes = table.Column<int>(type: "int", nullable: true),
                    Interlocuteurs = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entrevues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entrevues_Candidatures_CandidatureId",
                        column: x => x.CandidatureId,
                        principalTable: "Candidatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entrevues_CandidatureId",
                table: "Entrevues",
                column: "CandidatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entrevues");

            migrationBuilder.DropColumn(
                name: "DateProchaineRelance",
                table: "Candidatures");
        }
    }
}
