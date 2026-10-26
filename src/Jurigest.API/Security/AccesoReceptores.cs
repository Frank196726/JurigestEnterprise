using Jurigest.Application.Judicial;
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

        var nombres = await db.UsuariosReceptores.AsNoTracking()
            .Where(x => x.UsuarioId == id)
            .Join(db.ReceptoresJudiciales, a => a.ReceptorJudicialId, r => r.Id, (a, r) => r.Nombre)
            .ToListAsync(ct);

        // Compatibilidad: el rol "Receptor" ya filtraba por el nombre del propio usuario.
        if (string.Equals(usuario.NombreRolPersonalizado, "Receptor", StringComparison.OrdinalIgnoreCase))
        {
            _receptorPropio = usuario.Nombre;
            nombres.Add(usuario.Nombre);
        }

        _filtro = nombres.Count == 0 ? null : new FiltroReceptores(nombres);
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

        var receptores = await db.Diligencias.AsNoTracking()
            .Where(d => d.CausaId == causaId)
            .Select(d => d.ReceptorJudicial)
            .ToListAsync(ct);
        return filtro.PermiteCausa(receptores);
    }

    public async Task<bool> PuedeVerDiligenciaAsync(Guid diligenciaId, CancellationToken ct)
    {
        var filtro = await ObtenerFiltroAsync(ct);
        if (filtro is null) return true;

        var diligencia = await db.Diligencias.AsNoTracking()
            .Where(d => d.Id == diligenciaId)
            .Select(d => new { d.CausaId, d.ReceptorJudicial })
            .SingleOrDefaultAsync(ct);
        if (diligencia is null) return true; // El endpoint responde que no existe.

        return filtro.PermiteDiligencia(
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
    /// </summary>
    public async Task<bool> PuedeAsignarAsync(string? receptor, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(receptor)) return true;
        var filtro = await ObtenerFiltroAsync(ct);
        return filtro is null || filtro.EsPropio(receptor);
    }
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
/// no tiene asociados, y 400 cuando el cuerpo intenta asignar un receptor ajeno
/// (cualquier argumento con una propiedad <c>ReceptorJudicial</c>).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AccesoPorReceptorAttribute(RecursoJudicial recurso, string parametro = "id")
    : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var acceso = context.HttpContext.RequestServices.GetRequiredService<AccesoReceptores>();
        var ct = context.HttpContext.RequestAborted;

        if (context.ActionArguments.TryGetValue(parametro, out var valor) && valor is Guid id &&
            !await acceso.PuedeVerAsync(recurso, id, ct))
        {
            context.Result = new NotFoundObjectResult(new { mensaje = "El registro no existe o no está asociado a sus receptores." });
            return;
        }

        foreach (var argumento in context.ActionArguments.Values)
        {
            var receptor = argumento?.GetType().GetProperty("ReceptorJudicial")?.GetValue(argumento) as string;
            if (!await acceso.PuedeAsignarAsync(receptor, ct))
            {
                context.Result = new BadRequestObjectResult(new { mensaje = "Solo puede asignar receptores asociados a su usuario." });
                return;
            }
        }

        await next();
    }
}
