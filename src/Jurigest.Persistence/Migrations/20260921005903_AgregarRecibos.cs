using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRecibos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recibos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CausaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiligenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiligenciaRealizadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiligenciaRealizada = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recibos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recibos_Causas_CausaId",
                        column: x => x.CausaId,
                        principalTable: "Causas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recibos_Diligencias_DiligenciaId",
                        column: x => x.DiligenciaId,
                        principalTable: "Diligencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_CausaId",
                table: "Recibos",
                column: "CausaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_DiligenciaId",
                table: "Recibos",
                column: "DiligenciaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_DiligenciaRealizadaId",
                table: "Recibos",
                column: "DiligenciaRealizadaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recibos");
        }
    }
}
