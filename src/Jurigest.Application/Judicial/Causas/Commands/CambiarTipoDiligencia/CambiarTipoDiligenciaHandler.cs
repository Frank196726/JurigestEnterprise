using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.CambiarTipoDiligencia;

public sealed class CambiarTipoDiligenciaHandler
    : IRequestHandler<CambiarTipoDiligenciaCommand, bool>
{
    private readonly IDiligenciaRepository _repository;
    private readonly ICausaRepository _causas;

    public CambiarTipoDiligenciaHandler(
        IDiligenciaRepository repository, ICausaRepository causas)
    {
        _repository = repository;
        _causas = causas;
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

        var causa = await _causas.GetByIdAsync(diligencia.CausaId, cancellationToken)
            ?? throw new InvalidOperationException("La causa no existe.");
        causa.ValidarCorreccionDiligencia(diligencia.Id);

        diligencia.CambiarTipo(request.Tipo);

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}
