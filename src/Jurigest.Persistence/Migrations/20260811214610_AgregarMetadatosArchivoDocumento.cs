using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMetadatosArchivoDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Documentos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "application/octet-stream");

            migrationBuilder.AddColumn<long>(
                name: "TamanoBytes",
                table: "Documentos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "TamanoBytes",
                table: "Documentos");
        }
    }
}
