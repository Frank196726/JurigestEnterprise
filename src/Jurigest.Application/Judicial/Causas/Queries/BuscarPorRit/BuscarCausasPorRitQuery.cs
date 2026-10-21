using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.BuscarPorRit;

// Todas las causas con ese ROL, una por tribunal, de la más antigua a la más nueva.
public sealed record BuscarCausasPorRitQuery(
    string Rit,
    string? ReceptorAsignado = null)
    : IRequest<List<BuscarCausaPorRitResponse>>;

public sealed class BuscarCausasPorRitHandler(ICausaRepository repository)
    : IRequestHandler<BuscarCausasPorRitQuery, List<BuscarCausaPorRitResponse>>
{
    public async Task<List<BuscarCausaPorRitResponse>> Handle(
        BuscarCausasPorRitQuery request,
        CancellationToken cancellationToken)
    {
        var causas = await repository.GetAllByRitAsync(request.Rit, cancellationToken);

        return causas
            .Where(x => BuscarCausaPorRitMapper.EsVisiblePara(x, request.ReceptorAsignado))
            .Select(BuscarCausaPorRitMapper.Mapear)
            .ToList();
    }
}
