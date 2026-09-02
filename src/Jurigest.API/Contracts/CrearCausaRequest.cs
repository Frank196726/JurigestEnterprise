namespace Jurigest.API.Contracts;

public sealed class CrearCausaRequest
{
    public Guid Id { get; set; }

    // Compatibilidad temporal con clientes anteriores.
    public string Rit { get; set; } = string.Empty;

    public Guid? TipoCausaId { get; set; }

    public string NumeroRol { get; set; } = string.Empty;

    public string Tribunal { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public DateTime FechaEncargoCausa { get; set; }
}