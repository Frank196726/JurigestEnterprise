using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligenciasPorPlazo;

public sealed class ObtenerDiligenciasPorPlazoHandler
    : IRequestHandler<ObtenerDiligenciasPorPlazoQuery, List<ObtenerDiligenciasPorPlazoResponse>>
{
    private readonly IDiligenciaRepository _diligencias;
    private readonly ICausaRepository _causas;

    public ObtenerDiligenciasPorPlazoHandler(
        IDiligenciaRepository diligencias,
        ICausaRepository causas)
    {
        _diligencias = diligencias;
        _causas = causas;
    }

    public async Task<List<ObtenerDiligenciasPorPlazoResponse>> Handle(
        ObtenerDiligenciasPorPlazoQuery request,
        CancellationToken cancellationToken)
    {
        var hoy = DateTime.UtcNow.Date;
        var diligencias = await _diligencias.GetAllAsync(cancellationToken);
        var causas = (await _causas.GetAllAsync(cancellationToken))
            .ToDictionary(causa => causa.Id);

        return diligencias
            .Where(diligencia =>
                diligencia.FechaProgramada.HasValue &&
                diligencia.Estado is EstadoDiligencia.Pendiente or
                    EstadoDiligencia.EnProceso or
                    EstadoDiligencia.Reprogramada &&
                causas.ContainsKey(diligencia.CausaId))
            .Select(diligencia =>
            {
                var causa = causas[diligencia.CausaId];
                var fecha = diligencia.FechaProgramada!.Value;

                return new ObtenerDiligenciasPorPlazoResponse(
                    diligencia.Id,
                    diligencia.CausaId,
                    causa.Rit,
                    causa.Tribunal,
                    diligencia.Descripcion,
                    diligencia.Estado,
                    diligencia.Tipo,
                    fecha,
                    (fecha.Date - hoy).Days,
                    diligencia.ReceptorJudicial);
            })
            .OrderBy(item => item.FechaProgramada)
            .ThenBy(item => item.Rit)
            .ToList();
    }
}
