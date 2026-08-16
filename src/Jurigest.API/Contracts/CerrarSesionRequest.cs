namespace Jurigest.API.Contracts;

public sealed class CerrarSesionRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}