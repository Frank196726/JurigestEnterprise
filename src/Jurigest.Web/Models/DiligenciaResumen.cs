namespace Jurigest.Web.Models;

public sealed record DiligenciaResumen(
    Guid Id,
    string Descripcion,
    int Estado,
    int Tipo,
    DateTime FechaCreacion,
    DateTime? FechaProgramada,
    DateTime? FechaRealizada,
    string? ReceptorJudicial,
    string? Direccion,
    string? Comuna);