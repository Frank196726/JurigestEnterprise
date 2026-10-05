namespace Jurigest.Application.Judicial.Causas.Queries.BuscarPorRit;

public sealed record BuscarCausaPorRitResponse(
    Guid Id,
    string Rit,
    string Tribunal,
    string Descripcion,
    DateTime FechaCreacion)
{
    public string? NombreDemandado { get; init; }
    public string? Direccion { get; init; }
    public string? Comuna { get; init; }
    public string? Demandante { get; init; }
    public string? Demandado { get; init; }
}
