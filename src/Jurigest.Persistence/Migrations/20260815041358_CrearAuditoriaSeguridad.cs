using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearAuditoriaSeguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditoriasSeguridad",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Accion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsuarioAfectadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Detalle = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DireccionIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriasSeguridad", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriasSeguridad_FechaUtc",
                table: "AuditoriasSeguridad",
                column: "FechaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriasSeguridad_UsuarioActorId",
                table: "AuditoriasSeguridad",
                column: "UsuarioActorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriasSeguridad_UsuarioAfectadoId",
                table: "AuditoriasSeguridad",
                column: "UsuarioAfectadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriasSeguridad");
        }
    }
}
