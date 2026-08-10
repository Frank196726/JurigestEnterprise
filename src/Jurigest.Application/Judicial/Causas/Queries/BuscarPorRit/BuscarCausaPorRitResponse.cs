namespace Jurigest.Application.Judicial.Causas.Queries.BuscarPorRit;

public sealed record BuscarCausaPorRitResponse(
    Guid Id,
    string Rit,
    string Tribunal,
    string Descripcion,
    DateTime FechaCreacion);