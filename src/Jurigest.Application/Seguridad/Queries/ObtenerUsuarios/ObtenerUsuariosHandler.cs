using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Seguridad.Queries.ObtenerUsuarios;

public sealed class ObtenerUsuariosHandler
    : IRequestHandler<
        ObtenerUsuariosQuery,
        List<ObtenerUsuariosResponse>>
{
    private readonly IUsuarioRepository _repository;

    public ObtenerUsuariosHandler(
        IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ObtenerUsuariosResponse>> Handle(
        ObtenerUsuariosQuery request,
        CancellationToken cancellationToken)
    {
        var usuarios = await _repository.GetAllAsync(
            cancellationToken);

        var receptores = (await _repository.GetReceptoresAsignadosAsync(
                null,
                cancellationToken))
            .ToLookup(x => x.UsuarioId);

        return usuarios
            .Select(usuario => new ObtenerUsuariosResponse(
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
                receptores[usuario.Id]
                    .Select(x => new ReceptorAsignadoResponse(x.ReceptorId, x.Nombre))
                    .ToList()))
            .ToList();
    }
}
