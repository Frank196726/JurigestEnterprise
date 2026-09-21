using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Judicial.Recibos.Queries.ObtenerRecibos;
using MediatR;

namespace Jurigest.Application.Judicial.Recibos.Queries.ObtenerRecibo;

public sealed class ObtenerReciboHandler
    : IRequestHandler<ObtenerReciboQuery, ObtenerRecibosResponse?>
{
    private readonly IReciboRepository _recibos;

    public ObtenerReciboHandler(
        IReciboRepository recibos)
    {
        _recibos = recibos;
    }

    public async Task<ObtenerRecibosResponse?> Handle(
        ObtenerReciboQuery request,
        CancellationToken cancellationToken)
    {
        var recibo =
            await _recibos.GetByIdAsync(
                request.Id,
                cancellationToken);

        if (recibo is null)
        {
            return null;
        }

        return new ObtenerRecibosResponse(
            recibo.Id,
            recibo.CausaId,
            recibo.DiligenciaId,
            recibo.DiligenciaRealizadaId,
            recibo.DiligenciaRealizada,
            recibo.Monto,
            recibo.Estado,
            recibo.FechaEmision,
            recibo.FechaPago);
    }
}
