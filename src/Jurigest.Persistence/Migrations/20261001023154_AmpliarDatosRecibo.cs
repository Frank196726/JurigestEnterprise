using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarDatosRecibo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Abogado",
                table: "Recibos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Caratulado",
                table: "Recibos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Cuantia",
                table: "Recibos",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetalleAdicionales",
                table: "Recibos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiligenciaEncargada",
                table: "Recibos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Numero",
                table: "Recibos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "NumeroOperacion",
                table: "Recibos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observacion",
                table: "Recibos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Receptor",
                table: "Recibos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rol",
                table: "Recibos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAdicionales",
                table: "Recibos",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Tribunal",
                table: "Recibos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorGestion",
                table: "Recibos",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
                UPDATE r SET ValorGestion = r.Monto,
                    Rol = LEFT(c.Rit, 100), Tribunal = LEFT(c.Tribunal, 200),
                    Caratulado = LEFT(c.Descripcion, 500),
                    Receptor = LEFT(d.ReceptorJudicial, 200),
                    DiligenciaEncargada = LEFT(d.Descripcion, 500)
                FROM Recibos r
                JOIN Causas c ON c.Id = r.CausaId
                JOIN Diligencias d ON d.Id = r.DiligenciaId;");

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_Numero",
                table: "Recibos",
                column: "Numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recibos_Numero",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "Abogado",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "Caratulado",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "Cuantia",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "DetalleAdicionales",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "DiligenciaEncargada",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "NumeroOperacion",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "Observacion",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "Receptor",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "TotalAdicionales",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "Tribunal",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "ValorGestion",
                table: "Recibos");
        }
    }
}
