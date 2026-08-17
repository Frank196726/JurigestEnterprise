namespace Jurigest.Application.Seguridad.Queries.ObtenerSesionesUsuario;

public sealed record ObtenerSesionUsuarioResponse(
    Guid Id,
    DateTime FechaCreacionUtc,
    DateTime ExpiraUtc,
    string? DireccionIp,
    string? UserAgent,
    bool EsActual);