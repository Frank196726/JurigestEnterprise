using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Catalogos;

public sealed class AbogadoCatalogo : Entity<Guid>
{
    private AbogadoCatalogo()
    {
    }

    public AbogadoCatalogo(
        Guid id,
        string nombre)
        : base(id)
    {
        CambiarNombre(nombre);

        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public string Nombre { get; private set; } =
        string.Empty;

    public Guid? UsuarioId { get; private set; }

    public bool Activo { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre del abogado es obligatorio.",
                nameof(nombre));
        }

        Nombre = nombre.Trim();
    }

    public void AsociarUsuario(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ArgumentException(
                "El usuario no es válido.",
                nameof(usuarioId));
        }

        UsuarioId = usuarioId;
    }

    public void Activar() =>
        Activo = true;

    public void Desactivar() =>
        Activo = false;
}