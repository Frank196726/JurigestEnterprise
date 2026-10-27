using Jurigest.Application.Judicial;
using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.BuscarPorRit;

// Todas las causas con ese ROL, una por tribunal, de la más antigua a la más nueva.
// Con CualquierLetra incluye las del mismo número y año con otra letra (C-2580-26 y E-2580-26).
public sealed record BuscarCausasPorRitQuery(
    string Rit,
    FiltroReceptores? Receptores = null,
    bool CualquierLetra = false)
    : IRequest<List<BuscarCausaPorRitResponse>>;

public sealed class BuscarCausasPorRitHandler(ICausaRepository repository)
    : IRequestHandler<BuscarCausasPorRitQuery, List<BuscarCausaPorRitResponse>>
{
    public async Task<List<BuscarCausaPorRitResponse>> Handle(
        BuscarCausasPorRitQuery request,
        CancellationToken cancellationToken)
    {
        var causas = request.CualquierLetra
            ? await repository.GetAllByNumeroRolAsync(request.Rit, cancellationToken)
            : await repository.GetAllByRitAsync(request.Rit, cancellationToken);

        return causas
            .Where(x => BuscarCausaPorRitMapper.EsVisiblePara(x, request.Receptores))
            .Select(BuscarCausaPorRitMapper.Mapear)
            .ToList();
    }
}
