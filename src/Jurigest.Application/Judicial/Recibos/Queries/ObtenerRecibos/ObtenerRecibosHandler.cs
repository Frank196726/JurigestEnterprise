using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Recibos.Queries.ObtenerRecibos;

public sealed class ObtenerRecibosHandler
    : IRequestHandler<ObtenerRecibosQuery, List<ObtenerRecibosResponse>>
{
    private readonly IReciboRepository _recibos;

    public ObtenerRecibosHandler(
        IReciboRepository recibos)
    {
        _recibos = recibos;
    }

    public async Task<List<ObtenerRecibosResponse>> Handle(
        ObtenerRecibosQuery request,
        CancellationToken cancellationToken)
    {
        var recibos =
            await _recibos.GetAllAsync(
                cancellationToken);

        return recibos
            .Select(recibo =>
                new ObtenerRecibosResponse(
                    recibo.Id,
                    recibo.CausaId,
                    recibo.DiligenciaId,
                    recibo.DiligenciaRealizadaId,
                    recibo.DiligenciaRealizada,
                    recibo.Monto,
                    recibo.Estado,
                    recibo.FechaEmision,
                    recibo.FechaPago))
            .ToList();
    }
}
