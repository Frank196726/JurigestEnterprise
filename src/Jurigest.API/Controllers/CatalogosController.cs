using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CatalogosController : ControllerBase
{
    private readonly ITipoDiligenciaCatalogoRepository
        _tipoDiligenciaRepository;

    private readonly IReceptorJudicialCatalogoRepository
        _receptorRepository;

    private readonly IComunaCatalogoRepository
        _comunaRepository;

    private readonly ITipoCausaCatalogoRepository
        _tipoCausaRepository;

    private readonly ITribunalCatalogoRepository
        _tribunalRepository;

    private readonly IAbogadoCatalogoRepository
        _abogadoRepository;

    private readonly IDiligenciaEncargadaCatalogoRepository
        _diligenciaEncargadaRepository;

    private readonly IDiligenciaRealizadaCatalogoRepository
        _diligenciaRealizadaRepository;

    public CatalogosController(
        ITipoDiligenciaCatalogoRepository tipoDiligenciaRepository,
        IReceptorJudicialCatalogoRepository receptorRepository,
        IComunaCatalogoRepository comunaRepository,
        ITipoCausaCatalogoRepository tipoCausaRepository,
        ITribunalCatalogoRepository tribunalRepository,
        IAbogadoCatalogoRepository abogadoRepository,
        IDiligenciaEncargadaCatalogoRepository diligenciaEncargadaRepository,
        IDiligenciaRealizadaCatalogoRepository diligenciaRealizadaRepository)

    {
        _tipoDiligenciaRepository =
            tipoDiligenciaRepository;

        _receptorRepository =
            receptorRepository;

        _comunaRepository =
            comunaRepository;

        _tipoCausaRepository =
            tipoCausaRepository;

        _tribunalRepository =
            tribunalRepository;

        _abogadoRepository =
            abogadoRepository;

        _diligenciaEncargadaRepository =
            diligenciaEncargadaRepository;

        _diligenciaRealizadaRepository =
            diligenciaRealizadaRepository;
    }

    // ============================================================
    // TIPOS DE DILIGENCIA
    // ============================================================

    [HttpGet("tipos-diligencia")]
    [Authorize(Policy = "DiligenciasLectura")]
    public async Task<IActionResult> ObtenerTiposDiligencia(
        CancellationToken cancellationToken)
    {
        var tipos =
            await _tipoDiligenciaRepository
                .GetActivosAsync(cancellationToken);

        return Ok(
            tipos.Select(
                x => new
                {
                    x.Id,
                    x.Nombre,
                    x.CodigoSistema
                }));
    }

    [HttpPost("tipos-diligencia")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> CrearTipoDiligencia(
        [FromBody] CrearCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new
            {
                mensaje =
                    "Debe indicar el nombre del tipo de diligencia."
            });
        }

        var nombre =
            request.Nombre.Trim();

        if (await _tipoDiligenciaRepository
                .ExisteNombreAsync(
                    nombre,
                    cancellationToken))
        {
            return Conflict(new
            {
                mensaje =
                    "Ya existe un tipo de diligencia con ese nombre."
            });
        }

        var codigoSistema =
            await _tipoDiligenciaRepository
                .ObtenerSiguienteCodigoPersonalizadoAsync(
                    cancellationToken);

        var tipo =
            new TipoDiligenciaCatalogo(
                Guid.NewGuid(),
                nombre,
                codigoSistema);

        await _tipoDiligenciaRepository.AddAsync(
            tipo,
            cancellationToken);

        return Ok(new
        {
            tipo.Id,
            tipo.Nombre,
            tipo.CodigoSistema
        });
    }

    // ============================================================
    // RECEPTORES JUDICIALES
    // ============================================================

    [HttpGet("receptores")]
    [Authorize(Policy = "DiligenciasLectura")]
    public async Task<IActionResult> ObtenerReceptores(
        CancellationToken cancellationToken)
    {
        var receptores =
            await _receptorRepository
                .GetActivosAsync(cancellationToken);

        return Ok(
            receptores.Select(
                x => new
                {
                    x.Id,
                    x.Nombre
                }));
    }

    [HttpPost("receptores")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> CrearReceptor(
        [FromBody] CrearCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new
            {
                mensaje =
                    "Debe indicar el nombre del receptor judicial."
            });
        }

        var nombre =
            request.Nombre.Trim();

        if (await _receptorRepository
                .ExisteNombreAsync(
                    nombre,
                    cancellationToken))
        {
            return Conflict(new
            {
                mensaje =
                    "Ya existe un receptor judicial con ese nombre."
            });
        }

        var receptor =
            new ReceptorJudicialCatalogo(
                Guid.NewGuid(),
                nombre);

        await _receptorRepository.AddAsync(
            receptor,
            cancellationToken);

        return Ok(new
        {
            receptor.Id,
            receptor.Nombre
        });
    }

    // ============================================================
    // COMUNAS
    // ============================================================

    [HttpGet("comunas")]
    [Authorize(Policy = "DiligenciasLectura")]
    public async Task<IActionResult> ObtenerComunas(
        CancellationToken cancellationToken)
    {
        var comunas =
            await _comunaRepository
                .GetActivosAsync(cancellationToken);

        return Ok(
            comunas.Select(
                x => new
                {
                    x.Id,
                    x.Nombre
                }));
    }

    [HttpPost("comunas")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> CrearComuna(
        [FromBody] CrearCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new
            {
                mensaje =
                    "Debe indicar el nombre de la comuna."
            });
        }

        var nombre =
            request.Nombre.Trim();

        if (await _comunaRepository
                .ExisteNombreAsync(
                    nombre,
                    cancellationToken))
        {
            return Conflict(new
            {
                mensaje =
                    "Ya existe una comuna con ese nombre."
            });
        }

        var comuna =
            new ComunaCatalogo(
                Guid.NewGuid(),
                nombre);

        await _comunaRepository.AddAsync(
            comuna,
            cancellationToken);

        return Ok(new
        {
            comuna.Id,
            comuna.Nombre
        });
    }

    // ============================================================
    // TIPOS DE CAUSA
    // ============================================================

    [HttpGet("tipos-causa")]
    [Authorize(Policy = "CausasLectura")]
    public async Task<IActionResult> ObtenerTiposCausa(
        CancellationToken cancellationToken)
    {
        var tipos =
            await _tipoCausaRepository
                .GetActivosAsync(cancellationToken);

        return Ok(
            tipos.Select(
                x => new
                {
                    x.Id,
                    x.Nombre,
                    x.Codigo
                }));
    }

    [HttpPost("tipos-causa")]
    [Authorize(Policy = "CausasEscritura")]
    public async Task<IActionResult> CrearTipoCausa(
        [FromBody] CrearTipoCausaRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Codigo))
        {
            return BadRequest(new
            {
                mensaje =
                    "Debe indicar nombre y cÃ³digo del tipo de causa."
            });
        }

        var nombre =
            request.Nombre.Trim();

        var codigo =
            request.Codigo
                .Trim()
                .ToUpperInvariant();

        if (await _tipoCausaRepository
                .ExisteNombreAsync(
                    nombre,
                    cancellationToken))
        {
            return Conflict(new
            {
                mensaje =
                    "Ya existe un tipo de causa con ese nombre."
            });
        }

        if (await _tipoCausaRepository
                .ExisteCodigoAsync(
                    codigo,
                    cancellationToken))
        {
            return Conflict(new
            {
                mensaje =
                    "Ya existe un tipo de causa con ese cÃ³digo."
            });
        }

        var tipo =
            new TipoCausaCatalogo(
                Guid.NewGuid(),
                nombre,
                codigo);

        await _tipoCausaRepository.AddAsync(
            tipo,
            cancellationToken);

        return Ok(new
        {
            tipo.Id,
            tipo.Nombre,
            tipo.Codigo
        });
    }

    // ============================================================
    // TRIBUNALES
    // ============================================================

    [HttpGet("tribunales")]
    [Authorize(Policy = "CausasLectura")]
    public async Task<IActionResult> ObtenerTribunales(
        CancellationToken cancellationToken)
    {
        var tribunales =
            await _tribunalRepository
                .GetActivosAsync(cancellationToken);

        return Ok(
            tribunales.Select(
                x => new
                {
                    x.Id,
                    x.Nombre
                }));
    }

    [HttpPost("tribunales")]
    [Authorize(Policy = "CausasEscritura")]
    public async Task<IActionResult> CrearTribunal(
        [FromBody] CrearCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new
            {
                mensaje =
                    "Debe indicar el nombre del tribunal."
            });
        }

        var nombre =
            request.Nombre.Trim();

        if (await _tribunalRepository
                .ExisteNombreAsync(
                    nombre,
                    cancellationToken))
        {
            return Conflict(new
            {
                mensaje =
                    "Ya existe un tribunal con ese nombre."
            });
        }

        var tribunal =
            new TribunalCatalogo(
                Guid.NewGuid(),
                nombre);

        await _tribunalRepository.AddAsync(
            tribunal,
            cancellationToken);

        return Ok(new
        {
            tribunal.Id,
            tribunal.Nombre
        });
    }

    // ============================================================
    // ABOGADOS
    // ============================================================

    [HttpGet("abogados")]
    [Authorize(Policy = "CausasLectura")]
    public async Task<IActionResult> ObtenerAbogados(
        CancellationToken cancellationToken)
    {
        var abogados =
            await _abogadoRepository
                .GetActivosAsync(cancellationToken);

        return Ok(
            abogados.Select(
                x => new
                {
                    x.Id,
                    x.Nombre,
                    x.UsuarioId
                }));
    }

    [HttpPost("abogados")]
    [Authorize(Policy = "CausasEscritura")]
    public async Task<IActionResult> CrearAbogado(
        [FromBody] CrearCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new
            {
                mensaje =
                    "Debe indicar el nombre del abogado."
            });
        }

        var nombre =
            request.Nombre.Trim();

        if (await _abogadoRepository
                .ExisteNombreAsync(
                    nombre,
                    cancellationToken))
        {
            return Conflict(new
            {
                mensaje =
                    "Ya existe un abogado con ese nombre."
            });
        }

        var abogado =
            new AbogadoCatalogo(
                Guid.NewGuid(),
                nombre);

        await _abogadoRepository.AddAsync(
            abogado,
            cancellationToken);

        return Ok(new
        {
            abogado.Id,
            abogado.Nombre,
            abogado.UsuarioId
        });
    }

    // ============================================================
    // DILIGENCIAS ENCARGADAS
    // ============================================================

    [HttpGet("diligencias-encargadas")]
    [Authorize(Policy = "DiligenciasLectura")]
    public async Task<IActionResult> ObtenerDiligenciasEncargadas(
        CancellationToken cancellationToken)
    {
        var diligencias =
            await _diligenciaEncargadaRepository
                .GetActivosAsync(cancellationToken);

        return Ok(
            diligencias.Select(
                x => new
                {
                    x.Id,
                    x.Nombre,
                    x.CodigoTipoDiligencia
                }));
    }

    [HttpPost("diligencias-encargadas")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> CrearDiligenciaEncargada(
        [FromBody] CrearDiligenciaEncargadaRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new
            {
                mensaje =
                    "Debe indicar el nombre de la diligencia encargada."
            });
        }

        var nombre =
            request.Nombre.Trim();

        if (await _diligenciaEncargadaRepository
                .ExisteNombreAsync(
                    nombre,
                    cancellationToken))
        {
            return Conflict(new
            {
                mensaje =
                    "Ya existe una diligencia encargada con ese nombre."
            });
        }

        var diligencia =
            new DiligenciaEncargadaCatalogo(
                Guid.NewGuid(),
                nombre,
                request.CodigoTipoDiligencia);

        await _diligenciaEncargadaRepository.AddAsync(
            diligencia,
            cancellationToken);

        return Ok(new
        {
            diligencia.Id,
            diligencia.Nombre,
            diligencia.CodigoTipoDiligencia
        });
    }

    // ============================================================
    // DILIGENCIAS REALIZADAS
    // ============================================================

    [HttpGet("diligencias-realizadas")]
    [Authorize(Policy = "DiligenciasLectura")]
    public async Task<IActionResult> ObtenerDiligenciasRealizadas(
        [FromQuery] int? codigoTipoDiligencia,
        CancellationToken cancellationToken)
    {
        var diligencias =
            codigoTipoDiligencia.HasValue
                ? await _diligenciaRealizadaRepository
                    .GetActivosPorTipoAsync(
                        codigoTipoDiligencia.Value,
                        cancellationToken)
                : await _diligenciaRealizadaRepository
                    .GetActivosAsync(
                        cancellationToken);

        return Ok(
            diligencias.Select(
                x => new
                {
                    x.Id,
                    x.Nombre,
                    x.CodigoTipoDiligencia
                }));
    }

    // ============================================================
    // REQUESTS
    // ============================================================

    public sealed class CrearCatalogoRequest
    {
        public string Nombre { get; set; } =
            string.Empty;
    }

    public sealed class CrearTipoCausaRequest
    {
        public string Nombre { get; set; } =
            string.Empty;

        public string Codigo { get; set; } =
            string.Empty;
    }

    public sealed class CrearDiligenciaEncargadaRequest
    {
        public string Nombre { get; set; } =
            string.Empty;

        public int? CodigoTipoDiligencia { get; set; }
    }
}
