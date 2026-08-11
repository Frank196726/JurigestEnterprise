using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Resoluciones.Queries.ObtenerResolucion;

public sealed class ObtenerResolucionHandler
    : IRequestHandler<ObtenerResolucionQuery, ObtenerResolucionResponse?>
{
    private readonly IResolucionRepository _repository;

    public ObtenerResolucionHandler(IResolucionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ObtenerResolucionResponse?> Handle(
        ObtenerResolucionQuery request,
        CancellationToken cancellationToken)
    {
        var resolucion = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (resolucion is null)
            return null;

        return new ObtenerResolucionResponse(
            resolucion.Id,
            resolucion.CausaId,
            resolucion.Tipo,
            resolucion.Fecha,
            resolucion.Descripcion);
    }
}