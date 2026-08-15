namespace Jurigest.Application.Seguridad.Queries.ObtenerAuditoriasSeguridad;

public sealed record ObtenerAuditoriaSeguridadResponse(
    Guid Id,
    Guid? UsuarioActorId,
    string Accion,
    Guid? UsuarioAfectadoId,
    string? Detalle,
    string? DireccionIp,
    DateTime FechaUtc);