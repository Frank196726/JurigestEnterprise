using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligenciasPorCausa;

public sealed class ObtenerDiligenciasPorCausaHandler
    : IRequestHandler<
        ObtenerDiligenciasPorCausaQuery,
        List<ObtenerDiligenciasPorCausaResponse>>
{
    private readonly IDiligenciaRepository _repository;

    public ObtenerDiligenciasPorCausaHandler(
        IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ObtenerDiligenciasPorCausaResponse>> Handle(
        ObtenerDiligenciasPorCausaQuery request,
        CancellationToken cancellationToken)
    {
        var diligencias = await _repository.GetByCausaIdAsync(
            request.CausaId,
            cancellationToken);

        return diligencias
            .Select(d => new ObtenerDiligenciasPorCausaResponse(
                d.Id,
                d.Descripcion,
                d.Estado,
                d.Tipo,
                d.FechaCreacion,
                d.FechaProgramada,
                d.FechaRealizada,
                d.ReceptorJudicial,
                d.Direccion,
                d.Comuna))
            .ToList();
    }
}