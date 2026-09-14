using System.Data;
using Jurigest.Application.Judicial.Causas.Commands.ActualizarCausa;
using Jurigest.Application.Judicial.Diligencias.Commands.ActualizarDiligencia;
using Jurigest.Application.Judicial.Diligencias.Commands.CambiarTipoDiligencia;
using Jurigest.Application.Judicial.Diligencias.Commands.CrearDiligencia;
using Jurigest.Application.Judicial.Diligencias.Commands.ProgramarDiligencia;
using Jurigest.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence;

// Mantiene estable la colección de diligencias entre la validación y el guardado.
// La creación participa para que dos solicitudes concurrentes no eludan el bloqueo.
public sealed class CorreccionIngresoTransactionBehavior<TRequest, TResponse>(JurigestDbContext context)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not (Jurigest.Application.Judicial.Causas.Commands.CrearCausa.CrearCausaCommand or ActualizarCausaCommand or ActualizarDiligenciaCommand
            or CambiarTipoDiligenciaCommand or ProgramarDiligenciaCommand or CrearDiligenciaCommand)
            || !context.Database.IsRelational() || context.Database.CurrentTransaction is not null)
            return await next(cancellationToken);

        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var result = await next(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }
}