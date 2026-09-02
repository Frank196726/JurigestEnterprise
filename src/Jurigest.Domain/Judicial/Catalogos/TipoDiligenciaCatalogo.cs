using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Catalogos;

public sealed class TipoDiligenciaCatalogo : Entity<Guid>
{
    private TipoDiligenciaCatalogo()
    {
    }

    public TipoDiligenciaCatalogo(
        Guid id,
        string nombre,
        int? codigoSistema = null)
        : base(id)
    {
        CambiarNombre(nombre);

        CodigoSistema = codigoSistema;
        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public string Nombre { get; private set; } = string.Empty;

    public int? CodigoSistema { get; private set; }

    public bool Activo { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException(
                "El nombre del tipo de diligencia es obligatorio.",
                nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}