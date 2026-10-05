using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Domain.Judicial;

internal static class IdentificacionParte
{
    internal static (string? Rut, string? RepresentanteLegal, string? RutRepresentanteLegal) Normalizar(
        TipoPersonaDemandada tipoPersona, string? rut, string? representanteLegal, string? rutRepresentanteLegal)
    {
        var nombreRepresentante = string.IsNullOrWhiteSpace(representanteLegal) ? null : representanteLegal.Trim();
        if (nombreRepresentante?.Length > 200)
            throw new ArgumentException("El representante legal no puede superar 200 caracteres.", nameof(representanteLegal));
        if (tipoPersona == TipoPersonaDemandada.Natural &&
            (nombreRepresentante is not null || !string.IsNullOrWhiteSpace(rutRepresentanteLegal)))
            throw new ArgumentException("Una persona natural no registra representante legal.");
        if (nombreRepresentante is null && !string.IsNullOrWhiteSpace(rutRepresentanteLegal))
            throw new ArgumentException("Indique el nombre del representante legal antes de su RUT.");
        return (RutChileno.Normalizar(rut), nombreRepresentante, RutChileno.Normalizar(rutRepresentanteLegal));
    }
}
