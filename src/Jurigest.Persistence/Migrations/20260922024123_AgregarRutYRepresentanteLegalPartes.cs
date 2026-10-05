using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRutYRepresentanteLegalPartes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RepresentanteLegal",
                table: "Demandados",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rut",
                table: "Demandados",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutRepresentanteLegal",
                table: "Demandados",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentanteLegal",
                table: "AvalesSolidarios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rut",
                table: "AvalesSolidarios",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutRepresentanteLegal",
                table: "AvalesSolidarios",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepresentanteLegal",
                table: "Demandados");

            migrationBuilder.DropColumn(
                name: "Rut",
                table: "Demandados");

            migrationBuilder.DropColumn(
                name: "RutRepresentanteLegal",
                table: "Demandados");

            migrationBuilder.DropColumn(
                name: "RepresentanteLegal",
                table: "AvalesSolidarios");

            migrationBuilder.DropColumn(
                name: "Rut",
                table: "AvalesSolidarios");

            migrationBuilder.DropColumn(
                name: "RutRepresentanteLegal",
                table: "AvalesSolidarios");
        }
    }
}
