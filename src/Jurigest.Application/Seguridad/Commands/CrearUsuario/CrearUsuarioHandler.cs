using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CrearUsuario;

public sealed class CrearUsuarioHandler
    : IRequestHandler<CrearUsuarioCommand, CrearUsuarioResult>
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRolCatalogoRepository _roles;
    private readonly IAuditoriaSeguridadRepository
        _auditoriaRepository;

    public CrearUsuarioHandler(
        IUsuarioRepository repository,
        IPasswordHasher passwordHasher,
        IAuditoriaSeguridadRepository auditoriaRepository,
        IRolCatalogoRepository roles)
    {
        _repository = repository;
        _roles = roles;
        _passwordHasher = passwordHasher;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<CrearUsuarioResult> Handle(
        CrearUsuarioCommand request,
        CancellationToken cancellationToken)
    {
        if (await _repository.ExistsByEmailAsync(
                request.Email,
                cancellationToken))
        {
            return new CrearUsuarioResult(
                false,
                true,
                false,
                null);
        }

        if (await _repository.ExistsByRutAsync(request.Rut, cancellationToken))
            return new CrearUsuarioResult(false, false, true, null);

        RolCatalogo? rolCatalogo = null;
        if (request.RolCatalogoId is Guid rolId)
            rolCatalogo = await _roles.GetByIdAsync(rolId, cancellationToken)
                ?? throw new ArgumentException("El rol seleccionado no existe.");

        var passwordHash =
            _passwordHasher.Hash(request.Password);

        var usuario = new Usuario(
            Guid.NewGuid(),
            request.Nombre,
            request.Email,
            passwordHash,
            rolCatalogo?.Perfil ?? request.Rol);

        usuario.ActualizarDatosPersonales(
            request.Rut,
            request.Telefono,
            request.Direccion,
            request.NumeroOficina);
        usuario.RequerirCambioPassword();

        if (rolCatalogo is not null) usuario.AsignarRolCatalogo(rolCatalogo);

        await _repository.AddAsync(
            usuario,
            cancellationToken);

        var auditoria = new AuditoriaSeguridad(
            Guid.NewGuid(),
            request.UsuarioActorId,
            "UsuarioCreado",
            usuario.Id,
            usuario.NombreRolPersonalizado is null
                ? $"Rol asignado: {usuario.Rol}."
                : $"Rol asignado: {usuario.NombreRolPersonalizado}. Perfil: {usuario.Rol}.",
            request.DireccionIp);

        await _auditoriaRepository.AddAsync(
            auditoria,
            cancellationToken);

        return new CrearUsuarioResult(
            true,
            false,
            false,
            usuario.Id);
    }
}
