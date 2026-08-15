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
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

        var resultado = await _mediator.Send(
            new IniciarSesionCommand(
                request.Email,
                request.Password),
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

    try
    {
        var resultado = await _mediator.Send(
            new CrearUsuarioCommand(
                request.Nombre,
                request.Email,
                request.Password,
                request.Rol),
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

    try
    {
        var actualizado = await _mediator.Send(
            new RestablecerPasswordCommand(
                request.Email,
                request.NuevaPassword),
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

        var resultado = await _mediator.Send(
            new CambiarEstadoUsuarioCommand(
                id,
                request.Activo,
                administradorId),
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
}