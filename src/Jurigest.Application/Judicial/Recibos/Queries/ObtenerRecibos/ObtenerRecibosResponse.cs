using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Judicial.Recibos.Queries.ObtenerRecibos;

public sealed record ObtenerRecibosResponse(
    Guid Id,
    Guid CausaId,
    Guid DiligenciaId,
    Guid DiligenciaRealizadaId,
    string DiligenciaRealizada,
    decimal Monto,
    EstadoRecibo Estado,
    DateTime FechaEmision,
    DateTime? FechaPago);
