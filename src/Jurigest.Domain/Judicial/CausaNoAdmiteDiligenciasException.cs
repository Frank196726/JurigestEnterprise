namespace Jurigest.Domain.Judicial;

public sealed class CausaNoAdmiteDiligenciasException(string estado) : InvalidOperationException(
    $"La causa está {estado}: no admite nuevas diligencias ni registro de resultados. " +
    "Cambie su estado para continuar.");
