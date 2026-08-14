namespace Jurigest.API.Contracts;

public sealed class RestablecerPasswordRequest
{
    public string Email { get; set; } = string.Empty;

    public string NuevaPassword { get; set; } = string.Empty;
}