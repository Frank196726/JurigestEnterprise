using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Catalogos;

public sealed class DiligenciaRealizadaCatalogo : Entity<Guid>
{
    private DiligenciaRealizadaCatalogo()
    {
    }

    public DiligenciaRealizadaCatalogo(
        Guid id,
        string nombre,
        int codigoTipoDiligencia)
        : base(id)
    {
        CambiarNombre(nombre);
        CambiarTipo(codigoTipoDiligencia);

        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public decimal? Arancel { get; private set; }

    public string Nombre { get; private set; } =
        string.Empty;

    public int CodigoTipoDiligencia { get; private set; }

    public bool Activo { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre de la diligencia realizada es obligatorio.",
                nameof(nombre));
        }

        if (nombre.Trim().Length > 200)
        {
            throw new ArgumentException(
                "El nombre de la diligencia realizada no puede superar 200 caracteres.",
                nameof(nombre));
        }

        Nombre = nombre.Trim();
    }

    public void CambiarTipo(int codigoTipoDiligencia)
    {
        if (codigoTipoDiligencia <= 0)
        {
            throw new ArgumentException(
                "El tipo de diligencia es obligatorio.",
                nameof(codigoTipoDiligencia));
        }

        CodigoTipoDiligencia =
            codigoTipoDiligencia;
    }

    public void Activar() =>
        Activo = true;

    public void CambiarArancel(decimal? arancel)
    {
        if (arancel.HasValue && arancel.Value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(arancel),
                "El arancel no puede ser negativo.");
        }

        Arancel = arancel;
    }

    public void Desactivar() =>
        Activo = false;
}
