namespace Jurigest.API.Contracts;

public sealed class ActualizarCausaRequest
{
    public string Tribunal { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public DateTime? FechaEncargoCausa { get; set; }

    public Guid? DiligenciaId { get; set; }

    public Guid? DiligenciaEncargadaId { get; set; }

    public DateTime? FechaProgramada { get; set; }
}