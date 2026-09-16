using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarResultadoDiligenciaRealizada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiligenciaRealizada",
                table: "Diligencias",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DiligenciaRealizadaId",
                table: "Diligencias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diligencias_DiligenciaRealizadaId",
                table: "Diligencias",
                column: "DiligenciaRealizadaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Diligencias_DiligenciaRealizadaId",
                table: "Diligencias");

            migrationBuilder.DropColumn(
                name: "DiligenciaRealizada",
                table: "Diligencias");

            migrationBuilder.DropColumn(
                name: "DiligenciaRealizadaId",
                table: "Diligencias");
        }
    }
}
