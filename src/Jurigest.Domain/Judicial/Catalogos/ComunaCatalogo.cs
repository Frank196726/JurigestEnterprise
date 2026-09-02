using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Catalogos;

public sealed class ComunaCatalogo : Entity<Guid>
{
    private ComunaCatalogo()
    {
    }

    public ComunaCatalogo(
        Guid id,
        string nombre)
        : base(id)
    {
        CambiarNombre(nombre);

        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public string Nombre { get; private set; } = string.Empty;

    public bool Activo { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException(
                "El nombre de la comuna es obligatorio.",
                nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}