using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jurigest.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Jurigest.API.Security;

public sealed class JwtTokenValidationEvents
    : JwtBearerEvents
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISesionUsuarioRepository
        _sesionRepository;

    public JwtTokenValidationEvents(
        IUsuarioRepository usuarioRepository,
        ISesionUsuarioRepository sesionRepository)
    {
        _usuarioRepository = usuarioRepository;
        _sesionRepository = sesionRepository;
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

        var sesionIdTexto =
            context.Principal?
                .FindFirstValue("session_id");

        if (!Guid.TryParse(
                usuarioIdTexto,
                out var usuarioId) ||
            !int.TryParse(
                versionTexto,
                out var versionToken) ||
            !Guid.TryParse(
                sesionIdTexto,
                out var sesionId))
        {
            context.Fail(
                "El token no contiene datos de seguridad validos.");

            return;
        }

        var sesion = await _sesionRepository.GetByIdAsync(
            sesionId,
            context.HttpContext.RequestAborted);

        if (sesion is null ||
            sesion.UsuarioId != usuarioId ||
            sesion.VersionSeguridad != versionToken ||
            !sesion.EstaActiva(DateTime.UtcNow))
        {
            context.Fail("La sesion fue invalidada.");
            return;
        }

        var usuario = await _usuarioRepository.GetByIdAsync(
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