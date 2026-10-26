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
        if (Enum.GetNames<RolUsuario>().Any(x => x.Equals(nombre.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Ese nombre corresponde a un rol del sistema.");
        ValidarPerfil(perfil);
        Nombre = nombre.Trim();
        NombreNormalizado = Nombre.ToUpperInvariant();
        Perfil = perfil;
    }

    /// <summary>
    /// Cambia el perfil base del rol. Los usuarios del rol deben reasignarse para tomar el nuevo perfil.
    /// </summary>
    public void CambiarPerfil(RolUsuario perfil)
    {
        ValidarPerfil(perfil);
        Perfil = perfil;
    }

    private static void ValidarPerfil(RolUsuario perfil)
    {
        if (!Enum.IsDefined(perfil)) throw new ArgumentException("Seleccione un perfil de acceso válido.");
        if (perfil == RolUsuario.Administrador) throw new ArgumentException("El acceso de administrador está reservado al rol Administrador.");
    }

    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;
    public RolUsuario Perfil { get; private set; }
}

