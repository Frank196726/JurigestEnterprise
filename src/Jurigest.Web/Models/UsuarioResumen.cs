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
    DateTime FechaCreacion);
public sealed record UsuariosResponse(List<UsuarioResumen> Value, int Count);
