using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CerrarSesion;

public sealed class CerrarSesionHandler
    : IRequestHandler<CerrarSesionCommand>
{
    private readonly ISesionUsuarioRepository _sesionRepository;
    private readonly IRefreshTokenService _refreshTokenService;

    public CerrarSesionHandler(
        ISesionUsuarioRepository sesionRepository,
        IRefreshTokenService refreshTokenService)
    {
        _sesionRepository = sesionRepository;
        _refreshTokenService = refreshTokenService;
    }

    public async Task Handle(
        CerrarSesionCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return;

        var refreshTokenHash =
            _refreshTokenService.CalcularHash(
                request.RefreshToken);

        var sesion =
            await _sesionRepository
                .GetByRefreshTokenHashAsync(
                    refreshTokenHash,
                    cancellationToken);

        if (sesion is null ||
            sesion.RevocadaUtc.HasValue)
        {
            return;
        }

        sesion.Revocar(DateTime.UtcNow);

        await _sesionRepository.UpdateAsync(
            sesion,
            cancellationToken);
    }
}