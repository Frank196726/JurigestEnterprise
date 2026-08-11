using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Judicial.Resoluciones.Queries.ObtenerResolucion;
using MediatR;

namespace Jurigest.Application.Judicial.Resoluciones.Queries.ObtenerResolucionesPorCausa;

public sealed class ObtenerResolucionesPorCausaHandler
    : IRequestHandler<
        ObtenerResolucionesPorCausaQuery,
        List<ObtenerResolucionResponse>>
{
    private readonly IResolucionRepository _repository;

    public ObtenerResolucionesPorCausaHandler(
        IResolucionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ObtenerResolucionResponse>> Handle(
        ObtenerResolucionesPorCausaQuery request,
        CancellationToken cancellationToken)
    {
        var resoluciones = await _repository.GetByCausaIdAsync(
            request.CausaId,
            cancellationToken);

        return resoluciones
            .Select(resolucion => new ObtenerResolucionResponse(
                resolucion.Id,
                resolucion.CausaId,
                resolucion.Tipo,
                resolucion.Fecha,
                resolucion.Descripcion))
            .ToList();
    }
}