using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.CambiarTipoDiligencia;

public sealed class CambiarTipoDiligenciaHandler
    : IRequestHandler<CambiarTipoDiligenciaCommand, bool>
{
    private readonly IDiligenciaRepository _repository;

    public CambiarTipoDiligenciaHandler(
        IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        CambiarTipoDiligenciaCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return false;

        diligencia.CambiarTipo(request.Tipo);

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}
