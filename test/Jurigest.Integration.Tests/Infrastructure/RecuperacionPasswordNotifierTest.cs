using System.Collections.Concurrent;
using Jurigest.Application.Abstractions.Notifications;

namespace Jurigest.Integration.Tests.Infrastructure;

public sealed class RecuperacionPasswordNotifierTest
    : IRecuperacionPasswordNotifier
{
    private readonly ConcurrentDictionary<string, NotificacionRecuperacion>
        _notificaciones =
            new(StringComparer.OrdinalIgnoreCase);

    public Task EnviarAsync(
        string emailDestino,
        string token,
        DateTime expiraUtc,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var emailNormalizado =
            emailDestino.Trim().ToLowerInvariant();

        _notificaciones[emailNormalizado] =
            new NotificacionRecuperacion(
                emailNormalizado,
                token,
                expiraUtc);

        return Task.CompletedTask;
    }

    public NotificacionRecuperacion? ObtenerUltima(
        string email)
    {
        var emailNormalizado =
            email.Trim().ToLowerInvariant();

        return _notificaciones.TryGetValue(
            emailNormalizado,
            out var notificacion)
            ? notificacion
            : null;
    }
}

public sealed record NotificacionRecuperacion(
    string Email,
    string Token,
    DateTime ExpiraUtc);