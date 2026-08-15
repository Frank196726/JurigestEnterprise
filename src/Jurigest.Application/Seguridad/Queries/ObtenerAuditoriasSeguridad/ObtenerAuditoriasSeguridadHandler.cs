using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Seguridad.Queries.ObtenerAuditoriasSeguridad;

public sealed class ObtenerAuditoriasSeguridadHandler
    : IRequestHandler<
        ObtenerAuditoriasSeguridadQuery,
        List<ObtenerAuditoriaSeguridadResponse>>
{
    private readonly IAuditoriaSeguridadRepository _repository;

    public ObtenerAuditoriasSeguridadHandler(
        IAuditoriaSeguridadRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ObtenerAuditoriaSeguridadResponse>> Handle(
        ObtenerAuditoriasSeguridadQuery request,
        CancellationToken cancellationToken)
    {
        var auditorias = await _repository.GetLatestAsync(
            request.Cantidad,
            cancellationToken);

        return auditorias
            .Select(auditoria =>
                new ObtenerAuditoriaSeguridadResponse(
                    auditoria.Id,
                    auditoria.UsuarioActorId,
                    auditoria.Accion,
                    auditoria.UsuarioAfectadoId,
                    auditoria.Detalle,
                    auditoria.DireccionIp,
                    auditoria.FechaUtc))
            .ToList();
    }
}