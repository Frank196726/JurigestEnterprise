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
    string? Comuna)
{
    public int Resultado { get; init; }
    public string? Estampe { get; init; }
    public string? DiligenciaRealizada { get; init; }
    public DateTime? FechaGestion { get; init; }
}