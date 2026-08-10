namespace Jurigest.API.Contracts;

public sealed class AsignarUbicacionRequest
{
    public string Direccion { get; set; } = string.Empty;

    public string Comuna { get; set; } = string.Empty;
}
