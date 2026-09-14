using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.ProgramarDiligencia;

public sealed class ProgramarDiligenciaHandler
    : IRequestHandler<ProgramarDiligenciaCommand, bool>
{
    private readonly IDiligenciaRepository _repository;
    private readonly ICausaRepository _causas;

    public ProgramarDiligenciaHandler(
        IDiligenciaRepository repository, ICausaRepository causas)
    {
        _repository = repository;
        _causas = causas;
    }

    public async Task<bool> Handle(
        ProgramarDiligenciaCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return false;

        var causa = await _causas.GetByIdAsync(diligencia.CausaId, cancellationToken)
            ?? throw new InvalidOperationException("La causa no existe.");
        causa.ValidarCorreccionDiligencia(diligencia.Id);

        diligencia.Programar(request.FechaProgramada);

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}