using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Seguridad.Queries.ObtenerUsuarios;
using MediatR;

namespace Jurigest.Application.Seguridad.Queries.ObtenerUsuario;

public sealed class ObtenerUsuarioHandler
    : IRequestHandler<
        ObtenerUsuarioQuery,
        ObtenerUsuariosResponse?>
{
    private readonly IUsuarioRepository _repository;

    public ObtenerUsuarioHandler(
        IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<ObtenerUsuariosResponse?> Handle(
        ObtenerUsuarioQuery request,
        CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (usuario is null)
            return null;

        var receptores = await _repository.GetReceptoresAsignadosAsync(
            [usuario.Id],
            cancellationToken);

        return new ObtenerUsuariosResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            usuario.Rut,
            usuario.Telefono,
            usuario.Direccion,
            usuario.NumeroOficina,
            usuario.NombreRolPersonalizado ?? usuario.Rol.ToString(),
            usuario.Activo,
            usuario.FechaCreacion,
            receptores
                .Select(x => new ReceptorAsignadoResponse(x.ReceptorId, x.Nombre))
                .ToList());
    }
}
