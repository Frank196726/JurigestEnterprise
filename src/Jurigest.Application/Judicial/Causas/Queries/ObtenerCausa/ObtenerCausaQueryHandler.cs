using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.ObtenerCausa;

public sealed class ObtenerCausaHandler
    : IRequestHandler<ObtenerCausaQuery, ObtenerCausaResponse?>
{
    private readonly ICausaRepository _repository;

    public ObtenerCausaHandler(ICausaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ObtenerCausaResponse?> Handle(
        ObtenerCausaQuery request,
        CancellationToken cancellationToken)
    {
        var causa = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (causa is null)
            return null;

        if (!string.IsNullOrWhiteSpace(request.ReceptorAsignado) &&
            !causa.Diligencias.Any(d => string.Equals(
                d.ReceptorJudicial?.Trim(),
                request.ReceptorAsignado.Trim(),
                StringComparison.OrdinalIgnoreCase)))
            return null;

        var diasSinGestion = causa.ObtenerDiasSinGestion(
            DateTime.UtcNow.Date);

        return new ObtenerCausaResponse(
            causa.Id,
            causa.Rit,
            causa.Tribunal,
            causa.Descripcion,
            causa.FechaCreacion,
            causa.FechaEncargoCausa,
            causa.FechaGestionCausa,
            diasSinGestion,
            diasSinGestion > 10,
            (int)causa.Estado,
            causa.PuedeCorregirIngreso,
            causa.PrimeraDiligencia?.Id,
            causa.PrimeraDiligencia?.Descripcion,
            causa.PrimeraDiligencia?.FechaProgramada,
            causa.Demandados.Select(d => new DemandadoResponse(
                d.Id, d.Nombre, (int)d.TipoPersona, d.EsPrincipal,
                d.Avales.Select(a => new AvalSolidarioResponse(a.Id, a.Nombre, (int)a.TipoPersona,
                    a.Rut, a.RepresentanteLegal, a.RutRepresentanteLegal)).ToList(),
                d.Rut, d.RepresentanteLegal, d.RutRepresentanteLegal
            )).ToList())
        {
            Materia = causa.Materia,
            ReceptorJudicial = causa.PrimeraDiligencia?.ReceptorJudicial
        };
    }
}
