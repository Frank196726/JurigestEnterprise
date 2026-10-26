using Jurigest.API.Contracts;
using Jurigest.Application.Judicial;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Domain.Seguridad.Enums;
using Jurigest.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jurigest.API.Security;

/// <summary>
/// Calcula, una vez por solicitud, qué receptores judiciales puede ver el usuario actual.
/// Los administradores y los usuarios sin receptores asociados no tienen restricción.
/// </summary>
public sealed class AccesoReceptores(JurigestDbContext db, IHttpContextAccessor accessor)
{
    private bool _calculado;
    private FiltroReceptores? _filtro;
    private string? _receptorPropio;

    public async Task<FiltroReceptores?> ObtenerFiltroAsync(CancellationToken ct)
    {
        if (_calculado) return _filtro;
        _calculado = true;

        var principal = accessor.HttpContext?.User;
        if (!Guid.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            return _filtro;

        var usuario = await db.Usuarios.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.Nombre, x.Rol, x.NombreRolPersonalizado })
            .SingleOrDefaultAsync(ct);
        if (usuario is null || usuario.Rol == RolUsuario.Administrador)
            return _filtro;

        var asociados = await db.UsuariosReceptores.AsNoTracking()
            .Where(x => x.UsuarioId == id)
            .Join(db.ReceptoresJudiciales, a => a.ReceptorJudicialId, r => r.Id, (a, r) => new { r.Id, r.Nombre })
            .ToListAsync(ct);
        var ids = asociados.Select(x => x.Id).ToList();
        var nombres = asociados.Select(x => x.Nombre).ToList();

        // Compatibilidad: el rol "Receptor" ya filtraba por el nombre del propio usuario.
        if (string.Equals(usuario.NombreRolPersonalizado, "Receptor", StringComparison.OrdinalIgnoreCase))
        {
            _receptorPropio = usuario.Nombre;
            nombres.Add(usuario.Nombre);
            var propio = await db.ReceptoresJudiciales.AsNoTracking()
                .Where(r => r.Nombre == usuario.Nombre)
                .Select(r => (Guid?)r.Id)
                .FirstOrDefaultAsync(ct);
            if (propio is Guid propioId) ids.Add(propioId);
        }

