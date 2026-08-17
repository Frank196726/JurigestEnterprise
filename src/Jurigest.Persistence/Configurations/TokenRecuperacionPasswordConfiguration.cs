using Jurigest.Domain.Seguridad.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class TokenRecuperacionPasswordConfiguration
    : IEntityTypeConfiguration<TokenRecuperacionPassword>
{
    public void Configure(
        EntityTypeBuilder<TokenRecuperacionPassword> builder)
    {
        builder.ToTable("TokensRecuperacionPassword");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(token => token.FechaCreacionUtc)
            .IsRequired();

        builder.Property(token => token.ExpiraUtc)
            .IsRequired();

        builder.Property(token => token.RowVersion)
            .IsRowVersion();

        builder.HasIndex(token => token.TokenHash)
            .IsUnique();

        builder.HasIndex(token => new
        {
            token.UsuarioId,
            token.ExpiraUtc
        });

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(token => token.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}