using System.Text.RegularExpressions;

namespace Jurigest.Domain.Judicial;

public static class IdentificacionCausa
{
    public const string MensajeCausaDuplicada = "Ya existe una causa con ese ROL en el mismo tribunal.";

    public static string NormalizarRol(string? rol)
    {
        var valor = (rol ?? string.Empty).Trim().ToUpperInvariant();
        valor = Regex.Replace(valor, @"\s*-\s*", "-");
        return Regex.IsMatch(valor, @"^\d+-\d+$") ? $"C-{valor}" : valor;
    }

    // Número y año del ROL sin la letra del tipo de causa: "C-2580-26" y "E-2580-26" -> "2580-26".
    public static string? NumeroRol(string? rol)
    {
        var coincidencia = Regex.Match(NormalizarRol(rol), @"^[A-Z]+-(\d+-\d+)$");
        return coincidencia.Success ? coincidencia.Groups[1].Value : null;
    }

    public static string NormalizarTribunal(string? tribunal)
    {
        var valor = Regex.Replace((tribunal ?? string.Empty).Trim(), @"\s+", " ");
        return Regex.Replace(valor, @"^(\d+)\s*[°º]?(?=\s|$)",
            match => match.Groups[1].Value.TrimStart('0') is { Length: > 0 } numero
                ? $"{numero}°" : "0°");
    }

    public static bool MismoTribunal(string? primero, string? segundo) =>
        string.Equals(NormalizarTribunal(primero), NormalizarTribunal(segundo),
            StringComparison.OrdinalIgnoreCase);

    public static bool MismoRol(string? primero, string? segundo) =>
        string.Equals(NormalizarRol(primero), NormalizarRol(segundo), StringComparison.Ordinal);
}