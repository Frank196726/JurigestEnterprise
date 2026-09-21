namespace Jurigest.Web.Models;

public sealed record ReciboResumen(
    Guid Id,
    Guid CausaId,
    Guid DiligenciaId,
    Guid DiligenciaRealizadaId,
    string DiligenciaRealizada,
    decimal Monto,
    int Estado,
    DateTime FechaEmision,
    DateTime? FechaPago);
