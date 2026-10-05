namespace Jurigest.Application.Seguridad.Queries.ObtenerUsuarios;

public sealed record ObtenerUsuariosResponse(
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
