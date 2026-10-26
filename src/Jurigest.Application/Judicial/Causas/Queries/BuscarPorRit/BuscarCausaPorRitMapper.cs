using Jurigest.Application.Judicial;
using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Judicial.Causas.Queries.BuscarPorRit;

internal static class BuscarCausaPorRitMapper
{
    // Un usuario solo ve las causas de sus receptores o las aún sin receptor.
    public static bool EsVisiblePara(Causa causa, FiltroReceptores? receptores) =>
        receptores is null || receptores.PermiteCausa(causa.Diligencias);

    public static BuscarCausaPorRitResponse Mapear(Causa causa)
    {
        var partes = causa.Descripcion.Split('/', 2, StringSplitOptions.TrimEntries);
        var demandante = partes.Length == 2 ? partes[0] : null;
        var demandadoCaratula = partes.Length == 2 ? partes[1] : null;
        var demandadoPrincipal = causa.Demandados.FirstOrDefault(x => x.EsPrincipal)?.Nombre;
        var diligenciaConDireccion = causa.Diligencias
            .Where(x => !string.IsNullOrWhiteSpace(x.Direccion) && !string.IsNullOrWhiteSpace(x.Comuna))
            .OrderByDescending(x => x.FechaCreacion)
            .FirstOrDefault();

        return new BuscarCausaPorRitResponse(
            causa.Id,
            causa.Rit,
            causa.Tribunal,
            causa.Descripcion,
            causa.FechaCreacion)
        {
            NombreDemandado = demandadoPrincipal ?? demandadoCaratula,
            Direccion = diligenciaConDireccion?.Direccion,
            Comuna = diligenciaConDireccion?.Comuna,
            Demandante = demandante,
            Demandado = demandadoPrincipal ?? demandadoCaratula
        };
    }
}
