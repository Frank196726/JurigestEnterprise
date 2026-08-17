namespace Jurigest.Application.Abstractions.Security;

public interface ITokenRecuperacionPasswordService
{
    string GenerarToken();

    string CalcularHash(string token);
}