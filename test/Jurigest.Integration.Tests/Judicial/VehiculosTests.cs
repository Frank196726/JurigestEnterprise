using System.Net;
using System.Net.Http.Json;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Persistence.Context;
using Jurigest.Integration.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace Jurigest.Integration.Tests.Judicial;
public sealed class VehiculosTests
{
    [Fact]
    public async Task Vehiculo_SeCreaEditaListaYEliminaDelEncargo()
    {
        await using var factory = new JurigestApiFactory(); using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var causa = new Causa("C-222-26", "1°", "Banco / Torres"); var diligencia = causa.AgregarDiligencia("Embargo vehículo");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>(); db.Causas.Add(causa); await db.SaveChangesAsync();
        }
        var ruta = $"/api/diligencias/{diligencia.Id}/vehiculos";
        var datos = new VehiculoEncargo { Nombre = "Persona", Direccion = "Calle, Comuna", Patente = "ts0267-3", Marca = "Honda", Modelo = "SC125", Color = "Negro", TipoPropietario = 1, TipoVehiculo = "Moto", Serie = "VIN-123",
            FechaDocumento = new DateTime(2026, 10, 2), TipoAdquisicion = "Resolución judicial", AlzamientoProhibicion = "Ninguno",
            LimitacionDominio = "Embargo", LugarSolicitud = "Santiago", NumeroSolicitud = "123-26", TipoDocumento = "Resolución",
            DerechosInscripcion = 6140m, RutTitular = "19995588-8" };
        using var creada = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, ruta, admin.Token, datos);
        Assert.Equal(HttpStatusCode.OK, creada.StatusCode);
        var vehiculo = await creada.Content.ReadFromJsonAsync<VehiculoEncargo>();
        Assert.Equal("TS0267-3", vehiculo!.Patente);
        Assert.Equal("VIN-123", vehiculo.Serie); Assert.Equal("Moto", vehiculo.TipoVehiculo);
        Assert.Equal("123-26", vehiculo.NumeroSolicitud); Assert.Equal(6140m, vehiculo.DerechosInscripcion);
        Assert.Equal(new DateTime(2026, 10, 2), vehiculo.FechaDocumento);
        using var duplicada = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, ruta, admin.Token, datos);
        Assert.Equal(HttpStatusCode.Conflict, duplicada.StatusCode);
        datos.Color = "Blanco";
        using var editada = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put, $"{ruta}/{vehiculo.Id}", admin.Token, datos);
        Assert.Equal(HttpStatusCode.OK, editada.StatusCode);
        using var listado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, ruta, admin.Token);
        var lista = await listado.Content.ReadFromJsonAsync<VehiculoEncargo[]>();
        var guardado = Assert.Single(lista!); Assert.Equal("Blanco", guardado.Color);
        Assert.Equal("Embargo", guardado.LimitacionDominio); Assert.Equal("Resolución", guardado.TipoDocumento);
        Assert.Equal("19995588-8", guardado.RutTitular);
        datos.DerechosInscripcion = -1;
        using var invalido = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put, $"{ruta}/{vehiculo.Id}", admin.Token, datos);
        Assert.Equal(HttpStatusCode.BadRequest, invalido.StatusCode);
        using var eliminada = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Delete, $"{ruta}/{vehiculo.Id}", admin.Token);
        Assert.Equal(HttpStatusCode.NoContent, eliminada.StatusCode);
        using var vacio = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, ruta, admin.Token);
        Assert.Empty((await vacio.Content.ReadFromJsonAsync<VehiculoEncargo[]>())!);
    }
}
