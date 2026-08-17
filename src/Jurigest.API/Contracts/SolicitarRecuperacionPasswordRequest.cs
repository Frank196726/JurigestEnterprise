namespace Jurigest.API.Contracts;

public sealed class SolicitarRecuperacionPasswordRequest
{
    public string Email { get; init; } = string.Empty;
}