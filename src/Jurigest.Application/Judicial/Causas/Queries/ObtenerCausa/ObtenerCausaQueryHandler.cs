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
            causa.PrimeraDiligencia?.FechaProgramada);
    }
}
