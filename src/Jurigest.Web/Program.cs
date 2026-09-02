using Jurigest.Web.Components;
using Jurigest.Web.Security;
using Jurigest.Web.Endpoints;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;


var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole(options => options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    var proxy = builder.Configuration["ReverseProxy:KnownProxy"];
    if (IPAddress.TryParse(proxy, out var address)) options.KnownProxies.Add(address);
});

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

if (!builder.Environment.IsDevelopment() &&
    (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri) || apiUri.Scheme != Uri.UriSchemeHttps))
{
    throw new InvalidOperationException("Api:BaseUrl debe usar HTTPS en producción.");
}

builder.Services.AddHttpClient(
    "JurigestApi",
    client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next(context);
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapSeguridadWebEndpoints();

app.MapDocumentosWebEndpoints();

app.MapReportesWebEndpoints();

app.MapOperacionWebEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
