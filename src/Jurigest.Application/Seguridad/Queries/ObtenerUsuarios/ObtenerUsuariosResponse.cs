namespace Jurigest.Application.Seguridad.Queries.ObtenerUsuarios;

public sealed record ObtenerUsuariosResponse(
    Guid Id,
    string Nombre,
    string Email,
    string Rol,
    bool Activo,
    DateTime FechaCreacion);