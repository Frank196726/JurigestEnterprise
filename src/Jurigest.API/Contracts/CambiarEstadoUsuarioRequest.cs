namespace Jurigest.API.Contracts;

public sealed class CambiarEstadoUsuarioRequest
{
    public bool Activo { get; init; }
}