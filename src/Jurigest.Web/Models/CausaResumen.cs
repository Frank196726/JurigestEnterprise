namespace Jurigest.Web.Models;

public sealed record CausaResumen(
    Guid Id,
    string Rit,
    string Tribunal,
    string Descripcion,
    DateTime FechaCreacion,
    DateTime FechaEncargoCausa,
    DateTime? FechaGestionCausa,
    int DiasSinGestion,
    bool EsCritica,
    int Estado)
{
    public string? Materia { get; init; }
    public IReadOnlyList<DemandadoResumen> Demandados { get; init; } = [];
}

public sealed record DemandadoResumen(
    Guid Id, string Nombre, int TipoPersona, bool EsPrincipal,
    IReadOnlyList<AvalSolidarioResumen> Avales,
    string? Rut, string? RepresentanteLegal, string? RutRepresentanteLegal);

public sealed record AvalSolidarioResumen(Guid Id, string Nombre, int TipoPersona,
    string? Rut, string? RepresentanteLegal, string? RutRepresentanteLegal);
