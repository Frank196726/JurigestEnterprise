using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDiligenciasRealizadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiligenciasRealizadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CodigoTipoDiligencia = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiligenciasRealizadas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiligenciasRealizadas_CodigoTipoDiligencia",
                table: "DiligenciasRealizadas",
                column: "CodigoTipoDiligencia");

            migrationBuilder.CreateIndex(
                name: "IX_DiligenciasRealizadas_CodigoTipoDiligencia_Nombre",
                table: "DiligenciasRealizadas",
                columns: new[] { "CodigoTipoDiligencia", "Nombre" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiligenciasRealizadas");
        }
    }
}
