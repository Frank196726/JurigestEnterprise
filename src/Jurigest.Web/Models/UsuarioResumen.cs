namespace Jurigest.Web.Models;

public sealed record UsuarioResumen(
    Guid Id,
    string Nombre,
    string Email,
    string? Rut,
    string? Telefono,
    string? Direccion,
    string? NumeroOficina,
    string Rol,
    bool Activo,
    DateTime FechaCreacion,
    List<ReceptorAsignado>? Receptores);
public sealed record ReceptorAsignado(Guid Id, string Nombre);
public sealed record UsuariosResponse(List<UsuarioResumen> Value, int Count);
