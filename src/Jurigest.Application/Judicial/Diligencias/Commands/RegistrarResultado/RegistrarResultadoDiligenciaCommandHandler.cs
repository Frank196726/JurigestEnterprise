using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.RegistrarResultado;

public sealed class RegistrarResultadoDiligenciaCommandHandler
    : IRequestHandler<RegistrarResultadoDiligenciaCommand>
{
    private readonly IDiligenciaRepository
        _diligenciaRepository;

    private readonly ICausaRepository
        _causaRepository;

    public RegistrarResultadoDiligenciaCommandHandler(
        IDiligenciaRepository diligenciaRepository,
        ICausaRepository causaRepository)
    {
        _diligenciaRepository =
            diligenciaRepository;

        _causaRepository =
            causaRepository;
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

        // -------------------------------------------------
        // 1. Registrar el resultado de la diligencia
        // -------------------------------------------------

        diligencia.RegistrarResultado(
            request.Resultado,
            request.ResultadoDetalle,
            request.Estampe,
            request.FechaGestion);

        // -------------------------------------------------
        // 2. Registrar la gestión efectiva de la causa
        // -------------------------------------------------

        causa.RegistrarGestion(
            request.FechaGestion);

        // -------------------------------------------------
        // 3. Persistir ambos cambios juntos
        //
        // Diligencia y Causa están siendo rastreadas por
        // el mismo JurigestDbContext.
        // -------------------------------------------------

        await _causaRepository.SaveChangesAsync(
            cancellationToken);
    }
}