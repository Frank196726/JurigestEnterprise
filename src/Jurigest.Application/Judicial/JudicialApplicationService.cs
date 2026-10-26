using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;
using Jurigest.Domain.Judicial.ValueObjects;

namespace Jurigest.Application.Judicial;

public sealed class JudicialApplicationService
{
    private readonly ICausaRepository _causaRepository;
    private readonly IEncargoRepository _encargoRepository;

    public JudicialApplicationService(ICausaRepository causaRepository, IEncargoRepository encargoRepository)
    {
        _causaRepository = causaRepository;
        _encargoRepository = encargoRepository;
    }

    public async Task<IReadOnlyCollection<Causa>> ObtenerCausasAsync(CancellationToken cancellationToken = default)
        => await _causaRepository.GetAllAsync(cancellationToken);

    public async Task<Causa?> ObtenerCausaAsync(Guid id, CancellationToken cancellationToken = default)
        => await _causaRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Causa> CrearCausaAsync(string rit, CancellationToken cancellationToken = default)
    {
        var causa = new Causa(Guid.NewGuid(), new RolUnicoTribunal(rit));
        await _causaRepository.AddAsync(causa, cancellationToken);
        return causa;
    }

    public async Task<IReadOnlyCollection<Encargo>> ObtenerEncargosAsync(CancellationToken cancellationToken = default)
        => await _encargoRepository.GetAllAsync(cancellationToken);

    public async Task<Encargo?> ObtenerEncargoAsync(Guid id, CancellationToken cancellationToken = default)
        => await _encargoRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Encargo> CrearEncargoAsync(string descripcion, CancellationToken cancellationToken = default)
    {
        var encargo = new Encargo(Guid.NewGuid(), descripcion);
        await _encargoRepository.AddAsync(encargo, cancellationToken);
        return encargo;
    }

    public async Task<Causa> AsignarEncargoAsync(Guid causaId, Guid encargoId, CancellationToken cancellationToken = default)
    {
        var causa = await _causaRepository.GetByIdAsync(causaId, cancellationToken)
            ?? throw new InvalidOperationException("La causa no existe.");

        var encargo = await _encargoRepository.GetByIdAsync(encargoId, cancellationToken)
            ?? throw new InvalidOperationException("El encargo no existe.");

        causa.AsignarEncargo(encargo);
        await _causaRepository.UpdateAsync(causa, cancellationToken);
        return causa;
    }

    public async Task<Encargo> AgregarDiligenciaAsync(Guid encargoId, string descripcion, CancellationToken cancellationToken = default)
    {
        var encargo = await _encargoRepository.GetByIdAsync(encargoId, cancellationToken)
            ?? throw new InvalidOperationException("El encargo no existe.");

        encargo.AgregarDiligencia(new Diligencia(Guid.NewGuid(), descripcion));
        await _encargoRepository.UpdateAsync(encargo, cancellationToken);
        return encargo;
    }

    public async Task<Encargo> AgregarDocumentoAsync(Guid encargoId, string nombre, TipoDocumento tipo, string rutaArchivo, CancellationToken cancellationToken = default)
    {
        var encargo = await _encargoRepository.GetByIdAsync(encargoId, cancellationToken)
            ?? throw new InvalidOperationException("El encargo no existe.");

        encargo.AgregarDocumento(new Documento(Guid.NewGuid(), nombre, tipo, rutaArchivo));
        await _encargoRepository.UpdateAsync(encargo, cancellationToken);
        return encargo;
    }

    public async Task<Encargo> RegistrarResolucionAsync(Guid encargoId, TipoResolucion tipo, DateTime fecha, string descripcion, CancellationToken cancellationToken = default)
    {
        var encargo = await _encargoRepository.GetByIdAsync(encargoId, cancellationToken)
            ?? throw new InvalidOperationException("El encargo no existe.");

        encargo.RegistrarResolucion(new Resolucion(Guid.NewGuid(), tipo, fecha, descripcion));
        await _encargoRepository.UpdateAsync(encargo, cancellationToken);
        return encargo;
    }
}
