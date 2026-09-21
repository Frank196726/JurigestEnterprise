using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarArancelDiligenciaRealizada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Arancel",
                table: "DiligenciasRealizadas",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE DiligenciasRealizadas
                SET Arancel = 60000
                WHERE Id = '3D095903-4962-46D5-BD7B-799FC02D5A4C';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Arancel",
                table: "DiligenciasRealizadas");
        }
    }
}

