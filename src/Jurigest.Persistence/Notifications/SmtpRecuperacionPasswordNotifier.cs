using System.Net;
using Jurigest.Application.Abstractions.Notifications;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Jurigest.Persistence.Notifications;

public sealed class SmtpRecuperacionPasswordNotifier
    : IRecuperacionPasswordNotifier
{
    private readonly SmtpOptions _options;
    private readonly IHostEnvironment _environment;

    public SmtpRecuperacionPasswordNotifier(
        IOptions<SmtpOptions> options,
        IHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    public async Task EnviarAsync(
        string emailDestino,
        string token,
        DateTime expiraUtc,
        CancellationToken cancellationToken)
    {
        var separador = _options.RecoveryUrl.Contains(
            '?',
            StringComparison.Ordinal)
            ? "&"
            : "?";

        var enlace =
            $"{_options.RecoveryUrl}{separador}token=" +
            Uri.EscapeDataString(token);

        // SOLO PARA DESARROLLO
        if (_environment.IsDevelopment())
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("RECUPERACION PASSWORD - DEVELOPMENT");
            Console.WriteLine($"Email: {emailDestino}");
            Console.WriteLine($"Token: {token}");
            Console.WriteLine($"Enlace: {enlace}");
            Console.WriteLine($"Expira UTC: {expiraUtc:O}");
            Console.WriteLine("========================================");
            Console.WriteLine();

            return;
        }

        ValidarConfiguracion();

        var mensaje = new MimeMessage();

        mensaje.From.Add(
            new MailboxAddress(
                _options.FromName,
                _options.FromEmail));

        mensaje.To.Add(
            MailboxAddress.Parse(emailDestino));

        mensaje.Subject =
            "Recuperación de contraseña - Jurigest Enterprise";

        var enlaceSeguro = WebUtility.HtmlEncode(enlace);
        var expiracion =
            expiraUtc.ToString("yyyy-MM-dd HH:mm 'UTC'");

        mensaje.Body = new BodyBuilder
        {
            TextBody =
                "Se solicitó recuperar la contraseña de su cuenta. " +
                $"Abra el siguiente enlace: {enlace}. " +
                $"El enlace expira el {expiracion}. " +
                "Si usted no realizó esta solicitud, ignore este mensaje.",

            HtmlBody = $"""
                <h2>Recuperación de contraseña</h2>
                <p>
                    Se solicitó recuperar la contraseña de su cuenta
                    de Jurigest Enterprise.
                </p>
                <p>
                    <a href="{enlaceSeguro}">
                        Restablecer contraseña
                    </a>
                </p>
                <p>El enlace expira el {expiracion}.</p>
                <p>
                    Si usted no realizó esta solicitud,
                    ignore este mensaje.
                </p>
                """
        }.ToMessageBody();

        using var cliente = new SmtpClient();

        var seguridad = _options.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await cliente.ConnectAsync(
            _options.Host,
            _options.Port,
            seguridad,
            cancellationToken);

        await cliente.AuthenticateAsync(
            _options.Username,
            _options.Password,
            cancellationToken);

        await cliente.SendAsync(
            mensaje,
            cancellationToken);

        await cliente.DisconnectAsync(
            true,
            cancellationToken);
    }

    private void ValidarConfiguracion()
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
            throw new InvalidOperationException(
                "Falta la configuración Email:Smtp:Host.");

        if (_options.Port is < 1 or > 65535)
            throw new InvalidOperationException(
                "Email:Smtp:Port no es válido.");

        if (string.IsNullOrWhiteSpace(_options.Username))
            throw new InvalidOperationException(
                "Falta la configuración Email:Smtp:Username.");

        if (string.IsNullOrWhiteSpace(_options.Password))
            throw new InvalidOperationException(
                "Falta la configuración Email:Smtp:Password.");

        if (string.IsNullOrWhiteSpace(_options.FromEmail))
            throw new InvalidOperationException(
                "Falta la configuración Email:Smtp:FromEmail.");

        if (!Uri.TryCreate(
                _options.RecoveryUrl,
                UriKind.Absolute,
                out var recoveryUri) ||
            recoveryUri.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException(
                "Email:Smtp:RecoveryUrl no es una URL válida.");
        }
    }
}