using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jurigest.Persistence.Context;

public sealed class JurigestDbContextFactory
    : IDesignTimeDbContextFactory<JurigestDbContext>
{
    public JurigestDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<JurigestDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost\\SQLEXPRESS;Database=JurigestEnterprise;Trusted_Connection=True;TrustServerCertificate=True;");

        return new JurigestDbContext(optionsBuilder.Options);
    }
}