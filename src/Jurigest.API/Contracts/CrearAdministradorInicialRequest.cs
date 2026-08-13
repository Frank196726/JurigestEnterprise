namespace Jurigest.API.Contracts;

public sealed class CrearAdministradorInicialRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}