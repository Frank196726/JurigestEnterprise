using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Domain.Judicial.Enums;
using Jurigest.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class FlujoCausaDiligenciaResultadoTests
{
    [Fact]
    public async Task RegistrarResultado_SinAutenticacion_Devuelve401()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PutAsJsonAsync(
            $"/api/Diligencias/{Guid.NewGuid()}/resultado",
            new { resultado = 1, resultadoDetalle = "Detalle", estampe = "Estampe", fechaGestion = DateTime.UtcNow });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarResultado_SinDetalle_Devuelve400()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Diligencias/{Guid.NewGuid()}/resultado", admin.Token,
            new { diligenciaRealizadaId = Guid.NewGuid(), resultado = 1, resultadoDetalle = "", estampe = "Estampe", fechaGestion = DateTime.UtcNow });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarResultado_SinDiligenciaRealizada_Devuelve400()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();

        await SeguridadTestHelper.CrearAdministradorAsync(client);

        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client,
            SeguridadTestHelper.AdminEmail,
            SeguridadTestHelper.AdminPassword);

        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Put,
            $"/api/Diligencias/{Guid.NewGuid()}/resultado",
            admin.Token,
            new
            {
                diligenciaRealizadaId = Guid.Empty,
                resultado = 1,
                resultadoDetalle = "Detalle",
                estampe = "Estampe",
                fechaGestion = DateTime.UtcNow
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var contenido = await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Debe indicar la diligencia realizada.",
            contenido);
    }

    [Fact]
