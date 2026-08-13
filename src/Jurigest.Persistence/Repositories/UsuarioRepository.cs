using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly JurigestDbContext _context;

    public UsuarioRepository(JurigestDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Usuario usuario,
        CancellationToken cancellationToken)
    {
        await _context.Usuarios.AddAsync(
            usuario,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Usuario?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var emailNormalizado =
            email.Trim().ToLowerInvariant();

        return await _context.Usuarios
            .FirstOrDefaultAsync(
                usuario =>
                    usuario.Email == emailNormalizado,
                cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var emailNormalizado =
            email.Trim().ToLowerInvariant();

        return await _context.Usuarios.AnyAsync(
            usuario =>
                usuario.Email == emailNormalizado,
            cancellationToken);
    }

    public Task<bool> AnyAsync(
        CancellationToken cancellationToken)
    {
        return _context.Usuarios.AnyAsync(
            cancellationToken);
    }
}