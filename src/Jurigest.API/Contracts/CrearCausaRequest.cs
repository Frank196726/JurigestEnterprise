namespace Jurigest.API.Contracts;

public sealed class CrearCausaRequest
{
    public Guid Id { get; set; }

    public string Rit { get; set; } = string.Empty;

    public string Tribunal { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;
}