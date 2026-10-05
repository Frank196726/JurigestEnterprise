using Jurigest.Domain.Kernel.Common;
using Jurigest.Domain.Seguridad.Enums;

namespace Jurigest.Domain.Seguridad.Entities;

public sealed class RolCatalogo : Entity<Guid>
{
    private RolCatalogo() { }

    public RolCatalogo(Guid id, string nombre, RolUsuario perfil) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (nombre.Trim().Length > 80) throw new ArgumentException("El nombre del rol admite hasta 80 caracteres.");
        if (!Enum.IsDefined(perfil)) throw new ArgumentException("Seleccione un perfil de acceso válido.");
        if (Enum.GetNames<RolUsuario>().Any(x => x.Equals(nombre.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Ese nombre corresponde a un rol del sistema.");
        if (perfil == RolUsuario.Administrador) throw new ArgumentException("El acceso de administrador está reservado al rol Administrador.");
        Nombre = nombre.Trim();
        NombreNormalizado = Nombre.ToUpperInvariant();
        Perfil = perfil;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;
    public RolUsuario Perfil { get; private set; }
}

