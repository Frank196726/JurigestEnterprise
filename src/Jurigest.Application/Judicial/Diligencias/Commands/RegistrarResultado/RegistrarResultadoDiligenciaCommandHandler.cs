using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.RegistrarResultado;

public sealed class RegistrarResultadoDiligenciaCommandHandler
    : IRequestHandler<RegistrarResultadoDiligenciaCommand>
{
    private readonly IDiligenciaRepository _diligenciaRepository;
    private readonly ICausaRepository _causaRepository;
    private readonly IDiligenciaRealizadaCatalogoRepository
        _diligenciaRealizadaCatalogoRepository;
    private readonly IReciboRepository _reciboRepository;

    public RegistrarResultadoDiligenciaCommandHandler(
        IDiligenciaRepository diligenciaRepository,
        ICausaRepository causaRepository,
        IDiligenciaRealizadaCatalogoRepository diligenciaRealizadaCatalogoRepository,
        IReciboRepository reciboRepository)
    {
        _diligenciaRepository = diligenciaRepository;
        _causaRepository = causaRepository;
        _diligenciaRealizadaCatalogoRepository =
            diligenciaRealizadaCatalogoRepository;
        _reciboRepository = reciboRepository;
    }

    public async Task Handle(
        RegistrarResultadoDiligenciaCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia =
            await _diligenciaRepository.GetByIdAsync(
                request.DiligenciaId,
                cancellationToken);

        if (diligencia is null)
        {
            throw new InvalidOperationException(
                "La diligencia no existe.");
        }

        var causa =
            await _causaRepository.GetByIdAsync(
                diligencia.CausaId,
                cancellationToken);

        if (causa is null)
        {
            throw new InvalidOperationException(
                "La causa asociada a la diligencia no existe.");
        }

        var diligenciaRealizada =
            await _diligenciaRealizadaCatalogoRepository.GetByIdAsync(
                request.DiligenciaRealizadaId,
                cancellationToken);

        if (diligenciaRealizada is null ||
            !diligenciaRealizada.Activo)
        {
            throw new ArgumentException(
                "La diligencia realizada seleccionada no existe o no está activa.");
        }

        if (diligenciaRealizada.CodigoTipoDiligencia !=
            (int)diligencia.Tipo)
        {
            throw new ArgumentException(
                "La diligencia realizada seleccionada no corresponde al tipo de diligencia encargada.");
        }

        var reciboExistente = await _reciboRepository.GetByDiligenciaIdAsync(diligencia.Id, cancellationToken);
        var monto = request.Monto ?? reciboExistente?.ValorGestion ?? diligenciaRealizada.Arancel ?? 0m;
        Jurigest.Domain.Judicial.MontoEstampe.Validar(monto);
        if (reciboExistente is not null && (reciboExistente.ValorGestion != monto ||
            reciboExistente.DiligenciaRealizadaId != diligenciaRealizada.Id))
            throw new Jurigest.Domain.Judicial.ReciboEmitidoException();

        Jurigest.Domain.Judicial.MontoEstampe.Validar(request.TotalAdicionales);
        Jurigest.Domain.Judicial.MontoEstampe.Validar(monto + request.TotalAdicionales);
        if (request.Cuantia.HasValue) Jurigest.Domain.Judicial.MontoEstampe.Validar(request.Cuantia.Value);
        if (reciboExistente is not null && (reciboExistente.TotalAdicionales != request.TotalAdicionales ||
            reciboExistente.NumeroOperacion != request.NumeroOperacion || reciboExistente.Cuantia != request.Cuantia ||
            reciboExistente.Abogado != request.Abogado || reciboExistente.Observacion != request.ObservacionRecibo ||
            reciboExistente.DetalleAdicionales != request.DetalleAdicionales))
            throw new Jurigest.Domain.Judicial.ReciboEmitidoException();

        var estampe = request.Monto.HasValue || monto > 0
            ? Jurigest.Domain.Judicial.MontoEstampe.Aplicar(request.Estampe, monto)
            : request.Estampe;
        diligencia.RegistrarResultado(
            diligenciaRealizada.Id,
            diligenciaRealizada.Nombre,
            request.Resultado,
            request.ResultadoDetalle,
            estampe,
            request.FechaGestion);

        causa.RegistrarGestion(
            request.FechaGestion);

        if (monto + request.TotalAdicionales > 0)
        {
            if (reciboExistente is null)
            {
                var recibo = new Recibo(
                    Guid.NewGuid(),
                    causa.Id,
                    diligencia.Id,
                    diligenciaRealizada.Id,
                    diligenciaRealizada.Nombre,
                    monto + request.TotalAdicionales,
                    request.FechaGestion);

                // La emisión conserva los datos de la causa y del encargo.
                recibo.CompletarDatos(request.Abogado, diligencia.ReceptorJudicial, causa.Rit, causa.Tribunal,
                    causa.Descripcion, diligencia.Descripcion, request.NumeroOperacion, request.Cuantia,
                    request.ObservacionRecibo, request.DetalleAdicionales, request.TotalAdicionales, monto);

                await _reciboRepository.AddAsync(
                    recibo,
                    cancellationToken);
            }
        }

        await _causaRepository.SaveChangesAsync(
            cancellationToken);
    }
}
