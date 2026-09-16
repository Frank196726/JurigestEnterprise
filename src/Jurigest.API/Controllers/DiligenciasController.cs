using Jurigest.API.Contracts;
using Jurigest.Application.Judicial.Diligencias.Commands.AgregarObservacion;
using Jurigest.Application.Judicial.Diligencias.Commands.RegistrarCoordenadas;
using Jurigest.Application.Judicial.Diligencias.Commands.AsignarUbicacion;
using Jurigest.Application.Judicial.Diligencias.Commands.RechazarDiligencia;
using Jurigest.Application.Judicial.Diligencias.Commands.AsignarReceptor;
using Jurigest.Application.Judicial.Diligencias.Commands.CambiarTipoDiligencia;
using Jurigest.Application.Judicial.Diligencias.Commands.CompletarDiligencia;
using Jurigest.Application.Judicial.Diligencias.Commands.IniciarDiligencia;
using Jurigest.Application.Judicial.Diligencias.Commands.ProgramarDiligencia;
using Jurigest.Application.Judicial.Diligencias.Commands.SuspenderDiligencia;
using Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligencia;
using Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligenciasPorCausa;
using Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligenciasPorPlazo;
using Jurigest.Application.Judicial.Diligencias.Commands.RegistrarResultado;
using Jurigest.Application.Judicial.Diligencias.Commands.ActualizarDiligencia;
using Jurigest.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "DiligenciasLectura")]
public sealed class DiligenciasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDiligenciaRepository _diligenciaRepository;

    public DiligenciasController(
        IMediator mediator,
        IDiligenciaRepository diligenciaRepository)
{
        _mediator = mediator;
        _diligenciaRepository = diligenciaRepository;
}

    [HttpGet("plazos")]
    public async Task<IActionResult> ObtenerPorPlazo(
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new ObtenerDiligenciasPorPlazoQuery(),
            cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
        public async Task<IActionResult> Obtener(
        Guid id,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new ObtenerDiligenciaQuery(id),
            cancellationToken);

        if (resultado is null)
        {
            return NotFound(new
            {
                mensaje = "La diligencia no existe."
            });
        }

        return Ok(resultado);
    }

    [HttpGet("causa/{causaId:guid}")]
        public async Task<IActionResult> ObtenerPorCausa(
        Guid causaId,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new ObtenerDiligenciasPorCausaQuery(causaId),
            cancellationToken);

        return Ok(resultado);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        [FromBody] ActualizarDiligenciaRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Descripcion))
            return BadRequest(new { mensaje = "La descripción de la diligencia es obligatoria." });

        try
        {
            var resultado = await _mediator.Send(
                new ActualizarDiligenciaCommand(
                    id,
                    request.Descripcion,
                    request.Tipo,
                    request.FechaProgramada,
                    request.ReceptorJudicial,
                    request.Direccion,
                    request.Comuna,
                    request.Observaciones),
                cancellationToken);

            if (!resultado)
                return NotFound(new { mensaje = "La diligencia no existe." });

            return Ok(new { mensaje = "Diligencia actualizada correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:guid}/iniciar")]
    [Authorize(Policy = "DiligenciasGestion")]
        public async Task<IActionResult> Iniciar(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _mediator.Send(
                new IniciarDiligenciaCommand(id),
                cancellationToken);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "La diligencia no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Diligencia iniciada correctamente."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpGet("causa/{causaId:guid}/ultima")]
    [Authorize(Policy = "DiligenciasLectura")]
    public async Task<IActionResult> ObtenerUltima(
    Guid causaId,
    CancellationToken cancellationToken)
{
    var diligencia =
        await _diligenciaRepository.GetUltimaByCausaAsync(
            causaId,
            cancellationToken);

    if (diligencia is null)
    {
        return NotFound(new
        {
            mensaje =
                "La causa no tiene diligencias registradas."
        });
    }

    return Ok(new
    {
        diligencia.Id,
        diligencia.CausaId,
        diligencia.Descripcion,
        diligencia.Tipo,
        diligencia.Estado,
        diligencia.Resultado,
        diligencia.ResultadoDetalle,
        diligencia.DiligenciaRealizadaId,
        diligencia.DiligenciaRealizada,
        diligencia.FechaProgramada,
        diligencia.FechaGestion,
        diligencia.ReceptorJudicial,
        diligencia.Direccion,
        diligencia.Comuna,
        diligencia.Estampe,
        diligencia.FechaCreacion
    });
}

    [HttpPut("{id:guid}/completar")]
    [Authorize(Policy = "DiligenciasGestion")]
        public async Task<IActionResult> Completar(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _mediator.Send(
                new CompletarDiligenciaCommand(id),
                cancellationToken);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "La diligencia no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Diligencia completada correctamente."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpPut("{id:guid}/programar")]
    [Authorize(Policy = "DiligenciasGestion")]
        public async Task<IActionResult> Programar(
        Guid id,
        [FromBody] ProgramarDiligenciaRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar la fecha programada."
            });
        }

        try
        {
            var resultado = await _mediator.Send(
                new ProgramarDiligenciaCommand(
                    id,
                    request.FechaProgramada),
                cancellationToken);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "La diligencia no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Diligencia programada correctamente."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpPut("{id:guid}/asignar-receptor")]
    [Authorize(Policy = "DiligenciasGestion")]
        public async Task<IActionResult> AsignarReceptor(
        Guid id,
        [FromBody] AsignarReceptorRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.ReceptorJudicial))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar el receptor judicial."
            });
        }

        try
        {
            var resultado = await _mediator.Send(
                new AsignarReceptorCommand(
                    id,
                    request.ReceptorJudicial),
                cancellationToken);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "La diligencia no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Receptor asignado correctamente."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpPut("{id:guid}/tipo")]
    [Authorize(Policy = "DiligenciasGestion")]
        public async Task<IActionResult> CambiarTipo(
        Guid id,
        [FromBody] CambiarTipoDiligenciaRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar el tipo de diligencia."
            });
        }

        try
        {
            var resultado = await _mediator.Send(
                new CambiarTipoDiligenciaCommand(
                    id,
                    request.Tipo),
                cancellationToken);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "La diligencia no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Tipo de diligencia actualizado correctamente."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpPut("{id:guid}/suspender")]
    [Authorize(Policy = "DiligenciasGestion")]
        public async Task<IActionResult> Suspender(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _mediator.Send(
                new SuspenderDiligenciaCommand(id),
                cancellationToken);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "La diligencia no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Diligencia suspendida correctamente."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                mensaje = ex.Message
            });
        }
    }
    [HttpPut("{id:guid}/rechazar")]
    [Authorize(Policy = "DiligenciasGestion")]
        public async Task<IActionResult> Rechazar(
        Guid id,
        CancellationToken cancellationToken)
{
        try
    {
            var resultado = await _mediator.Send(
                new RechazarDiligenciaCommand(id),
                cancellationToken);

            if (!resultado)
        {
                return NotFound(new
            {
                mensaje = "La diligencia no existe."
            });
        }

            return Ok(new
        {
                mensaje = "Diligencia rechazada correctamente."
        });
    }
        catch (InvalidOperationException ex)
    {
                return Conflict(new
        {
                mensaje = ex.Message
        });
    }

}
    [HttpPut("{id:guid}/ubicacion")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> AsignarUbicacion(
        Guid id,
        [FromBody] AsignarUbicacionRequest request,
        CancellationToken cancellationToken)
{
    if (request is null ||
        string.IsNullOrWhiteSpace(request.Direccion) ||
        string.IsNullOrWhiteSpace(request.Comuna))
    {
        return BadRequest(new
        {
            mensaje = "Debe indicar dirección y comuna."
        });
    }

    try
    {
        var resultado = await _mediator.Send(
            new AsignarUbicacionCommand(
                id,
                request.Direccion,
                request.Comuna),
            cancellationToken);

        if (!resultado)
        {
            return NotFound(new
            {
                mensaje = "La diligencia no existe."
            });
        }

        return Ok(new
        {
            mensaje = "Ubicación asignada correctamente."
        });
    }
    catch (InvalidOperationException ex)
    {
        return Conflict(new
        {
            mensaje = ex.Message
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


        [HttpPut("{id:guid}/coordenadas")]
        [Authorize(Policy = "DiligenciasGestion")]
        public async Task<IActionResult> RegistrarCoordenadas(
            Guid id,
            [FromBody] RegistrarCoordenadasRequest request,
            CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar las coordenadas."
            });
        }

        try
        {
            var resultado = await _mediator.Send(
                new RegistrarCoordenadasCommand(
                    id,
                    request.Latitud,
                    request.Longitud),
                cancellationToken);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "La diligencia no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Coordenadas registradas correctamente."
            });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
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

        [HttpPut("{id:guid}/observacion")]
        [Authorize(Policy = "DiligenciasGestion")]
        public async Task<IActionResult> AgregarObservacion(
            Guid id,
            [FromBody] AgregarObservacionRequest request,
            CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Observacion))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar una observación."
            });
        }

        var resultado = await _mediator.Send(
            new AgregarObservacionCommand(
                id,
                request.Observacion),
                cancellationToken);

        if (!resultado)
        {
            return NotFound(new
            {
                mensaje = "La diligencia no existe."
            });
        }

        return Ok(new
        {
            mensaje = "Observación registrada correctamente."
        });
    }

        [HttpPut("{id:guid}/resultado")]
        [Authorize(Policy = "DiligenciasGestion")]
        public async Task<IActionResult> RegistrarResultado(
        Guid id,
        [FromBody] RegistrarResultadoDiligenciaRequest request,
        CancellationToken cancellationToken)
{
    if (request is null)
    {
        return BadRequest(new
        {
            mensaje = "La solicitud no contiene datos."
        });
    }

    if (request.DiligenciaRealizadaId == Guid.Empty)
    {
        return BadRequest(new
        {
            mensaje = "Debe indicar la diligencia realizada."
        });
    }

    if (request.Resultado == 0)
    {
        return BadRequest(new
        {
            mensaje = "Debe indicar el resultado de la diligencia."
        });
    }

    if (string.IsNullOrWhiteSpace(request.ResultadoDetalle))
{
    return BadRequest(new
    {
        mensaje = "Debe indicar el detalle del resultado de la diligencia."
    });
}

    if (request.ResultadoDetalle.Trim().Length > 500)
{
    return BadRequest(new
    {
        mensaje = "El detalle del resultado no puede superar 500 caracteres."
    });
}

    if (string.IsNullOrWhiteSpace(request.Estampe))
    {
        return BadRequest(new
        {
            mensaje = "Debe indicar el estampe de la diligencia."
        });
    }

    if (request.FechaGestion == default)
    {
        return BadRequest(new
        {
            mensaje = "Debe indicar la fecha de gestión."
        });
    }

    try
    {
        await _mediator.Send(
            new RegistrarResultadoDiligenciaCommand(
                id,
                request.DiligenciaRealizadaId,
                request.Resultado,
                request.ResultadoDetalle,
                request.Estampe,
                request.FechaGestion),
            cancellationToken);

        return Ok(new
        {
            mensaje = "Resultado de diligencia registrado correctamente."
        });
    }
    catch (InvalidOperationException ex)
    {
        return NotFound(new
        {
            mensaje = ex.Message
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
