using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarResultadoYEstampeDiligencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estampe",
                table: "Diligencias",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaGestion",
                table: "Diligencias",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Resultado",
                table: "Diligencias",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estampe",
                table: "Diligencias");

            migrationBuilder.DropColumn(
                name: "FechaGestion",
                table: "Diligencias");

            migrationBuilder.DropColumn(
                name: "Resultado",
                table: "Diligencias");
        }
    }
}