public async Task Administrador_CompletaFlujo_GeneraReciboPendientePorNotificacionPersonal()
{
    await using var factory = new JurigestApiFactory();
    using var client = factory.CreateClient();

    await SeguridadTestHelper.CrearAdministradorAsync(client);

    var admin = await SeguridadTestHelper.IniciarSesionAsync(
        client,
        SeguridadTestHelper.AdminEmail,
        SeguridadTestHelper.AdminPassword);

    var diligenciaRealizadaId = Guid.NewGuid();

    using (var scope = factory.Services.CreateScope())
    {
        var dbContext =
            scope.ServiceProvider.GetRequiredService<JurigestDbContext>();

        var diligenciaRealizada =
            new DiligenciaRealizadaCatalogo(
                diligenciaRealizadaId,
                "NotificaciÃ³n personal",
                (int)TipoDiligencia.Notificacion);

        diligenciaRealizada.CambiarArancel(60000m);

        dbContext.DiligenciasRealizadas.Add(diligenciaRealizada);

        await dbContext.SaveChangesAsync();
    }

    var fechaEncargo =
        new DateTime(
            2026,
            8,
            1,
            9,
            0,
            0,
            DateTimeKind.Utc);

    var fechaGestion = fechaEncargo.AddDays(4);

    using var crearCausa =
        await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Post,
            "/api/Causas",
            admin.Token,
            new
            {
                id = Guid.Empty,
                rit = "C-RECIBO-001-2026",
                tribunal = "Tribunal de integraciÃ³n",
                descripcion = "Prueba recibo NotificaciÃ³n personal",
                fechaEncargoCausa = fechaEncargo
            });

    Assert.Equal(
        HttpStatusCode.OK,
        crearCausa.StatusCode);

    var causaId =
        await LeerGuidAsync(
            crearCausa,
            "id");

    using var crearDiligencia =
        await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Post,
            $"/api/Causas/{causaId}/diligencias",
            admin.Token,
            new
            {
                descripcion = "NotificaciÃ³n de demanda",
                tipo = (int)TipoDiligencia.Notificacion
            });

    Assert.Equal(
        HttpStatusCode.OK,
        crearDiligencia.StatusCode);

    var diligenciaId =
        await LeerGuidAsync(
            crearDiligencia,
            "id");

    using var registrarResultado =
        await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Put,
            $"/api/Diligencias/{diligenciaId}/resultado",
            admin.Token,
            new
            {
                diligenciaRealizadaId,
                resultado = 1,
                resultadoDetalle =
                    "NotificaciÃ³n personal realizada",
                estampe =
                    "ESTAMPE-PRUEBA-RECIBO-60000",
                fechaGestion
            });

    Assert.Equal(
        HttpStatusCode.OK,
        registrarResultado.StatusCode);

    using var obtenerCausas =
        await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Get,
            "/api/Causas",
            admin.Token);

    obtenerCausas.EnsureSuccessStatusCode();

    using var documento =
        JsonDocument.Parse(
            await obtenerCausas.Content.ReadAsStringAsync());

    var causa =
        documento.RootElement
            .EnumerateArray()
            .Single(
                elemento =>
                    elemento.GetProperty("id").GetGuid()
                    == causaId);

    Assert.Equal(
        fechaGestion,
        causa.GetProperty(
            "fechaGestionCausa").GetDateTime());

    var diasEsperados =
        Math.Max(
            0,
            (DateTime.UtcNow.Date -
             fechaGestion.Date).Days);

    Assert.Equal(
        diasEsperados,
        causa.GetProperty(
            "diasSinGestion").GetInt32());

    await using var verificacion =
        factory.Services.CreateAsyncScope();

    var dbVerificacion =
        verificacion.ServiceProvider
            .GetRequiredService<JurigestDbContext>();

    var recibos =
        dbVerificacion.Recibos
            .Where(
                recibo =>
                    recibo.DiligenciaId ==
                    diligenciaId)
            .ToList();

    var recibo = Assert.Single(recibos);

    Assert.Equal(
        causaId,
        recibo.CausaId);

    Assert.Equal(
        diligenciaId,
        recibo.DiligenciaId);

    Assert.Equal(
        diligenciaRealizadaId,
        recibo.DiligenciaRealizadaId);

    Assert.Equal(
        "NotificaciÃ³n personal",
        recibo.DiligenciaRealizada);

    Assert.Equal(
        60000m,
        recibo.Monto);

    Assert.Equal(
        EstadoRecibo.Pendiente,
        recibo.Estado);

    Assert.Equal(
        fechaGestion,
        recibo.FechaEmision);

    Assert.Null(
        recibo.FechaPago);

    var reciboId = recibo.Id;
    
    using var obtenerRecibos =
        await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Get,
            "/api/Recibos",
            admin.Token);
    
    Assert.Equal(
        HttpStatusCode.OK,
        obtenerRecibos.StatusCode);
    
    using var documentoRecibos =
        JsonDocument.Parse(
            await obtenerRecibos.Content.ReadAsStringAsync());
    
    var reciboListado =
        documentoRecibos.RootElement
            .EnumerateArray()
            .Single(
                elemento =>
                    elemento.GetProperty("id").GetGuid()
                    == reciboId);
    
    Assert.Equal(
        60000m,
        reciboListado.GetProperty("monto").GetDecimal());
    
    Assert.Equal(
        (int)EstadoRecibo.Pendiente,
        reciboListado.GetProperty("estado").GetInt32());
    
    using var obtenerRecibo =
        await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Get,
            $"/api/Recibos/{reciboId}",
            admin.Token);
    
    Assert.Equal(
        HttpStatusCode.OK,
        obtenerRecibo.StatusCode);
    
    using var documentoRecibo =
        JsonDocument.Parse(
            await obtenerRecibo.Content.ReadAsStringAsync());
    
    Assert.Equal(
        reciboId,
        documentoRecibo.RootElement
            .GetProperty("id")
            .GetGuid());
    
    Assert.Equal(
        60000m,
        documentoRecibo.RootElement
            .GetProperty("monto")
            .GetDecimal());
    
    Assert.Equal(
        (int)EstadoRecibo.Pendiente,
        documentoRecibo.RootElement
            .GetProperty("estado")
            .GetInt32());
    
    var fechaPago =
        fechaGestion.AddDays(1);
    
    using var pagarRecibo =
        await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Put,
            $"/api/Recibos/{reciboId}/pagar",
            admin.Token,
            new
            {
                fechaPago
            });
    
    Assert.Equal(
        HttpStatusCode.OK,
        pagarRecibo.StatusCode);
    
    using var obtenerReciboPagado =
        await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Get,
            $"/api/Recibos/{reciboId}",
            admin.Token);
    
    Assert.Equal(
        HttpStatusCode.OK,
        obtenerReciboPagado.StatusCode);
    
    using var documentoReciboPagado =
        JsonDocument.Parse(
            await obtenerReciboPagado.Content.ReadAsStringAsync());
    
    var reciboPagado =
        documentoReciboPagado.RootElement;
    
    Assert.Equal(
        reciboId,
        reciboPagado.GetProperty("id").GetGuid());
    
    Assert.Equal(
        causaId,
        reciboPagado.GetProperty("causaId").GetGuid());
    
    Assert.Equal(
        diligenciaId,
        reciboPagado.GetProperty("diligenciaId").GetGuid());
    
    Assert.Equal(
        diligenciaRealizadaId,
        reciboPagado
            .GetProperty("diligenciaRealizadaId")
            .GetGuid());
    
    Assert.Equal(
        60000m,
        reciboPagado.GetProperty("monto").GetDecimal());
    
    Assert.Equal(
        (int)EstadoRecibo.Pagado,
        reciboPagado.GetProperty("estado").GetInt32());
    
    Assert.Equal(
        fechaGestion,
        reciboPagado
            .GetProperty("fechaEmision")
            .GetDateTime());
    
    Assert.Equal(
        fechaPago,
        reciboPagado
            .GetProperty("fechaPago")
            .GetDateTime());
    
    await using var verificacionPago =
        factory.Services.CreateAsyncScope();
    
    var dbPago =
        verificacionPago.ServiceProvider
            .GetRequiredService<JurigestDbContext>();
    
    var recibosPersistidos =
        dbPago.Recibos
            .Where(
                item =>
                    item.DiligenciaId == diligenciaId)
            .ToList();
    
    var reciboPersistido =
        Assert.Single(recibosPersistidos);
    
    Assert.Equal(
        reciboId,
        reciboPersistido.Id);
    
    Assert.Equal(
        causaId,
        reciboPersistido.CausaId);
    
    Assert.Equal(
        diligenciaId,
        reciboPersistido.DiligenciaId);
    
    Assert.Equal(
        diligenciaRealizadaId,
        reciboPersistido.DiligenciaRealizadaId);
    
    Assert.Equal(
        60000m,
        reciboPersistido.Monto);
    
    Assert.Equal(
        EstadoRecibo.Pagado,
        reciboPersistido.Estado);
    
    Assert.Equal(
        fechaGestion,
        reciboPersistido.FechaEmision);
    
    Assert.Equal(
        fechaPago,
        reciboPersistido.FechaPago);
    }
    

    private static async Task<Guid> LeerGuidAsync(HttpResponseMessage response, string propiedad)
    {
        using var documento = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return documento.RootElement.GetProperty(propiedad).GetGuid();
    }

}
