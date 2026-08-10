using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligencia;

public sealed class ObtenerDiligenciaHandler
    : IRequestHandler<ObtenerDiligenciaQuery, ObtenerDiligenciaResponse?>
{
    private readonly IDiligenciaRepository _repository;

    public ObtenerDiligenciaHandler(IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ObtenerDiligenciaResponse?> Handle(
        ObtenerDiligenciaQuery request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return null;

    return new ObtenerDiligenciaResponse(
        diligencia.Id,
        diligencia.CausaId,
        diligencia.Descripcion,
        diligencia.Tipo,
        diligencia.Estado,
        diligencia.FechaCreacion,
        diligencia.FechaProgramada,
        diligencia.FechaRealizada,
        diligencia.ReceptorJudicial,
        diligencia.Direccion,
        diligencia.Comuna,
        diligencia.Observaciones,
        diligencia.Latitud,
        diligencia.Longitud);

    }
}