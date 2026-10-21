using System.Globalization;
using System.Text.RegularExpressions;

namespace Jurigest.Domain.Judicial;

public static class MontoEstampe
{
    public const decimal Maximo = 999999999999999999m;
    private const string Prefijo = "VALOR DE LA DILIGENCIA: ";

    public static void Validar(decimal monto)
    {
        if (monto < 0 || monto > Maximo || decimal.Truncate(monto) != monto)
            throw new ArgumentException("Ingrese un valor en pesos enteros, entre 0 y 999999999999999999.");
    }

    // Solo honorarios identificados como Drs; no incluye cuantías ni el total final.
    public static decimal? SumarActuaciones(string estampe)
    {
        var importes = Regex.Matches(estampe.Replace("\r\n", "\n"), @"(?im)^[ \t]*Drs\.?[ \t]*\$[ \t]*(\d{1,3}(?:\.\d{3})+|\d+)(?:\.\-|-)?[ \t]*$");
        if (importes.Count == 0) return null;
        decimal total = 0;
        foreach (Match importe in importes)
        {
            if (!decimal.TryParse(importe.Groups[1].Value.Replace(".", ""), NumberStyles.None,
                CultureInfo.InvariantCulture, out var valor))
                throw new ArgumentException("El valor de una actuación no es válido.");
            Validar(valor);
            total += valor;
            Validar(total);
        }
        return total;
    }

    public static string Aplicar(string estampe, decimal monto)
    {
        Validar(monto);
        ArgumentException.ThrowIfNullOrWhiteSpace(estampe);
        var lineas = estampe.Replace("\r\n", "\n").Split('\n')
            .Where(x => !x.TrimStart().StartsWith(Prefijo, StringComparison.Ordinal));
        return string.Join(Environment.NewLine, lineas).TrimEnd() + Environment.NewLine + Environment.NewLine +
            Prefijo + "$" + monto.ToString("N0", CultureInfo.GetCultureInfo("es-CL")) + ".-";
    }
}
