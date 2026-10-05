namespace Jurigest.API.Contracts;

public sealed class AvalSolidarioRequest
{
    public string Nombre { get; set; } = string.Empty;
    public Jurigest.Domain.Judicial.Enums.TipoPersonaDemandada TipoPersona { get; set; }
    public string? Rut { get; set; }
    public string? RepresentanteLegal { get; set; }
    public string? RutRepresentanteLegal { get; set; }
}

public sealed class DemandadoRequest
{
    public string Nombre { get; set; } = string.Empty;
    public Jurigest.Domain.Judicial.Enums.TipoPersonaDemandada TipoPersona { get; set; }
    public bool EsPrincipal { get; set; }
    public string? Rut { get; set; }
    public string? RepresentanteLegal { get; set; }
    public string? RutRepresentanteLegal { get; set; }
    public List<AvalSolidarioRequest> Avales { get; set; } = [];
}

public sealed class CrearCausaRequest
{
    public Guid Id { get; set; }

    // Compatibilidad temporal con clientes anteriores.
    public string Rit { get; set; } = string.Empty;

    public Guid? TipoCausaId { get; set; }

    public string NumeroRol { get; set; } = string.Empty;

    public string Tribunal { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public Guid? MateriaId { get; set; }
    public DateTime FechaEncargoCausa { get; set; }
    public List<DemandadoRequest> Demandados { get; set; } = [];
}
