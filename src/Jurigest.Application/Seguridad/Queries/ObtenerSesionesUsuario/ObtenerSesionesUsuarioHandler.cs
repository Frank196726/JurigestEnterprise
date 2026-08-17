using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Seguridad.Queries.ObtenerSesionesUsuario;

public sealed class ObtenerSesionesUsuarioHandler
    : IRequestHandler<
        ObtenerSesionesUsuarioQuery,
        List<ObtenerSesionUsuarioResponse>>
{
    private readonly ISesionUsuarioRepository
        _sesionRepository;

    public ObtenerSesionesUsuarioHandler(
        ISesionUsuarioRepository sesionRepository)
    {
        _sesionRepository = sesionRepository;
    }

    public async Task<List<ObtenerSesionUsuarioResponse>> Handle(
        ObtenerSesionesUsuarioQuery request,
        CancellationToken cancellationToken)
    {
        var sesiones =
            await _sesionRepository
                .GetActiveByUsuarioIdAsync(
                    request.UsuarioId,
                    DateTime.UtcNow,
                    cancellationToken);

        var sesionActual = sesiones.FirstOrDefault(
            sesion =>
                sesion.Id == request.SesionActualId);

        if (sesionActual is null)
        {
            return [];
        }

        return sesiones
            .Where(sesion =>
                sesion.VersionSeguridad ==
                sesionActual.VersionSeguridad)
            .Select(sesion =>
                new ObtenerSesionUsuarioResponse(
                    sesion.Id,
                    sesion.FechaCreacionUtc,
                    sesion.ExpiraUtc,
                    sesion.DireccionIp,
                    sesion.UserAgent,
                    sesion.Id == request.SesionActualId))
            .ToList();
    }
}