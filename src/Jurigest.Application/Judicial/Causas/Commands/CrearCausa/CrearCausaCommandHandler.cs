using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.CrearCausa;

public sealed class CrearCausaCommandHandler
    : IRequestHandler<CrearCausaCommand, CrearCausaResponse>
{
    private readonly ICausaRepository _causaRepository;

    private readonly ITipoCausaCatalogoRepository
        _tipoCausaRepository;

    public CrearCausaCommandHandler(
        ICausaRepository causaRepository,
        ITipoCausaCatalogoRepository tipoCausaRepository)
    {
        _causaRepository =
            causaRepository;

        _tipoCausaRepository =
            tipoCausaRepository;
    }

    public async Task<CrearCausaResponse> Handle(
        CrearCausaCommand request,
        CancellationToken cancellationToken)
    {
        Causa causa;

        if (request.TipoCausaId.HasValue)
        {
            if (request.TipoCausaId.Value == Guid.Empty)
            {
                throw new ArgumentException(
                    "El tipo de causa no es válido.");
            }

            if (string.IsNullOrWhiteSpace(
                    request.NumeroRol))
            {
                throw new ArgumentException(
                    "Debe indicar el número de ROL.");
            }

            var tipoCausa =
                await _tipoCausaRepository.GetByIdAsync(
                    request.TipoCausaId.Value,
                    cancellationToken);

            if (tipoCausa is null)
            {
                throw new InvalidOperationException(
                    "El tipo de causa seleccionado no existe o está inactivo.");
            }

            var rit =
                $"{tipoCausa.Codigo}-{request.NumeroRol.Trim()}";

            causa = new Causa(
                rit,
                request.Tribunal,
                request.Descripcion,
                request.FechaEncargoCausa);

            causa.AsignarTipoCausa(
                tipoCausa.Id,
                request.NumeroRol,
                tipoCausa.Codigo);
        }
        else
        {
            // Compatibilidad temporal con llamadas antiguas.
            if (string.IsNullOrWhiteSpace(request.Rit))
            {
                throw new ArgumentException(
                    "Debe indicar el RIT o seleccionar un tipo de causa.");
            }

            causa = new Causa(
                request.Rit,
                request.Tribunal,
                request.Descripcion,
                request.FechaEncargoCausa);
        }

        await _causaRepository.AddAsync(
            causa,
            cancellationToken);

        return new CrearCausaResponse(
            causa.Id,
            true,
            "La causa fue creada correctamente.");
    }
}