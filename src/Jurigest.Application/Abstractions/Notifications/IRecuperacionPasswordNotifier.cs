namespace Jurigest.Application.Abstractions.Notifications;

public interface IRecuperacionPasswordNotifier
{
    Task EnviarAsync(
        string emailDestino,
        string token,
        DateTime expiraUtc,
        CancellationToken cancellationToken);
}