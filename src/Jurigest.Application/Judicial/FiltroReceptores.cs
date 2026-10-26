using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Judicial;

/// <summary>
/// Restringe lo que ve un usuario a los receptores judiciales que tiene asociados.
/// Un usuario puede estar asociado a varios receptores. Una causa es visible si
/// alguna de sus diligencias está asignada a uno de esos receptores, o si todavía
/// ninguna diligencia tiene receptor asignado (causa pendiente de asignación).
/// Un filtro null significa que el usuario no tiene restricción.
/// </summary>
public sealed class FiltroReceptores
{
    private readonly HashSet<string> _nombres;

    public FiltroReceptores(IEnumerable<string?> nombres)
    {
        _nombres = nombres
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> Nombres => _nombres;

    /// <summary>
    /// Indica si el receptor indicado es uno de los receptores del usuario.
    /// </summary>
    public bool EsPropio(string? receptor) =>
        !string.IsNullOrWhiteSpace(receptor) &&
        _nombres.Contains(receptor.Trim());

    /// <summary>
    /// Visibilidad de una causa a partir de los receptores de todas sus diligencias.
    /// </summary>
    public bool PermiteCausa(IEnumerable<string?> receptoresDeDiligencias)
    {
        var asignados = receptoresDeDiligencias
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        return asignados.Count == 0 || asignados.Any(EsPropio);
    }

    public bool PermiteCausa(IEnumerable<Diligencia> diligencias) =>
        PermiteCausa(diligencias.Select(x => x.ReceptorJudicial));

    /// <summary>
    /// Una diligencia es visible si es de uno de sus receptores, o si aún no tiene
    /// receptor y pertenece a una causa visible.
    /// </summary>
    public bool PermiteDiligencia(string? receptor, bool causaVisible) =>
        EsPropio(receptor) ||
        (string.IsNullOrWhiteSpace(receptor) && causaVisible);

    /// <summary>
    /// Filtra un listado plano de diligencias. Debe recibir todas las diligencias de
    /// cada causa para poder decidir la visibilidad de la causa.
    /// </summary>
    public List<Diligencia> FiltrarDiligencias(IEnumerable<Diligencia> diligencias)
    {
        var lista = diligencias.ToList();
        var causasVisibles = CausasVisibles(lista);

        return lista
            .Where(d => PermiteDiligencia(d.ReceptorJudicial, causasVisibles.Contains(d.CausaId)))
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
