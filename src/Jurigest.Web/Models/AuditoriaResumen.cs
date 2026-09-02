namespace Jurigest.Web.Models;

public sealed record AuditoriaResumen(Guid Id, Guid? UsuarioActorId, string Accion, Guid? UsuarioAfectadoId, string? Detalle, string? DireccionIp, DateTime FechaUtc);
public sealed record AuditoriasResponse(List<AuditoriaResumen> Value, int Count);
