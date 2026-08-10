using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.ObtenerCausas;

public sealed class ObtenerCausasQueryHandler
    : IRequestHandler<ObtenerCausasQuery, List<ObtenerCausasResponse>>
{
    private readonly ICausaRepository _repository;

    public ObtenerCausasQueryHandler(ICausaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ObtenerCausasResponse>> Handle(
        ObtenerCausasQuery request,
        CancellationToken cancellationToken)
    {
        var causas = await _repository.GetAllAsync(cancellationToken);

        return causas
            .Select(c => new ObtenerCausasResponse(
                c.Id,
                c.Rit,
                c.Tribunal,
                c.Descripcion,
                c.FechaCreacion,
                (int)c.Estado))
            .ToList();
    }
}