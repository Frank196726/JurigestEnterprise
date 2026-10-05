namespace Jurigest.Web.Models;

public sealed class DemandadoGestionModel
{
    public string Nombre { get; set; } = string.Empty;
    public int TipoPersona { get; set; } = 1;
    public string Rut { get; set; } = string.Empty;
    public string RepresentanteLegal { get; set; } = string.Empty;
    public string RutRepresentanteLegal { get; set; } = string.Empty;
}
