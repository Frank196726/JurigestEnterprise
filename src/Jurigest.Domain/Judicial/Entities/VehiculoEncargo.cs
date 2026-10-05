namespace Jurigest.Domain.Judicial.Entities;
public sealed class VehiculoEncargo
{
    public Guid Id { get; set; }
    public Guid DiligenciaId { get; set; }
    public int TipoPropietario { get; set; } = 1;
    public string TipoVehiculo { get; set; } = "";
    public string Serie { get; set; } = "";
    public DateTime? FechaDocumento { get; set; }
    public string TipoAdquisicion { get; set; } = "";
    public string AlzamientoProhibicion { get; set; } = "";
    public string LimitacionDominio { get; set; } = "";
    public string LugarSolicitud { get; set; } = "";
    public string NumeroSolicitud { get; set; } = "";
    public string TipoDocumento { get; set; } = "";
    public decimal DerechosInscripcion { get; set; } = 6140m;
    public string RutTitular { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Patente { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string Color { get; set; } = "";
    public string Cilindrada { get; set; } = "";
    public int? Ano { get; set; }
    public string Motor { get; set; } = "";
    public string Chasis { get; set; } = "";
    public string Observaciones { get; set; } = "";
}