        _filtro = nombres.Count == 0 ? null : new FiltroReceptores(ids, nombres);
        return _filtro;
    }

    /// <summary>
    /// Receptor que se asigna automáticamente a una diligencia creada por el usuario:
    /// el propio usuario si su rol es "Receptor", o su único receptor asociado.
    /// </summary>
    public async Task<string?> ObtenerReceptorUnicoAsync(CancellationToken ct)
    {
        var filtro = await ObtenerFiltroAsync(ct);
        return _receptorPropio ?? (filtro?.Nombres.Count == 1 ? filtro.Nombres.Single() : null);
    }

    public async Task<bool> PuedeVerCausaAsync(Guid causaId, CancellationToken ct)
    {
        var filtro = await ObtenerFiltroAsync(ct);
        if (filtro is null) return true;

        var asignaciones = await db.Diligencias.AsNoTracking()
            .Where(d => d.CausaId == causaId)
            .Select(d => new { d.ReceptorJudicialId, d.ReceptorJudicial })
            .ToListAsync(ct);
        return filtro.PermiteCausa(asignaciones.Select(x => (x.ReceptorJudicialId, x.ReceptorJudicial)));
    }

    public async Task<bool> PuedeVerDiligenciaAsync(Guid diligenciaId, CancellationToken ct)
    {
        var filtro = await ObtenerFiltroAsync(ct);
        if (filtro is null) return true;

        var diligencia = await db.Diligencias.AsNoTracking()
            .Where(d => d.Id == diligenciaId)
            .Select(d => new { d.CausaId, d.ReceptorJudicialId, d.ReceptorJudicial })
            .SingleOrDefaultAsync(ct);
        if (diligencia is null) return true; // El endpoint responde que no existe.

        return filtro.PermiteDiligencia(
            diligencia.ReceptorJudicialId,
            diligencia.ReceptorJudicial,
            await PuedeVerCausaAsync(diligencia.CausaId, ct));
    }

    public async Task<bool> PuedeVerAsync(RecursoJudicial recurso, Guid id, CancellationToken ct)
    {
        switch (recurso)
        {
            case RecursoJudicial.Causa:
                return await PuedeVerCausaAsync(id, ct);
            case RecursoJudicial.Diligencia:
                return await PuedeVerDiligenciaAsync(id, ct);
            case RecursoJudicial.Documento:
                var documento = await db.Documentos.AsNoTracking().Where(x => x.Id == id).Select(x => (Guid?)x.CausaId).SingleOrDefaultAsync(ct);
                return documento is null || await PuedeVerCausaAsync(documento.Value, ct);
            case RecursoJudicial.Resolucion:
                var resolucion = await db.Resoluciones.AsNoTracking().Where(x => x.Id == id).Select(x => (Guid?)x.CausaId).SingleOrDefaultAsync(ct);
                return resolucion is null || await PuedeVerCausaAsync(resolucion.Value, ct);
            case RecursoJudicial.Recibo:
                var recibo = await db.Recibos.AsNoTracking().Where(x => x.Id == id).Select(x => (Guid?)x.DiligenciaId).SingleOrDefaultAsync(ct);
                return recibo is null || await PuedeVerDiligenciaAsync(recibo.Value, ct);
            default:
                return false;
        }
    }

    /// <summary>
    /// Un usuario con receptores solo puede asignar diligencias a sus propios receptores.
    /// Reenviar el receptor que el registro ya tiene no es una asignación nueva.
    /// </summary>
    public async Task<bool> PuedeAsignarAsync(
        string? receptor, RecursoJudicial recurso, Guid? id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(receptor)) return true;
        var filtro = await ObtenerFiltroAsync(ct);
        if (filtro is null || filtro.EsPropio(receptor)) return true;
        return id is Guid recursoId &&
            receptor.Trim() == (await ObtenerReceptorActualAsync(recurso, recursoId, ct))?.Trim();
    }

    /// <summary>
    /// Asocia un receptor recién creado al usuario actual cuando este tiene el acceso
    /// restringido, para que pueda usarlo de inmediato.
    /// </summary>
    public async Task AsociarReceptorCreadoAsync(Guid receptorId, CancellationToken ct)
    {
        if (await ObtenerFiltroAsync(ct) is null) return;
        if (!Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return;

        db.UsuariosReceptores.Add(new UsuarioReceptor(usuarioId, receptorId));
        await db.SaveChangesAsync(ct);
    }

    // Mismo criterio que ActualizarCausa: el receptor de la causa es el de su primera diligencia.
    private async Task<string?> ObtenerReceptorActualAsync(RecursoJudicial recurso, Guid id, CancellationToken ct) =>
        recurso switch
        {
            RecursoJudicial.Diligencia => await db.Diligencias.AsNoTracking()
                .Where(d => d.Id == id).Select(d => d.ReceptorJudicial).SingleOrDefaultAsync(ct),
            RecursoJudicial.Causa => await db.Diligencias.AsNoTracking()
                .Where(d => d.CausaId == id)
                .OrderBy(d => d.FechaCreacion).ThenBy(d => d.Id)
                .Select(d => d.ReceptorJudicial).FirstOrDefaultAsync(ct),
            _ => null
        };
}

public enum RecursoJudicial
{
    Causa,
    Diligencia,
    Documento,
    Resolucion,
    Recibo
}

/// <summary>
/// Responde 404 cuando el recurso de la ruta pertenece a receptores que el usuario
/// no tiene asociados, y 400 cuando un <see cref="IAsignaReceptorRequest"/> intenta
/// asignar un receptor ajeno.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AccesoPorReceptorAttribute(RecursoJudicial recurso, string parametro = "id")
    : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var acceso = context.HttpContext.RequestServices.GetRequiredService<AccesoReceptores>();
        var ct = context.HttpContext.RequestAborted;

        Guid? id = context.ActionArguments.TryGetValue(parametro, out var valor) && valor is Guid guid
            ? guid
            : null;

        if (id is Guid recursoId && !await acceso.PuedeVerAsync(recurso, recursoId, ct))
        {
            context.Result = new NotFoundObjectResult(new { mensaje = "El registro no existe o no está asociado a sus receptores." });
            return;
        }

        foreach (var solicitud in context.ActionArguments.Values.OfType<IAsignaReceptorRequest>())
        {
            if (!await acceso.PuedeAsignarAsync(solicitud.ReceptorJudicial, recurso, id, ct))
            {
                context.Result = new BadRequestObjectResult(new { mensaje = "Solo puede asignar receptores asociados a su usuario." });
                return;
            }
        }

        await next();
    }
}
