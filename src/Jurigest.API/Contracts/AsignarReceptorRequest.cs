namespace Jurigest.API.Contracts;

public sealed class AsignarReceptorRequest : IAsignaReceptorRequest
{
    public string ReceptorJudicial { get; set; } = string.Empty;

    string? IAsignaReceptorRequest.ReceptorJudicial => ReceptorJudicial;
}
