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

        return usuarios
            .Select(usuario => new ObtenerUsuariosResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.Rol.ToString(),
                usuario.Activo,
                usuario.FechaCreacion))
            .ToList();
    }
}