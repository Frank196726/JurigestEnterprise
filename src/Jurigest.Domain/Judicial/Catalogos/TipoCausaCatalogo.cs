using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Catalogos;

public sealed class TipoCausaCatalogo : Entity<Guid>
{
    private TipoCausaCatalogo()
    {
    }

    public TipoCausaCatalogo(
        Guid id,
        string nombre,
        string codigo)
        : base(id)
    {
        CambiarNombre(nombre);
        CambiarCodigo(codigo);

        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public string Nombre { get; private set; } =
        string.Empty;

    public string Codigo { get; private set; } =
        string.Empty;

    public bool Activo { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre del tipo de causa es obligatorio.",
                nameof(nombre));
        }

        Nombre = nombre.Trim();
    }

    public void CambiarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código del tipo de causa es obligatorio.",
                nameof(codigo));
        }

        var codigoNormalizado =
            codigo.Trim().ToUpperInvariant();

        if (codigoNormalizado.Length > 10)
        {
            throw new ArgumentException(
                "El código del tipo de causa no puede superar 10 caracteres.",
                nameof(codigo));
        }

        Codigo = codigoNormalizado;
    }

    public void Activar()
    {
        Activo = true;
    }

    public void Desactivar()
    {
        Activo = false;
    }
}