using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Recibos.Commands.MarcarPagado;

public sealed class MarcarReciboPagadoCommandHandler
    : IRequestHandler<MarcarReciboPagadoCommand, bool>
{
    private readonly IReciboRepository _reciboRepository;

    public MarcarReciboPagadoCommandHandler(
        IReciboRepository reciboRepository)
    {
        _reciboRepository = reciboRepository;
    }

    public async Task<bool> Handle(
        MarcarReciboPagadoCommand request,
        CancellationToken cancellationToken)
    {
        var recibo =
            await _reciboRepository.GetByIdAsync(
                request.ReciboId,
                cancellationToken);

        if (recibo is null)
        {
            return false;
        }

        recibo.MarcarPagado(request.FechaPago);

        await _reciboRepository.UpdateAsync(
            recibo,
            cancellationToken);

        return true;
    }
}
