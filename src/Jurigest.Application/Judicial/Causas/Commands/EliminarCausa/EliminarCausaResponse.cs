namespace Jurigest.Application.Judicial.Causas.Commands.EliminarCausa;

public sealed record EliminarCausaResponse(
    Guid Id,
    string Mensaje);