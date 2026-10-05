using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Judicial.Causas.Commands.CrearCausa;

public sealed record AvalSolidarioInput(string Nombre, TipoPersonaDemandada TipoPersona,
    string? Rut = null, string? RepresentanteLegal = null, string? RutRepresentanteLegal = null);

public sealed record DemandadoInput(
    string Nombre,
    TipoPersonaDemandada TipoPersona,
    bool EsPrincipal,
    List<AvalSolidarioInput> Avales,
    string? Rut = null,
    string? RepresentanteLegal = null,
    string? RutRepresentanteLegal = null);
