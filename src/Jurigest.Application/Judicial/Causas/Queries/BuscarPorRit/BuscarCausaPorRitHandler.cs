using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.BuscarPorRit;

public sealed class BuscarCausaPorRitHandler
    : IRequestHandler<
        BuscarCausaPorRitQuery,
        BuscarCausaPorRitResponse?>
{
    private readonly ICausaRepository _repository;

    public BuscarCausaPorRitHandler(
        ICausaRepository repository)
    {
        _repository = repository;
    }

    public async Task<BuscarCausaPorRitResponse?> Handle(
        BuscarCausaPorRitQuery request,
        CancellationToken cancellationToken)
    {
        // La más antigua visible; si el ROL está en varios tribunales, el receptor
        // ve la causa donde tiene diligencias aunque no sea la primera.
        var causas = await _repository.GetAllByRitAsync(
            request.Rit,
            cancellationToken);

        var causa = causas.FirstOrDefault(x =>
            BuscarCausaPorRitMapper.EsVisiblePara(x, request.ReceptorAsignado));

        return causa is null
            ? null
            : BuscarCausaPorRitMapper.Mapear(causa);
    }
}
