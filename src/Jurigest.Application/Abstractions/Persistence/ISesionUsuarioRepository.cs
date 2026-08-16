using Jurigest.Domain.Seguridad.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface ISesionUsuarioRepository
{
    Task AddAsync(
        SesionUsuario sesion,
        CancellationToken cancellationToken);

    Task<SesionUsuario?> GetByRefreshTokenHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        SesionUsuario sesion,
        CancellationToken cancellationToken);

    Task RotateAsync(
        SesionUsuario sesionAnterior,
        SesionUsuario sesionNueva,
        CancellationToken cancellationToken);

    Task RevokeAllActiveAsync(
        Guid usuarioId,
        DateTime fechaUtc,
        CancellationToken cancellationToken);
}