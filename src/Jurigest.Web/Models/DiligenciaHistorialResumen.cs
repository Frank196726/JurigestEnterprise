namespace Jurigest.Web.Models;

public sealed record DiligenciaHistorialResumen(
    Guid Id,
    string Descripcion,
    int Estado,
    int Tipo,
    int Resultado,
    string? ResultadoDetalle,
    DateTime FechaCreacion,
    DateTime? FechaProgramada,
    DateTime? FechaRealizada,
    DateTime? FechaGestion,
    string? ReceptorJudicial,
    string? Direccion,
    string? Comuna,
    string? Estampe);
