using System.Text;
using Jurigest.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Jurigest.Application.Abstractions.Notifications;

namespace Jurigest.Integration.Tests.Infrastructure;

public sealed class JurigestApiFactory
    : WebApplicationFactory<Program>
{
    private const string JwtIssuer =
        "Jurigest.Tests";

    private const string JwtAudience =
        "Jurigest.Tests.Client";

    private const string JwtKey =
        "Jurigest.Tests.Key.0123456789.abcdefghijklmnopqrstuvwxyz.ABCDEFGHIJ";

    private readonly string _databaseName =
        $"JurigestTests-{Guid.NewGuid()}";

    public RecuperacionPasswordNotifierTest
        RecuperacionPasswordNotifier
    { get; } = new();

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = JwtIssuer,
                    ["Jwt:Audience"] = JwtAudience,
                    ["Jwt:Key"] = JwtKey,
                    ["Jwt:ExpirationMinutes"] = "60"
                });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                IDbContextOptionsConfiguration<
                    JurigestDbContext>>();

            services.RemoveAll<IRecuperacionPasswordNotifier>();

            services.AddSingleton<
                IRecuperacionPasswordNotifier>(
                RecuperacionPasswordNotifier);

            services.RemoveAll<
                DbContextOptions<JurigestDbContext>>();

            services.RemoveAll<JurigestDbContext>();

            services.AddDbContext<JurigestDbContext>(
                options => options.UseInMemoryDatabase(
                    _databaseName));

            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters.ValidIssuer =
                        JwtIssuer;

                    options.TokenValidationParameters.ValidAudience =
                        JwtAudience;

                    options.TokenValidationParameters
                        .IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(JwtKey));
                });
        });
    }
}