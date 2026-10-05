namespace Jurigest.API.Contracts;

public sealed class CambiarPasswordInicialRequest
{
    public string NuevaPassword { get; set; } = string.Empty;
}
