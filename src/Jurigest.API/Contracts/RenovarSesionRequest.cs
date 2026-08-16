namespace Jurigest.API.Contracts;

public sealed class RenovarSesionRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}