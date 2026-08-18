namespace Jurigest.Web.Security;

public sealed record IniciarSesionApiResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    Guid UsuarioId,
    string Nombre,
    string Email,
    string Rol);