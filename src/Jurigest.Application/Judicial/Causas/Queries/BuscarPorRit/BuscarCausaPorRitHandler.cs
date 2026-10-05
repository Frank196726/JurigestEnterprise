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

        if (!string.IsNullOrWhiteSpace(request.ReceptorAsignado) &&
            !causa.Diligencias.Any(d => string.Equals(
                d.ReceptorJudicial?.Trim(),
                request.ReceptorAsignado.Trim(),
                StringComparison.OrdinalIgnoreCase)))
            return null;

        var partes = causa.Descripcion.Split('/', 2, StringSplitOptions.TrimEntries);
        var demandante = partes.Length == 2 ? partes[0] : null;
        var demandadoCaratula = partes.Length == 2 ? partes[1] : null;
        var demandadoPrincipal = causa.Demandados.FirstOrDefault(x => x.EsPrincipal)?.Nombre;
        var diligenciaConDireccion = causa.Diligencias
            .Where(x => !string.IsNullOrWhiteSpace(x.Direccion) && !string.IsNullOrWhiteSpace(x.Comuna))
            .OrderByDescending(x => x.FechaCreacion)
            .FirstOrDefault();

        return new BuscarCausaPorRitResponse(
            causa.Id,
            causa.Rit,
            causa.Tribunal,
            causa.Descripcion,
            causa.FechaCreacion)
        {
            NombreDemandado = demandadoPrincipal ?? demandadoCaratula,
            Direccion = diligenciaConDireccion?.Direccion,
            Comuna = diligenciaConDireccion?.Comuna,
            Demandante = demandante,
            Demandado = demandadoPrincipal ?? demandadoCaratula
        };
    }
}
