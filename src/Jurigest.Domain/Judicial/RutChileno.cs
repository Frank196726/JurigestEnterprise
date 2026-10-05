namespace Jurigest.Domain.Judicial;

public static class RutChileno
{
    public static string? Normalizar(string? rut)
    {
        if (string.IsNullOrWhiteSpace(rut)) return null;
        var limpio = new string(rut.Where(c => c != '.' && !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        var partes = limpio.Split('-');
        if (partes.Length != 2 || partes[0].Length is < 1 or > 8 ||
            !partes[0].All(char.IsAsciiDigit) || partes[1].Length != 1 ||
            !(char.IsAsciiDigit(partes[1][0]) || partes[1][0] == 'K') ||
            !int.TryParse(partes[0], out var numero) || numero == 0)
            throw new ArgumentException("El RUT debe tener formato y dígito verificador válidos.", nameof(rut));

        var factor = 2;
        var suma = 0;
        foreach (var digito in partes[0].Reverse())
        {
            suma += (digito - '0') * factor;
            factor = factor == 7 ? 2 : factor + 1;
        }
        var resto = 11 - suma % 11;
        var verificador = resto switch { 11 => '0', 10 => 'K', _ => (char)('0' + resto) };
        if (partes[1][0] != verificador)
            throw new ArgumentException("El dígito verificador del RUT no es válido.", nameof(rut));
        return $"{numero}-{verificador}";
    }
}
