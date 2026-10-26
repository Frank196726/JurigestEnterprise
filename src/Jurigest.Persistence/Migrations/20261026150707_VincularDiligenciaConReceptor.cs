using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VincularDiligenciaConReceptor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReceptorJudicialId",
                table: "Diligencias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diligencias_ReceptorJudicialId",
                table: "Diligencias",
                column: "ReceptorJudicialId");

            migrationBuilder.AddForeignKey(
                name: "FK_Diligencias_ReceptoresJudiciales_ReceptorJudicialId",
                table: "Diligencias",
                column: "ReceptorJudicialId",
                principalTable: "ReceptoresJudiciales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Vincula las diligencias existentes con el receptor del catálogo de igual nombre.
            // Los nombres del catálogo son únicos; los que no coinciden quedan como texto libre.
            // EXEC: en un script SQL la columna nueva aún no existe al compilar el lote.
            migrationBuilder.Sql("""
                EXEC(N'
                UPDATE d
                SET d.ReceptorJudicialId = r.Id
                FROM Diligencias d
                INNER JOIN ReceptoresJudiciales r
                    ON r.Nombre = LTRIM(RTRIM(d.ReceptorJudicial))
                WHERE d.ReceptorJudicial IS NOT NULL;');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diligencias_ReceptoresJudiciales_ReceptorJudicialId",
                table: "Diligencias");

            migrationBuilder.DropIndex(
                name: "IX_Diligencias_ReceptorJudicialId",
                table: "Diligencias");

            migrationBuilder.DropColumn(
                name: "ReceptorJudicialId",
                table: "Diligencias");
        }
    }
}
