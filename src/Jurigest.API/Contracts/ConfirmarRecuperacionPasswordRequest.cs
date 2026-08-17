namespace Jurigest.API.Contracts;

public sealed class ConfirmarRecuperacionPasswordRequest
{
    public string Token { get; init; } = string.Empty;

    public string NuevaPassword { get; init; } = string.Empty;
}