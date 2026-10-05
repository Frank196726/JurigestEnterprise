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
            recibo.FechaPago) { Numero = recibo.Numero, Abogado = recibo.Abogado, Receptor = recibo.Receptor, Rol = recibo.Rol, Tribunal = recibo.Tribunal, Caratulado = recibo.Caratulado, DiligenciaEncargada = recibo.DiligenciaEncargada, NumeroOperacion = recibo.NumeroOperacion, Cuantia = recibo.Cuantia, ValorGestion = recibo.ValorGestion, Observacion = recibo.Observacion, DetalleAdicionales = recibo.DetalleAdicionales, TotalAdicionales = recibo.TotalAdicionales };
    }
}
