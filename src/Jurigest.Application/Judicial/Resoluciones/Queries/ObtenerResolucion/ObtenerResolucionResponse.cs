using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Judicial.Resoluciones.Queries.ObtenerResolucion;

public sealed record ObtenerResolucionResponse(
    Guid Id,
    Guid CausaId,
    TipoResolucion Tipo,
    DateTime Fecha,
    string Descripcion);