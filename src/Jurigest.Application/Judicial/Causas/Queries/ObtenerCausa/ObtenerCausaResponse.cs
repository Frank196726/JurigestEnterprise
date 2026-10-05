namespace Jurigest.Application.Judicial.Causas.Queries.ObtenerCausa;

public sealed record ObtenerCausaResponse(
    Guid Id,
    string Rit,
    string Tribunal,
    string Descripcion,
    DateTime FechaCreacion,
    DateTime FechaEncargoCausa,
    DateTime? FechaGestionCausa,
    int DiasSinGestion,
    bool EsCritica,
    int Estado,
    bool PuedeCorregirIngreso,
    Guid? DiligenciaId,
    string? DiligenciaEncargada,
    DateTime? FechaProgramada,
    IReadOnlyList<DemandadoResponse> Demandados)
{
    public string? Materia { get; init; }
    public string? ReceptorJudicial { get; init; }
}

public sealed record DemandadoResponse(
    Guid Id, string Nombre, int TipoPersona, bool EsPrincipal,
    IReadOnlyList<AvalSolidarioResponse> Avales,
    string? Rut, string? RepresentanteLegal, string? RutRepresentanteLegal);

public sealed record AvalSolidarioResponse(Guid Id, string Nombre, int TipoPersona,
    string? Rut, string? RepresentanteLegal, string? RutRepresentanteLegal);
