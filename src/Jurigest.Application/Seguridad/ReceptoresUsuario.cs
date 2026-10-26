using Jurigest.Application.Abstractions.Persistence;

namespace Jurigest.Application.Seguridad;

/// <summary>
/// Reglas para los receptores judiciales asociados a un usuario.
/// </summary>
public static class ReceptoresUsuario
{
    /// <summary>
    /// Valida que se indique al menos un receptor y que todos existan y estén activos.
    /// Devuelve los identificadores sin duplicados.
    /// </summary>
    public static async Task<List<Guid>> ValidarAsync(
        IEnumerable<Guid>? receptorIds,
        IReceptorJudicialCatalogoRepository receptores,
        CancellationToken cancellationToken)
    {
        var ids = (receptorIds ?? [])
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            throw new ArgumentException("Debe asociar el usuario a al menos un receptor.");

        var activos = (await receptores.GetActivosAsync(cancellationToken))
            .Select(x => x.Id)
            .ToHashSet();

        if (ids.Any(id => !activos.Contains(id)))
            throw new ArgumentException("Uno o más receptores seleccionados no existen o están inactivos.");

        return ids;
    }
}
