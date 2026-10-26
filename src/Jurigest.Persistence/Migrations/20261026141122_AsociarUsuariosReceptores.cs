using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AsociarUsuariosReceptores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuariosReceptores",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceptorJudicialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosReceptores", x => new { x.UsuarioId, x.ReceptorJudicialId });
                    table.ForeignKey(
                        name: "FK_UsuariosReceptores_ReceptoresJudiciales_ReceptorJudicialId",
                        column: x => x.ReceptorJudicialId,
                        principalTable: "ReceptoresJudiciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosReceptores_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosReceptores_ReceptorJudicialId",
                table: "UsuariosReceptores",
                column: "ReceptorJudicialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuariosReceptores");
        }
    }
}
