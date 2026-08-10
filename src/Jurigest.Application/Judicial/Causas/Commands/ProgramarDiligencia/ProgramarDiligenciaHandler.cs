using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.ProgramarDiligencia;

public sealed class ProgramarDiligenciaHandler
    : IRequestHandler<ProgramarDiligenciaCommand, bool>
{
    private readonly IDiligenciaRepository _repository;

    public ProgramarDiligenciaHandler(
        IDiligenciaRepository repository)
    {
        _repository = repository;
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

        diligencia.Programar(request.FechaProgramada);

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}