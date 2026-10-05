using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RelacionarModeloEstampeConGestionRealizada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DiligenciaRealizadaId",
                table: "ModelosEstampe",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelosEstampe_DiligenciaRealizadaId_Resultado_Activo",
                table: "ModelosEstampe",
                columns: new[] { "DiligenciaRealizadaId", "Resultado", "Activo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ModelosEstampe_DiligenciaRealizadaId_Resultado_Activo",
                table: "ModelosEstampe");

            migrationBuilder.DropColumn(
                name: "DiligenciaRealizadaId",
                table: "ModelosEstampe");
        }
    }
}
