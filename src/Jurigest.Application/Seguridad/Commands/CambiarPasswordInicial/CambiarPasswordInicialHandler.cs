using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CambiarPasswordInicial;

public sealed class CambiarPasswordInicialHandler(
    IUsuarioRepository usuarios,
    IPasswordHasher passwordHasher,
    ISesionUsuarioRepository sesiones,
    IAuditoriaSeguridadRepository auditorias)
    : IRequestHandler<CambiarPasswordInicialCommand, bool>
{
    public async Task<bool> Handle(
        CambiarPasswordInicialCommand request,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarios.GetByIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null || !usuario.Activo)
            return false;

        if (!usuario.DebeCambiarPassword)
            throw new InvalidOperationException("El usuario no tiene un cambio inicial de contraseña pendiente.");

        if (passwordHasher.Verify(request.NuevaPassword, usuario.PasswordHash))
            throw new ArgumentException("La nueva contraseña debe ser distinta de la contraseña temporal.");

        usuario.CambiarPasswordHash(passwordHasher.Hash(request.NuevaPassword));
        await usuarios.UpdateAsync(usuario, cancellationToken);
        await sesiones.RevokeAllActiveAsync(usuario.Id, DateTime.UtcNow, cancellationToken);

        await auditorias.AddAsync(
            new AuditoriaSeguridad(
                Guid.NewGuid(),
                usuario.Id,
                "PasswordInicialCambiada",
                usuario.Id,
                "El usuario cambió su contraseña temporal en el primer ingreso.",
                request.DireccionIp),
            cancellationToken);

        return true;
    }
}
