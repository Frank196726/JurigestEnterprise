using Jurigest.API.Contracts;
using Jurigest.Application.Seguridad.Commands.CrearAdministradorInicial;
using Jurigest.Application.Seguridad.Commands.IniciarSesion;
using Jurigest.Application.Seguridad.Commands.CrearUsuario;
using Jurigest.Domain.Seguridad.Enums;
using Microsoft.AspNetCore.Authorization;
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
}