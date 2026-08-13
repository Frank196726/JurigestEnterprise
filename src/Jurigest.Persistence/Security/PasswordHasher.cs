using System.Security.Cryptography;
using Jurigest.Application.Abstractions.Security;

namespace Jurigest.Persistence.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int Iteraciones = 210_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;
    private const string Version = "v1";

    public string Hash(string password)
    {
        ValidarPassword(password);

        var sal = RandomNumberGenerator.GetBytes(TamanoSal);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            sal,
            Iteraciones,
            HashAlgorithmName.SHA256,
            TamanoHash);

        return string.Join(
            '$',
            Version,
            Iteraciones,
            Convert.ToBase64String(sal),
            Convert.ToBase64String(hash));
    }

    public bool Verify(
        string password,
        string passwordHash)
    {
        if (string.IsNullOrEmpty(password) ||
            string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var partes = passwordHash.Split('$');

        if (partes.Length != 4 ||
            partes[0] != Version ||
            !int.TryParse(partes[1], out var iteraciones) ||
            iteraciones <= 0)
        {
            return false;
        }

        try
        {
            var sal = Convert.FromBase64String(partes[2]);
            var hashEsperado = Convert.FromBase64String(partes[3]);

            if (sal.Length != TamanoSal ||
                hashEsperado.Length != TamanoHash)
            {
                return false;
            }

            var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                password,
                sal,
                iteraciones,
                HashAlgorithmName.SHA256,
                hashEsperado.Length);

            return CryptographicOperations.FixedTimeEquals(
                hashCalculado,
                hashEsperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static void ValidarPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            password.Length < 12)
        {
            throw new ArgumentException(
                "La contraseña debe tener al menos 12 caracteres.");
        }

        if (password.Length > 128)
        {
            throw new ArgumentException(
                "La contraseña no puede superar 128 caracteres.");
        }
    }
}