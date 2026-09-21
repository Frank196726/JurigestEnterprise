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

        diligencia.RegistrarResultado(
            diligenciaRealizada.Id,
            diligenciaRealizada.Nombre,
            request.Resultado,
            request.ResultadoDetalle,
            request.Estampe,
            request.FechaGestion);

        causa.RegistrarGestion(
            request.FechaGestion);

        if (diligenciaRealizada.Arancel.HasValue &&
            diligenciaRealizada.Arancel.Value > 0)
        {
            var reciboExistente =
                await _reciboRepository.GetByDiligenciaIdAsync(
                    diligencia.Id,
                    cancellationToken);

            if (reciboExistente is null)
            {
                var recibo = new Recibo(
                    Guid.NewGuid(),
                    causa.Id,
                    diligencia.Id,
                    diligenciaRealizada.Id,
                    diligenciaRealizada.Nombre,
                    diligenciaRealizada.Arancel.Value,
                    request.FechaGestion);

                await _reciboRepository.AddAsync(
                    recibo,
                    cancellationToken);
            }
        }

        await _causaRepository.SaveChangesAsync(
            cancellationToken);
    }
}
