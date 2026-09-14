using System.Text.RegularExpressions;

namespace Jurigest.Domain.Judicial;

public static class IdentificacionCausa
{
    public static string NormalizarRol(string? rol)
    {
        var valor = (rol ?? string.Empty).Trim().ToUpperInvariant();
        valor = Regex.Replace(valor, @"\s*-\s*", "-");
        return Regex.IsMatch(valor, @"^\d+-\d+$") ? $"C-{valor}" : valor;
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