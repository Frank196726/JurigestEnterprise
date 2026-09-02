using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Catalogos;

public sealed class DiligenciaEncargadaCatalogo : Entity<Guid>
{
    private DiligenciaEncargadaCatalogo()
    {
    }

    public DiligenciaEncargadaCatalogo(
        Guid id,
        string nombre,
        int? codigoTipoDiligencia = null)
        : base(id)
    {
        CambiarNombre(nombre);

        CodigoTipoDiligencia =
            codigoTipoDiligencia;

        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public string Nombre { get; private set; } =
        string.Empty;

    public int? CodigoTipoDiligencia { get; private set; }

    public bool Activo { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre de la diligencia encargada es obligatorio.",
                nameof(nombre));
        }

        Nombre = nombre.Trim();
    }

    public void CambiarTipo(
        int? codigoTipoDiligencia)
    {
        CodigoTipoDiligencia =
            codigoTipoDiligencia;
    }

    public void Activar() =>
        Activo = true;

    public void Desactivar() =>
        Activo = false;
}