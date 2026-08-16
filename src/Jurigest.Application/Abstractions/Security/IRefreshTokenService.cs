namespace Jurigest.Application.Abstractions.Security;

public interface IRefreshTokenService
{
    string GenerarToken();

    string CalcularHash(string token);
}