namespace Jurigest.API.Contracts;

public sealed class ActualizarCausaRequest
{
    public string Tribunal { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;
}
