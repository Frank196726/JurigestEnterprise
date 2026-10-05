using System.Globalization;

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
