namespace Jurigest.Domain.Seguridad.Entities;

public sealed class PermisosRol
{
    public string Clave { get; set; } = string.Empty;
    public string SeleccionJson { get; set; } = "[]";
    public Guid Version { get; set; }
}
