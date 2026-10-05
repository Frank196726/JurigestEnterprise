using System.Net;
using System.Net.Http.Json;
using Jurigest.Domain.Judicial;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Jurigest.Web.Services.Estampes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class MontoGestionTests
{
    [Fact]
    public async Task ReciboConAdicionales_ConservaDesgloseYDatosEnApi()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var causa = new Causa("C-9087-26", "13°", "Banco / Morales", DateTime.UtcNow.AddDays(-1));
        var diligencia = causa.AgregarDiligencia("Embargo vehículo");
        var gestion = new DiligenciaRealizadaCatalogo(Guid.NewGuid(), "Embargo vehículo", (int)diligencia.Tipo);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            db.Causas.Add(causa); db.DiligenciasRealizadas.Add(gestion); await db.SaveChangesAsync();
        }
        var datos = new { diligenciaRealizadaId = gestion.Id, resultado = 1, fechaGestion = DateTime.UtcNow,
            estampe = "Texto", monto = 30000, totalAdicionales = 27000, abogado = "Abogada",
            numeroOperacion = "OP-123", cuantia = 100000, observacionRecibo = "Not. Rvm.", detalleAdicionales = "Traslado" };
        for (var i = 0; i < 2; i++)
        {
            using var guardado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
                $"/api/Diligencias/{diligencia.Id}/resultado", admin.Token, datos);
            Assert.Equal(HttpStatusCode.OK, guardado.StatusCode);
        }
        using var lectura = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Recibos", admin.Token);
        lectura.EnsureSuccessStatusCode();
        var recibos = await lectura.Content.ReadFromJsonAsync<System.Text.Json.JsonElement[]>();
        var recibo = Assert.Single(recibos!);
        Assert.Equal(57000m, recibo.GetProperty("monto").GetDecimal());
        Assert.Equal(30000m, recibo.GetProperty("valorGestion").GetDecimal());
        Assert.Equal(27000m, recibo.GetProperty("totalAdicionales").GetDecimal());
        Assert.Equal("OP-123", recibo.GetProperty("numeroOperacion").GetString());
        Assert.Equal("Banco / Morales", recibo.GetProperty("caratulado").GetString());
    }

    [Fact]
    public void BorradorUsaEncabezadoDelReceptorYDatosDeLaCausa()
    {
        var texto = GeneradorEstampe.Generar("1188-26", "10°", "Banco / Javier Orellana", "Embargo",
            TipoDiligencia.Embargo, "Embargo Tesorería", ResultadoDiligencia.Positiva, "",
            new DateTime(2026, 10, 1, 1, 0, 0), receptorJudicial: "Marcela Vargas",
            modelo: "CERTIFICO: Demandado RUT $rut_ejecutado.", monto: 30000m,
            abogado: "Abogada", rutDemandado: "19995588-8", materia: "Ejecutivo");
        Assert.StartsWith("Marcela Vargas" + Environment.NewLine + "Receptor Judicial", texto);
        Assert.Contains("Tribunal: 10°", texto);
        Assert.Contains("Caratulado: Banco / Javier Orellana", texto);
        Assert.Contains("Abogado: Abogada", texto);
        Assert.Contains("Materia: Ejecutivo", texto);
        Assert.Contains("Rut Demandado: 19995588-8", texto);
        Assert.Contains("Demandado RUT 19995588-8.", texto);
        Assert.DoesNotContain("ESTAMPE DE EMBARGO", texto);
        Assert.Equal(1, texto.Split("CERTIFICO:").Length - 1);
    }

    [Theory]
    [InlineData(45000, 45000)]
    [InlineData(0, 0)]
    [InlineData(null, 60000)]
    public async Task ValorIngresadoSeGuardaEnReciboYEstampeSinDuplicados(int? ingresado, int esperado)
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var causa = new Causa("C-901-2026", "Tribunal", "Banco / Demandado", DateTime.UtcNow.AddDays(-1));
        var diligencia = causa.AgregarDiligencia("Notificación");
        var gestion = new DiligenciaRealizadaCatalogo(Guid.NewGuid(), "Gestión con valor", (int)diligencia.Tipo);
        gestion.CambiarArancel(60000m);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            db.Causas.Add(causa); db.DiligenciasRealizadas.Add(gestion); await db.SaveChangesAsync();
        }
        async Task<HttpResponseMessage> Guardar(decimal? monto) => await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Diligencias/{diligencia.Id}/resultado", admin.Token,
            new { diligenciaRealizadaId = gestion.Id, resultado = 1, fechaGestion = DateTime.UtcNow, estampe = "Texto revisado por receptor.", monto });

        foreach (var invalido in new[] { -1m, 1.5m, 1000000000000000000m })
        {
            using var rechazado = await Guardar(invalido);
            Assert.Equal(HttpStatusCode.BadRequest, rechazado.StatusCode);
        }
        for (var i = 0; i < 2; i++)
        {
            using var resultado = await Guardar(ingresado);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
        }
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            var guardada = await db.Diligencias.SingleAsync(x => x.Id == diligencia.Id);
            Assert.Equal(MontoEstampe.Aplicar("Texto revisado por receptor.", esperado), guardada.Estampe);
            var recibos = await db.Recibos.Where(x => x.DiligenciaId == diligencia.Id).ToListAsync();
            if (esperado == 0) Assert.Empty(recibos);
            else { var recibo = Assert.Single(recibos); Assert.Equal(esperado, recibo.Monto); Assert.Equal(EstadoRecibo.Pendiente, recibo.Estado); }
        }
        if (esperado > 0)
        {
            using var cambio = await Guardar(esperado + 1);
            Assert.Equal(HttpStatusCode.Conflict, cambio.StatusCode);
            using var lectura = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
                $"/api/Recibos/diligencia/{diligencia.Id}", admin.Token);
            Assert.Equal(HttpStatusCode.OK, lectura.StatusCode);
            var recibo = await lectura.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            Assert.Equal(esperado, recibo.GetProperty("monto").GetDecimal());
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("CERTIFICO: Texto del modelo {{ROL}}.")]
    public void BorradorConModeloOPredeterminadoFinalizaConMontoSinDuplicarlo(string? modelo)
    {
        var texto = GeneradorEstampe.Generar("C-1-2026", "Tribunal", "Banco / Persona", "Notificación",
            TipoDiligencia.Notificacion, "Notificación personal", ResultadoDiligencia.Positiva, "", DateTime.Now,
            observaciones: "Observación conservada", modelo: modelo, monto: 45000);
        Assert.EndsWith("VALOR DE LA DILIGENCIA: $45.000.-", texto);
        var actualizado = MontoEstampe.Aplicar(texto, 60000);
        Assert.Contains("Observación conservada", actualizado);
        Assert.EndsWith("VALOR DE LA DILIGENCIA: $60.000.-", actualizado);
        Assert.DoesNotContain("$45.000", actualizado);
        Assert.Equal(1, actualizado.Split("VALOR DE LA DILIGENCIA:").Length - 1);
    }
}
