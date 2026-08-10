namespace Jurigest.Application.Judicial.Causas.Commands.CrearCausa;

public sealed record CrearCausaResponse(
    Guid Id,
    bool Success,
    string Message);