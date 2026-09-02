using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechasGestionCausa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Primero se agrega como nullable para poder
            // poblar correctamente las causas existentes.
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEncargoCausa",
                table: "Causas",
                type: "datetime2",
                nullable: true);

            // La fecha de gestión debe permanecer NULL
            // hasta que exista una gestión efectiva.
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaGestionCausa",
                table: "Causas",
                type: "datetime2",
                nullable: true);

            // Para las causas históricas usamos FechaCreacion
            // como fecha inicial de encargo.
            migrationBuilder.Sql(
                """
                UPDATE [Causas]
                SET [FechaEncargoCausa] = [FechaCreacion]
                WHERE [FechaEncargoCausa] IS NULL;
                """);

            // Después de poblar los registros históricos,
            // FechaEncargoCausa pasa a ser obligatoria.
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaEncargoCausa",
                table: "Causas",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Causas_FechaGestionCausa",
                table: "Causas",
                column: "FechaGestionCausa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Causas_FechaGestionCausa",
                table: "Causas");

            migrationBuilder.DropColumn(
                name: "FechaEncargoCausa",
                table: "Causas");

            migrationBuilder.DropColumn(
                name: "FechaGestionCausa",
                table: "Causas");
        }
    }
}