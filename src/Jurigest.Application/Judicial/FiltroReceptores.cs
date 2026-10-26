using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Judicial;

/// <summary>
/// Restringe lo que ve un usuario a los receptores judiciales que tiene asociados.
/// Un usuario puede estar asociado a varios receptores. Una causa es visible si
/// alguna de sus diligencias está asignada a uno de esos receptores, o si todavía
/// ninguna diligencia tiene receptor asignado (causa pendiente de asignación).
/// Un filtro null significa que el usuario no tiene restricción.
/// </summary>
/// <remarks>
/// Las diligencias vinculadas al catálogo se comparan por Id, así renombrar un
/// receptor no cambia la visibilidad. Las que tienen un nombre fuera del catálogo
/// (texto libre o el propio usuario con rol "Receptor") se comparan por nombre.
/// </remarks>
public sealed class FiltroReceptores
{
    private readonly HashSet<Guid> _ids;
    private readonly HashSet<string> _nombres;

    public FiltroReceptores(IEnumerable<Guid> ids, IEnumerable<string?> nombres)
    {
        _ids = ids.ToHashSet();
        _nombres = nombres
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Nombres actuales de los receptores del usuario; son los que puede asignar.
    /// </summary>
    public IReadOnlyCollection<string> Nombres => _nombres;

    /// <summary>
    /// Indica si el nombre corresponde a uno de los receptores del usuario.
    /// </summary>
    public bool EsPropio(string? nombre) =>
        !string.IsNullOrWhiteSpace(nombre) &&
        _nombres.Contains(nombre.Trim());

    /// <summary>
    /// Indica si la asignación de una diligencia es de uno de los receptores del usuario.
    /// </summary>
    public bool EsPropio(Guid? receptorId, string? nombre) =>
        receptorId is Guid id ? _ids.Contains(id) : EsPropio(nombre);

    /// <summary>
    /// Visibilidad de una causa a partir de las asignaciones de todas sus diligencias.
    /// </summary>
    public bool PermiteCausa(IEnumerable<(Guid? Id, string? Nombre)> asignaciones)
    {
        var asignadas = asignaciones
            .Where(x => x.Id.HasValue || !string.IsNullOrWhiteSpace(x.Nombre))
            .ToList();

        return asignadas.Count == 0 || asignadas.Any(x => EsPropio(x.Id, x.Nombre));
    }

    public bool PermiteCausa(IEnumerable<Diligencia> diligencias) =>
        PermiteCausa(diligencias.Select(x => (x.ReceptorJudicialId, x.ReceptorJudicial)));

    /// <summary>
    /// Una diligencia es visible si es de uno de sus receptores, o si aún no tiene
    /// receptor y pertenece a una causa visible.
    /// </summary>
    public bool PermiteDiligencia(Guid? receptorId, string? nombre, bool causaVisible) =>
        EsPropio(receptorId, nombre) ||
        (receptorId is null && string.IsNullOrWhiteSpace(nombre) && causaVisible);

    /// <summary>
    /// Filtra un listado plano de diligencias. Debe recibir todas las diligencias de
    /// cada causa para poder decidir la visibilidad de la causa.
    /// </summary>
    public List<Diligencia> FiltrarDiligencias(IEnumerable<Diligencia> diligencias)
    {
        var lista = diligencias.ToList();
        var causasVisibles = CausasVisibles(lista);

        return lista
            .Where(d => PermiteDiligencia(d.ReceptorJudicialId, d.ReceptorJudicial, causasVisibles.Contains(d.CausaId)))
            .ToList();
    }

    /// <summary>
    /// Filtra causas usando el listado de diligencias. Las causas sin diligencias
    /// no tienen receptor asignado y por lo tanto son visibles.
    /// </summary>
    public List<Causa> FiltrarCausas(IEnumerable<Causa> causas, IEnumerable<Diligencia> diligencias)
    {
        var porCausa = diligencias
            .GroupBy(d => d.CausaId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return causas
            .Where(c => PermiteCausa(porCausa.GetValueOrDefault(c.Id) ?? []))
            .ToList();
    }

    private HashSet<Guid> CausasVisibles(IEnumerable<Diligencia> diligencias) =>
        diligencias
            .GroupBy(d => d.CausaId)
            .Where(PermiteCausa)
            .Select(g => g.Key)
            .ToHashSet();
}
