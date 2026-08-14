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

    public DiligenciasController(IMediator mediator)
    {
        _mediator = mediator;
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
}