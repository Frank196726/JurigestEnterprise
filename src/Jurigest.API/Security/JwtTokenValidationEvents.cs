using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jurigest.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Jurigest.API.Security;

public sealed class JwtTokenValidationEvents
    : JwtBearerEvents
{
    private readonly IUsuarioRepository _repository;

    public JwtTokenValidationEvents(
        IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public override async Task TokenValidated(
        TokenValidatedContext context)
    {
        var usuarioIdTexto =
            context.Principal?
                .FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.Principal?
                .FindFirstValue(JwtRegisteredClaimNames.Sub);

        var versionTexto =
            context.Principal?
                .FindFirstValue("token_version");

        if (!Guid.TryParse(usuarioIdTexto, out var usuarioId) ||
            !int.TryParse(versionTexto, out var versionToken))
        {
            context.Fail("El token no contiene datos de seguridad validos.");
            return;
        }

        var usuario = await _repository.GetByIdAsync(
            usuarioId,
            context.HttpContext.RequestAborted);

        if (usuario is null ||
            !usuario.Activo ||
            usuario.VersionSeguridad != versionToken)
        {
            context.Fail("El token fue invalidado.");
        }
    }
}