using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Panel.Queries.ObtenerPanelEjecutivo;

public sealed class ObtenerPanelEjecutivoHandler
    : IRequestHandler<ObtenerPanelEjecutivoQuery, ObtenerPanelEjecutivoResponse>
{
    private readonly ICausaRepository _causas;
    private readonly IDiligenciaRepository _diligencias;

    public ObtenerPanelEjecutivoHandler(
        ICausaRepository causas,
        IDiligenciaRepository diligencias)
    {
        _causas = causas;
        _diligencias = diligencias;
    }

    public async Task<ObtenerPanelEjecutivoResponse> Handle(
        ObtenerPanelEjecutivoQuery request,
        CancellationToken cancellationToken)
    {
        var ahora = DateTime.UtcNow;
        var hoy = ahora.Date;
        var causas = await _causas.GetAllAsync(cancellationToken);
        var diligencias = await _diligencias.GetAllAsync(cancellationToken);

        var ultimaGestionPorCausa = diligencias
            .Where(d => d.FechaRealizada.HasValue)
            .GroupBy(d => d.CausaId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Max(d => d.FechaRealizada!.Value));

        var diasSinGestion = causas.Select(causa =>
        {
            var referencia = ultimaGestionPorCausa.GetValueOrDefault(
                causa.Id,
                causa.FechaCreacion);
            return Math.Max(0, (hoy - referencia.Date).Days);
        }).ToList();

        var activas = diligencias.Where(d =>
            d.FechaProgramada.HasValue &&
            d.Estado is EstadoDiligencia.Pendiente or
                EstadoDiligencia.EnProceso or
                EstadoDiligencia.Reprogramada).ToList();

        int DiasHastaVencimiento(DateTime fecha) =>
            (fecha.Date - hoy).Days;

        return new ObtenerPanelEjecutivoResponse(
            causas.Count,
            new TramoAntiguedadResponse(
                diasSinGestion.Count(d => d <= 10),
                diasSinGestion.Count(d => d is >= 11 and <= 15),
                diasSinGestion.Count(d => d > 15)),
            new EstadoPlazosResponse(
                activas.Count(d => DiasHastaVencimiento(d.FechaProgramada!.Value) < 0),
                activas.Count(d => DiasHastaVencimiento(d.FechaProgramada!.Value) == 0),
                activas.Count(d => DiasHastaVencimiento(d.FechaProgramada!.Value) is >= 1 and <= 3),
                activas.Count(d => DiasHastaVencimiento(d.FechaProgramada!.Value) is >= 4 and <= 7),
                activas.Count(d => DiasHastaVencimiento(d.FechaProgramada!.Value) > 7)),
            ahora);
    }
}
