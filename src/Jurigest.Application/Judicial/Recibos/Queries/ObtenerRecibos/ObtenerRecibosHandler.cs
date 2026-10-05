using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Recibos.Queries.ObtenerRecibos;

public sealed class ObtenerRecibosHandler
    : IRequestHandler<ObtenerRecibosQuery, List<ObtenerRecibosResponse>>
{
    private readonly IReciboRepository _recibos;
    private readonly ICausaRepository _causas;

    public ObtenerRecibosHandler(
        IReciboRepository recibos, ICausaRepository causas)
    {
        _recibos = recibos;
        _causas = causas;
    }

    public async Task<List<ObtenerRecibosResponse>> Handle(
        ObtenerRecibosQuery request,
        CancellationToken cancellationToken)
    {
        var recibos =
            await _recibos.GetAllAsync(
                cancellationToken);

        var causas = (await _causas.GetAllAsync(cancellationToken)).ToDictionary(c => c.Id);
        string? Caratula(Jurigest.Domain.Judicial.Entities.Recibo recibo) =>
            !string.IsNullOrWhiteSpace(recibo.Caratulado) ? recibo.Caratulado : causas.GetValueOrDefault(recibo.CausaId)?.Descripcion;
        string? Abogado(Jurigest.Domain.Judicial.Entities.Recibo recibo)
        {
            if (!string.IsNullOrWhiteSpace(recibo.Abogado)) return recibo.Abogado;
            var observaciones = causas.GetValueOrDefault(recibo.CausaId)?.Diligencias
                .FirstOrDefault(d => d.Id == recibo.DiligenciaId)?.Observaciones;
            if (string.IsNullOrWhiteSpace(observaciones)) return null;
            const string marca = "Abogado responsable: ";
            var posicion = observaciones.IndexOf(marca, StringComparison.OrdinalIgnoreCase);
            if (posicion < 0) return null;
            var nombre = observaciones[(posicion + marca.Length)..];
            var fin = nombre.IndexOf(". Fecha de retiro:", StringComparison.OrdinalIgnoreCase);
            return (fin < 0 ? nombre : nombre[..fin]).Trim().TrimEnd('.');
        }
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
                    recibo.FechaPago) { Numero = recibo.Numero, Abogado = Abogado(recibo), Demandante = Caratula(recibo)?.Split(" / ", 2)[0].Trim(), Receptor = recibo.Receptor, Rol = recibo.Rol, Tribunal = recibo.Tribunal, Caratulado = recibo.Caratulado, DiligenciaEncargada = recibo.DiligenciaEncargada, NumeroOperacion = recibo.NumeroOperacion, Cuantia = recibo.Cuantia, ValorGestion = recibo.ValorGestion, Observacion = recibo.Observacion, DetalleAdicionales = recibo.DetalleAdicionales, TotalAdicionales = recibo.TotalAdicionales })
            .ToList();
    }
}
