using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarFormularioVehiculosRvm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlzamientoProhibicion",
                table: "VehiculosEncargo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DerechosInscripcion",
                table: "VehiculosEncargo",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDocumento",
                table: "VehiculosEncargo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LimitacionDominio",
                table: "VehiculosEncargo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LugarSolicitud",
                table: "VehiculosEncargo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroSolicitud",
                table: "VehiculosEncargo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RutTitular",
                table: "VehiculosEncargo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Serie",
                table: "VehiculosEncargo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoAdquisicion",
                table: "VehiculosEncargo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoDocumento",
                table: "VehiculosEncargo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TipoPropietario",
                table: "VehiculosEncargo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TipoVehiculo",
                table: "VehiculosEncargo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "VehiculoOpciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehiculoOpciones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehiculoOpciones_Categoria_Nombre",
                table: "VehiculoOpciones",
                columns: new[] { "Categoria", "Nombre" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehiculoOpciones");

            migrationBuilder.DropColumn(
                name: "AlzamientoProhibicion",
                table: "VehiculosEncargo");

            migrationBuilder.DropColumn(
                name: "DerechosInscripcion",
                table: "VehiculosEncargo");

            migrationBuilder.DropColumn(
                name: "FechaDocumento",
                table: "VehiculosEncargo");

            migrationBuilder.DropColumn(
                name: "LimitacionDominio",
                table: "VehiculosEncargo");

            migrationBuilder.DropColumn(
                name: "LugarSolicitud",
                table: "VehiculosEncargo");

            migrationBuilder.DropColumn(
                name: "NumeroSolicitud",
                table: "VehiculosEncargo");

            migrationBuilder.DropColumn(
                name: "RutTitular",
                table: "VehiculosEncargo");

            migrationBuilder.DropColumn(
                name: "Serie",
                table: "VehiculosEncargo");

            migrationBuilder.DropColumn(
                name: "TipoAdquisicion",
                table: "VehiculosEncargo");

            migrationBuilder.DropColumn(
                name: "TipoDocumento",
                table: "VehiculosEncargo");

            migrationBuilder.DropColumn(
                name: "TipoPropietario",
                table: "VehiculosEncargo");

            migrationBuilder.DropColumn(
                name: "TipoVehiculo",
                table: "VehiculosEncargo");
        }
    }
}
