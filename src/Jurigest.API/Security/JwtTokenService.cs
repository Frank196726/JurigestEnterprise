using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Domain.Seguridad.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Jurigest.API.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenResult CrearToken(
        Usuario usuario,
        Guid sesionId)
    {
        if (sesionId == Guid.Empty)
        {
            throw new ArgumentException(
                "La sesion es obligatoria.",
                nameof(sesionId));
        }

        var issuer =
            ObtenerConfiguracion("Jwt:Issuer");

        var audience =
            ObtenerConfiguracion("Jwt:Audience");

        var key =
            ObtenerConfiguracion("Jwt:Key");

        var expirationMinutes =
            _configuration.GetValue<int>(
                "Jwt:ExpirationMinutes");

        if (expirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "La expiracion JWT no es valida.");
        }

        var expiresAtUtc =
            DateTime.UtcNow.AddMinutes(
                expirationMinutes);

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                usuario.Id.ToString()),

            new Claim(
                "session_id",
                sesionId.ToString()),

            new Claim(
                "token_version",
                usuario.VersionSeguridad.ToString(
                    CultureInfo.InvariantCulture)),

            new Claim(
                JwtRegisteredClaimNames.Email,
                usuario.Email),

            new Claim(
                ClaimTypes.Name,
                usuario.Nombre),

            new Claim(
                ClaimTypes.Role,
                usuario.Rol.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var signingKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

        var credentials =
            new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new TokenResult(
            new JwtSecurityTokenHandler()
                .WriteToken(token),
            expiresAtUtc);
    }

    private string ObtenerConfiguracion(
        string clave)
    {
        var valor = _configuration[clave];

        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new InvalidOperationException(
                $"Falta la configuracion {clave}.");
        }

        return valor;
    }
}