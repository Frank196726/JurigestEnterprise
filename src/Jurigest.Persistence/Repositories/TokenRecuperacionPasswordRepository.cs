using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class TokenRecuperacionPasswordRepository
    : ITokenRecuperacionPasswordRepository
{
    private readonly JurigestDbContext _context;

    public TokenRecuperacionPasswordRepository(
        JurigestDbContext context)
    {
        _context = context;
    }

    public async Task ReplaceActiveAsync(
        TokenRecuperacionPassword token,
        DateTime fechaUtc,
        CancellationToken cancellationToken)
    {
        var anteriores =
            await _context.TokensRecuperacionPassword
                .Where(anterior =>
                    anterior.UsuarioId == token.UsuarioId &&
                    anterior.UsadoUtc == null &&
                    anterior.RevocadoUtc == null)
                .ToListAsync(cancellationToken);

        foreach (var anterior in anteriores)
        {
            anterior.Revocar(fechaUtc);
        }

        await _context.TokensRecuperacionPassword.AddAsync(
            token,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TokenRecuperacionPassword?>
        GetByHashAsync(
            string tokenHash,
            CancellationToken cancellationToken)
    {
        return await _context.TokensRecuperacionPassword
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task<bool> CompleteAsync(
    TokenRecuperacionPassword token,
    Usuario usuario,
    CancellationToken cancellationToken)
    {
        _context.TokensRecuperacionPassword.Update(token);
        _context.Usuarios.Update(usuario);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
}