using Jurigest.Domain.Seguridad.Enums;

namespace Jurigest.Domain.Seguridad;

public static class PermisosSistema
{
    public sealed record Definicion(string Codigo, string Nombre, string Grupo, string? Requiere = null, bool ExclusivoAdministrador = false);
    public static readonly Definicion[] Catalogo =
    [
        new("CausasLectura", "Consultar causas, panel y reportes", "Causas"),
        new("CausasEscritura", "Crear y modificar causas", "Causas", "CausasLectura"),
        new("CausasEliminacion", "Eliminar causas", "Causas", "CausasLectura"),
        new("DiligenciasLectura", "Consultar diligencias, recibos y estampes", "Diligencias"),
        new("DiligenciasGestion", "Gestionar diligencias, pagos y estampes", "Diligencias", "DiligenciasLectura"),
        new("DocumentosLectura", "Consultar y descargar documentos", "Documentos"),
        new("DocumentosCarga", "Cargar documentos", "Documentos", "DocumentosLectura"),
        new("DocumentosEliminacion", "Eliminar documentos", "Documentos", "DocumentosLectura"),
        new("ResolucionesLectura", "Consultar resoluciones", "Resoluciones"),
        new("ResolucionesRegistro", "Registrar resoluciones", "Resoluciones", "ResolucionesLectura"),
        new("ResolucionesEliminacion", "Eliminar resoluciones", "Resoluciones", "ResolucionesLectura"),
        new("Administracion", "Administrar usuarios, roles, permisos, auditoría y sistema", "Administración", null, true)
    ];

    public static string[] Predeterminados(RolUsuario perfil) => Catalogo.Where(x =>
        perfil == RolUsuario.Administrador ||
        x.Codigo.EndsWith("Lectura") ||
        (perfil is RolUsuario.Abogado or RolUsuario.Procurador && x.Codigo is "DiligenciasGestion" or "DocumentosCarga" or "ResolucionesRegistro") ||
        (perfil == RolUsuario.Abogado && x.Codigo == "CausasEscritura"))
        .Select(x => x.Codigo).ToArray();

    public static void Validar(string[] permisos)
    {
        if (permisos.Any(p => !Catalogo.Any(x => x.Codigo == p && !x.ExclusivoAdministrador)))
            throw new ArgumentException("La selección contiene permisos desconocidos o exclusivos del administrador.");
        foreach (var item in Catalogo.Where(x => permisos.Contains(x.Codigo)))
            if (item.Requiere is not null && !permisos.Contains(item.Requiere))
                throw new ArgumentException($"{item.Nombre} requiere el permiso de lectura de su grupo.");
    }
}
