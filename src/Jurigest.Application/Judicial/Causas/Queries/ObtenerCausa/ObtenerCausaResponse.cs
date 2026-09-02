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
    int Estado);
