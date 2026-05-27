// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Security.VA.STSv2.Configuration;
using CarePlatform.Security.VA.STSv2.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Bind STS / SSO settings (the OBO downstream + SOAP STS endpoint)
builder.Services.Configure<StsOptions>(builder.Configuration.GetSection("Sts"));
builder.Services.Configure<SsoConfigOptions>(builder.Configuration.GetSection("SsoConfig"));

// Microsoft.Identity.Web wires up OpenID Connect + Cookie auth + token acquisition
// (PKCE is on by default). Token cache is in-memory; swap for distributed in production.
var stsScope = builder.Configuration["Sts:Scope"];
var initialScopes = string.IsNullOrEmpty(stsScope)
    ? System.Array.Empty<string>()
    : new[] { stsScope };

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddInMemoryTokenCaches();

// Preserve the legacy "id_token" claim that the FX project added in
// SecurityTokenValidated, since downstream code/views may still look for it.
builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.SaveTokens = true;
    options.TokenValidationParameters ??= new TokenValidationParameters();
    options.TokenValidationParameters.NameClaimType = "name";

    var existingOnTokenValidated = options.Events.OnTokenValidated;
    options.Events.OnTokenValidated = async ctx =>
    {
        if (existingOnTokenValidated is not null)
        {
            await existingOnTokenValidated(ctx);
        }

        var idToken = ctx.ProtocolMessage?.IdToken;
        if (!string.IsNullOrEmpty(idToken) && ctx.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
        {
            if (!identity.HasClaim(c => c.Type == "id_token"))
            {
                identity.AddClaim(new System.Security.Claims.Claim("id_token", idToken));
            }
        }
    };
});

// Application services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<OboTokenService>();
builder.Services.AddScoped<StsTokenIssuer>();
builder.Services.AddSingleton<CarePlatform.Security.VA.STSv2.Certificates.CertificateUtil>();

builder.Services
    .AddControllersWithViews()
    .AddMicrosoftIdentityUI();
builder.Services.AddRazorPages();

// When IIS reverse-proxies via ANCM, X-Forwarded-Proto / X-Forwarded-For tell us
// the real client scheme/host. Required so OIDC redirect URIs are built as https.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
