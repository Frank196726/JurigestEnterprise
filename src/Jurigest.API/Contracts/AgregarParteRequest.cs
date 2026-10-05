using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.API.Contracts;

public sealed class AgregarParteRequest
{
    public string Nombre { get; set; } = string.Empty;
    public TipoPersonaDemandada TipoPersona { get; set; }
    public bool EsPrincipal { get; set; }
    public string? Rut { get; set; }
    public string? RepresentanteLegal { get; set; }
    public string? RutRepresentanteLegal { get; set; }
}

public sealed class ActualizarIdentificacionParteRequest
{
    public string Rut { get; set; } = string.Empty;
    public string? RepresentanteLegal { get; set; }
    public string? RutRepresentanteLegal { get; set; }
}
