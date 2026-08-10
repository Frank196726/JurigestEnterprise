using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.IniciarDiligencia;

public sealed class IniciarDiligenciaHandler
    : IRequestHandler<IniciarDiligenciaCommand, bool>
{
    private readonly IDiligenciaRepository _repository;

    public IniciarDiligenciaHandler(
        IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        IniciarDiligenciaCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return false;

        diligencia.Iniciar();

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}