using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Resoluciones.Commands.EliminarResolucion;

    public sealed class EliminarResolucionHandler
        : IRequestHandler<EliminarResolucionCommand, bool>
{
    private readonly IResolucionRepository _repository;

    public EliminarResolucionHandler(IResolucionRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        EliminarResolucionCommand request,
        CancellationToken cancellationToken)
    {
        var resolucion = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (resolucion is null)
            return false;

            await _repository.DeleteAsync(
                resolucion,
                cancellationToken);

            return true;
    }
}