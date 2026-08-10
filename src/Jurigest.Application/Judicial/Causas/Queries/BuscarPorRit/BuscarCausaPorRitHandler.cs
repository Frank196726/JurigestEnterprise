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
        var causa = await _repository.GetByRitAsync(
            request.Rit,
            cancellationToken);

        if (causa is null)
            return null;

        return new BuscarCausaPorRitResponse(
            causa.Id,
            causa.Rit,
            causa.Tribunal,
            causa.Descripcion,
            causa.FechaCreacion);
    }
}