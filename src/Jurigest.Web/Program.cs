using Jurigest.Web.Components;
using Jurigest.Web.Security;
using Jurigest.Web.Endpoints;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<
    ISesionWebStore,
    MemorySesionWebStore>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(
        SesionAuthenticationHandler.SchemeName)
    .AddScheme<
        AuthenticationSchemeOptions,
        SesionAuthenticationHandler>(
            SesionAuthenticationHandler.SchemeName,
            options => { });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<
    AuthenticationStateProvider,
    SesionAuthenticationStateProvider>();

builder.Services.AddScoped<
    IJurigestApiClient,
    JurigestApiClient>();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException(
        "Falta la configuración Api:BaseUrl.");

builder.Services.AddHttpClient(
    "JurigestApi",
    client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapSeguridadWebEndpoints();

app.MapDocumentosWebEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
