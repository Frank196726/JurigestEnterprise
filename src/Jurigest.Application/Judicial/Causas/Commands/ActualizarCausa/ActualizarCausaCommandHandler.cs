using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.ActualizarCausa;

public sealed class ActualizarCausaCommandHandler
    : IRequestHandler<ActualizarCausaCommand, ActualizarCausaResponse?>
{
    private readonly ICausaRepository _repository;
    private readonly IDiligenciaEncargadaCatalogoRepository _catalogo;

    public ActualizarCausaCommandHandler(
        ICausaRepository repository, IDiligenciaEncargadaCatalogoRepository catalogo)
    {
        _repository = repository;
        _catalogo = catalogo;
    }


    public async Task<ActualizarCausaResponse?> Handle(
        ActualizarCausaCommand request,
        CancellationToken cancellationToken)
    {
        var causa = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (causa is null)
            return null;


        causa.ValidarCorreccionIngreso();
        string? nombre = null;
        Jurigest.Domain.Judicial.Enums.TipoDiligencia? tipo = null;
        if (request.DiligenciaEncargadaId.HasValue)
        {
            var catalogo = await _catalogo.GetActivosAsync(cancellationToken);
            var seleccion = catalogo.SingleOrDefault(d => d.Id == request.DiligenciaEncargadaId.Value)
                ?? throw new ArgumentException("La diligencia encargada seleccionada no está disponible.");
            nombre = seleccion.Nombre;
            tipo = (Jurigest.Domain.Judicial.Enums.TipoDiligencia)(seleccion.CodigoTipoDiligencia
                ?? (int)Jurigest.Domain.Judicial.Enums.TipoDiligencia.Otro);
        }
        else if (request.DiligenciaId.HasValue)
        {
            // Conservar encargos históricos que ya no estén en el catálogo activo.
            nombre = causa.PrimeraDiligencia?.Descripcion;
        }

	if (request.FechaEncargoCausa.HasValue)
	{
    		causa.ActualizarFechaEncargo(
        		request.FechaEncargoCausa.Value);
	}

        causa.CorregirIngreso(request.Tribunal, request.Descripcion,
            request.DiligenciaId, nombre, tipo, request.FechaProgramada);


        await _repository.SaveChangesAsync(cancellationToken);


        return new ActualizarCausaResponse(
            causa.Id,
            "La causa fue actualizada correctamente.");
    }
}