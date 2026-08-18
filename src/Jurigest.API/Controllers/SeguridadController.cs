using Jurigest.API.Contracts;
using Jurigest.Application.Seguridad.Commands.CrearAdministradorInicial;
using Jurigest.Application.Seguridad.Commands.IniciarSesion;
using Jurigest.Application.Seguridad.Commands.CrearUsuario;
using Jurigest.Domain.Seguridad.Enums;
using Microsoft.AspNetCore.Authorization;
using Jurigest.Application.Seguridad.Commands.RestablecerPassword;
using System.Security.Claims;
using Jurigest.Application.Seguridad.Commands.CambiarEstadoUsuario;
using Jurigest.Application.Seguridad.Queries.ObtenerUsuario;
using Jurigest.Application.Seguridad.Queries.ObtenerUsuarios;
using Jurigest.Application.Seguridad.Queries.ObtenerAuditoriasSeguridad;
using Jurigest.Application.Seguridad.Commands.RenovarSesion;
using Jurigest.Application.Seguridad.Commands.CerrarSesion;
using Jurigest.Application.Seguridad.Queries.ObtenerSesionesUsuario;
using Jurigest.Application.Seguridad.Commands.CerrarSesionPorId;
using Jurigest.Application.Seguridad.Commands.CerrarOtrasSesiones;
using Jurigest.Application.Seguridad.Commands.SolicitarRecuperacionPassword;
using Jurigest.Application.Seguridad.Commands.ConfirmarRecuperacionPassword;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/seguridad")]
public sealed class SeguridadController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    public SeguridadController(
        IMediator mediator,
        IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _environment = environment;
    }

    [HttpPost("bootstrap")]
    public async Task<IActionResult> CrearAdministradorInicial(
        [FromBody] CrearAdministradorInicialRequest request,
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        if (request is null ||
            string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar nombre, email y contraseña."
            });
        }

        try
        {
            var usuarioId = await _mediator.Send(
                new CrearAdministradorInicialCommand(
                    request.Nombre,
                    request.Email,
                    request.Password),
                    cancellationToken);

            if (usuarioId is null)
            {
                return Conflict(new
                {
                    mensaje = "El administrador inicial ya fue creado."
                });
            }

            return Created(
                $"/api/usuarios/{usuarioId}",
                new
                {
                    id = usuarioId,
                    mensaje = "Administrador inicial creado correctamente."
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [EnableRateLimiting("RecuperacionPassword")]
    [HttpPost("password/recuperacion/solicitar")]
    public async Task<IActionResult> SolicitarRecuperacionPassword(
    [FromBody] SolicitarRecuperacionPasswordRequest request,
    CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar el email."
            });
        }

        var direccionIp =
            HttpContext.Connection.RemoteIpAddress?.ToString();

        await _mediator.Send(
            new SolicitarRecuperacionPasswordCommand(
                request.Email,
                direccionIp),
            cancellationToken);

        return Ok(new
        {
            mensaje =
                "Si la cuenta existe, se enviaron las " +
                "instrucciones de recuperacion."
        });
    }

    [EnableRateLimiting("RecuperacionPassword")]
    [HttpPost("password/recuperacion/confirmar")]
    public async Task<IActionResult> ConfirmarRecuperacionPassword(
        [FromBody] ConfirmarRecuperacionPasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.NuevaPassword))
        {
            return BadRequest(new
            {
                mensaje =
                    "Debe indicar el token y la nueva contraseña."
            });
        }

        var direccionIp =
            HttpContext.Connection.RemoteIpAddress?.ToString();

        try
        {
            var resultado = await _mediator.Send(
                new ConfirmarRecuperacionPasswordCommand(
                    request.Token,
                    request.NuevaPassword,
                    direccionIp),
                cancellationToken);

            if (resultado ==
                ConfirmarRecuperacionPasswordResultado.TokenInvalido)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El token de recuperacion no es valido o expiro."
                });
            }

            return Ok(new
            {
                mensaje = "Contraseña recuperada correctamente."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [EnableRateLimiting("Login")]
    [HttpPost("login")]
    public async Task<IActionResult> IniciarSesion(
        [FromBody] IniciarSesionRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar email y contraseña."
            });
        }
        var direccionIp =
            HttpContext.Connection.RemoteIpAddress?.ToString();

        var userAgent =
            Request.Headers.UserAgent.ToString();

        var resultado = await _mediator.Send(
            new IniciarSesionCommand(
                request.Email,
                request.Password,
                direccionIp,
                userAgent),
                cancellationToken);

        if (resultado is null)
        {
            return Unauthorized(new
            {
                mensaje = "Credenciales invalidas."
            });
        }

        return Ok(resultado);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RenovarSesion(
        [FromBody] RenovarSesionRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar el refresh token."
            });
        }

        var direccionIp =
            HttpContext.Connection.RemoteIpAddress?.ToString();

        var userAgent =
            Request.Headers.UserAgent.ToString();

        var resultado = await _mediator.Send(
            new RenovarSesionCommand(
                request.RefreshToken,
                direccionIp,
                userAgent),
                cancellationToken);

        if (resultado is null)
        {
            return Unauthorized(new
            {
                mensaje = "El refresh token no es valido."
            });
        }

        return Ok(resultado);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> CerrarSesion(
        [FromBody] CerrarSesionRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar el refresh token."
            });
        }

        await _mediator.Send(
        new CerrarSesionCommand(
            request.RefreshToken),
            cancellationToken);

        return Ok(new
        {
            mensaje = "Sesion cerrada correctamente."
        });
    }
    [Authorize(Roles = "Administrador")]
    [HttpPost("usuarios")]
    public async Task<IActionResult> CrearUsuario(
        [FromBody] CrearUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar nombre, email, contraseña y rol."
            });
        }

        if (!Enum.IsDefined(typeof(RolUsuario), request.Rol))
        {
            return BadRequest(new
            {
                mensaje = "El rol no es valido."
            });
        }

        var usuarioActorIdTexto =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(
            usuarioActorIdTexto,
            out var usuarioActorId))
        {
            return Unauthorized(new
            {
                mensaje = "El token no contiene un usuario valido."
            });
        }

        var direccionIp =
            HttpContext.Connection.RemoteIpAddress?.ToString();

        try
        {
            var resultado = await _mediator.Send(
                new CrearUsuarioCommand(
                    request.Nombre,
                    request.Email,
                    request.Password,
                    request.Rol,
                    usuarioActorId,
                    direccionIp),
                    cancellationToken);

            if (resultado.EmailDuplicado)
            {
                return Conflict(new
                {
                    mensaje = "Ya existe un usuario con ese email."
                });
            }

            return Created(
                $"/api/usuarios/{resultado.UsuarioId}",
                new
                {
                    id = resultado.UsuarioId,
                    mensaje = "Usuario creado correctamente."
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("usuarios/password")]
    public async Task<IActionResult> RestablecerPassword(
        [FromBody] RestablecerPasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.NuevaPassword))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar email y nueva contraseña."
            });
        }

        var usuarioActorIdTexto =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(
            usuarioActorIdTexto,
        out var usuarioActorId))
        {
            return Unauthorized(new
            {
                mensaje = "El token no contiene un usuario valido."
            });
        }

        var direccionIp =
        HttpContext.Connection.RemoteIpAddress?.ToString();

        try
        {
            var actualizado = await _mediator.Send(
            new RestablecerPasswordCommand(
                request.Email,
                request.NuevaPassword,
                usuarioActorId,
                direccionIp),
                cancellationToken);

            if (!actualizado)
            {
                return NotFound(new
                {
                    mensaje = "El usuario no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Contraseña restablecida correctamente."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }
    [Authorize(Roles = "Administrador")]
    [HttpGet("usuarios")]
    public async Task<IActionResult> ObtenerUsuarios(
        CancellationToken cancellationToken)
    {
        var usuarios = await _mediator.Send(
            new ObtenerUsuariosQuery(),
            cancellationToken);

        return Ok(new
        {
            value = usuarios,
            count = usuarios.Count
        });
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet("usuarios/{id:guid}")]
    public async Task<IActionResult> ObtenerUsuario(
        Guid id,
        CancellationToken cancellationToken)
    {
        var usuario = await _mediator.Send(
            new ObtenerUsuarioQuery(id),
            cancellationToken);

        if (usuario is null)
        {
            return NotFound(new
            {
                mensaje = "El usuario no existe."
            });
        }

        return Ok(usuario);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("usuarios/{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstadoUsuario(
        Guid id,
        [FromBody] CambiarEstadoUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar el estado del usuario."
            });
        }

        var administradorIdTexto =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(
        administradorIdTexto,
        out var administradorId))
        {
            return Unauthorized(new
            {
                mensaje = "El token no contiene un usuario valido."
            });
        }

        var direccionIp =
            HttpContext.Connection.RemoteIpAddress?.ToString();

        var resultado = await _mediator.Send(
            new CambiarEstadoUsuarioCommand(
                id,
                request.Activo,
                administradorId,
                direccionIp),
                cancellationToken);

        return resultado switch
        {
            CambiarEstadoUsuarioResultado.NoEncontrado =>
            NotFound(new
            {
                mensaje = "El usuario no existe."
            }),

            CambiarEstadoUsuarioResultado
            .AutodesactivacionNoPermitida =>
            Conflict(new
            {
                mensaje =
                    "No puede desactivar su propia cuenta."
            }),

            _ => Ok(new
            {
                mensaje = request.Activo
                    ? "Usuario activado correctamente."
                    : "Usuario desactivado correctamente."
            })
        };

    }

    [Authorize]
    [HttpGet("sesiones")]
    public async Task<IActionResult> ObtenerSesiones(
        CancellationToken cancellationToken)
    {
        var usuarioIdTexto =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        var sesionIdTexto =
            User.FindFirst("session_id")?.Value;

        if (!Guid.TryParse(
                usuarioIdTexto,
                out var usuarioId) ||
                !Guid.TryParse(
                sesionIdTexto,
                out var sesionId))
        {
            return Unauthorized(new
            {
                mensaje = "El token no contiene una sesion valida."
            });
        }

        var sesiones = await _mediator.Send(
            new ObtenerSesionesUsuarioQuery(
                usuarioId,
                sesionId),
                cancellationToken);

        return Ok(new
        {
            value = sesiones,
            count = sesiones.Count
        });
    }

    [Authorize]
    [HttpDelete("sesiones/otras")]
    public async Task<IActionResult> CerrarOtrasSesiones(
        CancellationToken cancellationToken)
    {
        var usuarioIdTexto =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        var sesionIdTexto =
            User.FindFirst("session_id")?.Value;

        if (!Guid.TryParse(
                usuarioIdTexto,
                out var usuarioId) ||
            !Guid.TryParse(
                sesionIdTexto,
                out var sesionId))
        {
            return Unauthorized(new
            {
                mensaje = "El token no contiene una sesion valida."
            });
        }

        var direccionIp =
            HttpContext.Connection.RemoteIpAddress?.ToString();

        var cantidad = await _mediator.Send(
            new CerrarOtrasSesionesCommand(
                usuarioId,
                sesionId,
                direccionIp),
                cancellationToken);

        return Ok(new
        {
            mensaje = "Las otras sesiones fueron cerradas correctamente.",
            cantidad
        });
    }

    [Authorize]
    [HttpDelete("sesiones/{id:guid}")]
    public async Task<IActionResult> CerrarSesionPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var usuarioIdTexto =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(
                usuarioIdTexto,
                out var usuarioId))
        {
            return Unauthorized(new
            {
                mensaje = "El token no contiene un usuario valido."
            });
        }

        var direccionIp =
            HttpContext.Connection.RemoteIpAddress?.ToString();

        var encontrada = await _mediator.Send(
            new CerrarSesionPorIdCommand(
                usuarioId,
                id,
                direccionIp),
                cancellationToken);

        if (!encontrada)
        {
            return NotFound(new
            {
                mensaje = "La sesion no existe."
            });
        }

        return Ok(new
        {
            mensaje = "Sesion cerrada correctamente."
        });
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet("auditorias")]
    public async Task<IActionResult> ObtenerAuditoriasSeguridad(
        [FromQuery] int cantidad = 100,
        CancellationToken cancellationToken = default)
    {
        var auditorias = await _mediator.Send(
            new ObtenerAuditoriasSeguridadQuery(cantidad),
            cancellationToken);

        return Ok(new
        {
            value = auditorias,
            count = auditorias.Count
        });
    }
}