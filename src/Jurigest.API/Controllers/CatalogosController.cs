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

    [HttpGet("vehiculos/{categoria}")]
    [Authorize(Policy = "DiligenciasLectura")]
    public async Task<IActionResult> OpcionesVehiculo(string categoria, [FromServices] Jurigest.Persistence.Context.JurigestDbContext db, CancellationToken ct)
    {
        if (!CategoriaVehiculo(categoria)) return BadRequest();
        return Ok(await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(db.VehiculoOpciones.Where(x => x.Categoria == categoria).OrderBy(x => x.Nombre), ct));
    }
    [HttpPost("vehiculos/{categoria}")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> AgregarOpcionVehiculo(string categoria, CrearCatalogoRequest request, [FromServices] Jurigest.Persistence.Context.JurigestDbContext db, CancellationToken ct)
    {
        if (!CategoriaVehiculo(categoria) || string.IsNullOrWhiteSpace(request.Nombre) || request.Nombre.Trim().Length > 200) return BadRequest(new { mensaje = "Revise categoría y nombre." });
        var nombre = request.Nombre.Trim();
        if (await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(db.VehiculoOpciones, x => x.Categoria == categoria && x.Nombre == nombre, ct)) return Conflict(new { mensaje = "La opción ya existe." });
        var opcion = new Jurigest.Domain.Judicial.Catalogos.VehiculoOpcion { Id = Guid.NewGuid(), Categoria = categoria, Nombre = nombre };
        db.VehiculoOpciones.Add(opcion);
        try { await db.SaveChangesAsync(ct); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sql && sql.Number is 2601 or 2627)
        { return Conflict(new { mensaje = "La opción ya existe." }); }
        return Ok(opcion);
    }
    private static bool CategoriaVehiculo(string categoria) => categoria is "tipos" or "marcas" or "adquisiciones" or "alzamientos" or "limitaciones" or "documentos";

    [HttpGet("materias")]
    [Authorize(Policy = "CausasLectura")]
    public async Task<IActionResult> ObtenerMaterias([FromServices] Jurigest.Persistence.Context.JurigestDbContext db,
        CancellationToken cancellationToken)
    {
        var materias = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            db.Materias.OrderBy(x => x.Nombre), cancellationToken);
        return Ok(materias.Select(x => new { x.Id, x.Nombre }));
    }

    [HttpPost("materias")]
    [Authorize(Policy = "CausasEscritura")]
    public async Task<IActionResult> CrearMateria([FromBody] CrearCatalogoRequest request,
        [FromServices] Jurigest.Persistence.Context.JurigestDbContext db, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) || request.Nombre.Trim().Length > 200)
            return BadRequest(new { mensaje = "Ingrese una materia de hasta 200 caracteres." });
        var nombre = request.Nombre.Trim();
        if (await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(db.Materias,
            x => x.Nombre == nombre, cancellationToken))
            return Conflict(new { mensaje = "Ya existe una materia con ese nombre." });
        var materia = new Jurigest.Domain.Judicial.Catalogos.MateriaCatalogo(nombre);
        db.Materias.Add(materia);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sql && sql.Number is 2601 or 2627)
        { return Conflict(new { mensaje = "Ya existe una materia con ese nombre." }); }
        return Ok(new { materia.Id, materia.Nombre });
    }

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
            string.IsNullOrWhiteSpace(request.Nombre) ||
            !request.CodigoTipoDiligencia.HasValue ||
            request.CodigoTipoDiligencia.Value <= 0)
        {
            return BadRequest(new
            {
                mensaje =
                    "Debe indicar el nombre y un tipo de diligencia válido."
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
                    x.CodigoTipoDiligencia,
                    x.Arancel
                }));
    }

    [HttpPost("diligencias-realizadas")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> CrearDiligenciaRealizada(
        [FromBody] CrearDiligenciaRealizadaRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Nombre) ||
            request.Nombre.Trim().Length > 200 || request.CodigoTipoDiligencia <= 0)
        {
            return BadRequest(new { mensaje = "Indique un nombre de hasta 200 caracteres y un tipo de diligencia válido." });
        }

        var nombre = request.Nombre.Trim();
        if (await _diligenciaRealizadaRepository.ExisteNombreAsync(
                nombre, request.CodigoTipoDiligencia, cancellationToken))
        {
            return Conflict(new { mensaje = "Ya existe una diligencia realizada con ese nombre para este tipo." });
        }

        var diligencia = new DiligenciaRealizadaCatalogo(
            Guid.NewGuid(), nombre, request.CodigoTipoDiligencia);
        await _diligenciaRealizadaRepository.AddAsync(diligencia, cancellationToken);
        return Ok(new { diligencia.Id, diligencia.Nombre, diligencia.CodigoTipoDiligencia });
    }

    // ============================================================
    // REQUESTS
    // ============================================================

    public sealed class CrearCatalogoRequest
    {
        public string Nombre { get; set; } =
            string.Empty;
    }

    public sealed class CrearDiligenciaRealizadaRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public int CodigoTipoDiligencia { get; set; }
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
