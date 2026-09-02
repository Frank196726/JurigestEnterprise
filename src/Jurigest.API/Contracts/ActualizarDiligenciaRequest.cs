using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.API.Contracts;

public sealed class ActualizarDiligenciaRequest
{
    public string Descripcion { get; set; } = string.Empty;

    public TipoDiligencia Tipo { get; set; }

    public DateTime? FechaProgramada { get; set; }

    public string? ReceptorJudicial { get; set; }

    public string? Direccion { get; set; }

    public string? Comuna { get; set; }

    public string? Observaciones { get; set; }
}
