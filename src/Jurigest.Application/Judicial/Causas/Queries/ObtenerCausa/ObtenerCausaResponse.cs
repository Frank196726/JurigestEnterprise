using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Judicial.Causas.Queries.ObtenerCausa;

public sealed record ObtenerCausaResponse(
    Guid Id,
    string Rit,
    string Tribunal,
    string Descripcion,
    DateTime FechaCreacion,
    EstadoCausa Estado);