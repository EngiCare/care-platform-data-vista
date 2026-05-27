using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Asp.Versioning;
using CarePlatform.Data.CPRS;
using CarePlatform.Data.CPRS.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings.json (replaces legacy mdws.conf XML)
var startupLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
ECConfiguration.LoadConfiguration(builder.Configuration, startupLogger);
TokenServiceConfiguration.LoadConfiguration(builder.Configuration);

// API versioning — current API is v1, unversioned URLs default to v1
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("x-api-version"));
});

// Add controllers
builder.Services.AddControllers();

// Configure JWT authentication
var signingCert = CertificateUtil.GetCertificate(
    StoreName.My,
    StoreLocation.LocalMachine,
    TokenServiceConfiguration.SigningCertificateName);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection("JwtValidation");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = jwtSection.GetValue("ValidateIssuer", true),
            ValidIssuer = TokenServiceConfiguration.IssuerName,
            ValidateAudience = jwtSection.GetValue("ValidateAudience", true),
            ValidAudience = TokenServiceConfiguration.Realm,
            ValidateIssuerSigningKey = jwtSection.GetValue("ValidateIssuerSigningKey", true),
            IssuerSigningKey = new X509SecurityKey(signingCert),
            ValidateLifetime = jwtSection.GetValue("ValidateLifetime", true)
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");
                logger.LogWarning(context.Exception, "[JwtAuth] Authentication FAILED for {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");
                var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");
                var authHeaderPrefix = hasAuthHeader
                    ? context.Request.Headers["Authorization"].ToString()[..Math.Min(30, context.Request.Headers["Authorization"].ToString().Length)]
                    : "(none)";
                logger.LogWarning("[JwtAuth] Challenge issued for {Method} {Path} — HasAuthHeader={HasAuth}, AuthPrefix=\"{Prefix}\", Error=\"{Error}\"",
                    context.Request.Method, context.Request.Path, hasAuthHeader, authHeaderPrefix, context.Error);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");
                var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                var contextIdClaim = identity?.FindFirst("http://schemas.token.engineerecare.org/claims/ContextId");
                logger.LogInformation("[JwtAuth] Token VALIDATED for {Method} {Path} — HasContextId={HasCtx}, ContextId={CtxId}",
                    context.Request.Method, context.Request.Path,
                    contextIdClaim != null, contextIdClaim?.Value ?? "(none)");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuth");
                var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");
                logger.LogTrace("[JwtAuth] MessageReceived for {Method} {Path} — HasAuthHeader={HasAuth}",
                    context.Request.Method, context.Request.Path, hasAuthHeader);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Swagger/OpenAPI (replaces HelpPage)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Set the shared ILoggerFactory for non-DI classes
ECConfiguration.LoggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint for container probes
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Data.CPRS API" }));

app.Logger.LogInformation("CarePlatform.Data.CPRS started on .NET 10");

app.Run();
