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
    int Estado);