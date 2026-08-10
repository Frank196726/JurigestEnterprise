namespace Jurigest.Application.Judicial.Causas.Queries.ObtenerCausas;

public sealed record ObtenerCausasResponse(
    Guid Id,
    string Rit,
    string Tribunal,
    string Descripcion,
    DateTime FechaCreacion,
    int Estado);