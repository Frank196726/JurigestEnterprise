namespace Jurigest.Persistence.Notifications;

public sealed class SmtpOptions
{
    public const string SectionName = "Email:Smtp";

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 587;

    public bool UseStartTls { get; init; } = true;

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string FromEmail { get; init; } = string.Empty;

    public string FromName { get; init; } = "Jurigest Enterprise";

    public string RecoveryUrl { get; init; } = string.Empty;
}