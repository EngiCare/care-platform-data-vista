// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.

// Minimal example: .NET MVC frontend that authenticates against the
// Care Platform Data VistA service and displays connection info.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(o => o.IdleTimeout = TimeSpan.FromMinutes(20));

// Register a typed HttpClient pointing at the Data API.
// Default is the C# service on port 5001; change in appsettings.json.
builder.Services.AddHttpClient("DataApi", client =>
{
    var baseUrl = builder.Configuration["DataApi:BaseUrl"] ?? "https://localhost:5001";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Allow self-signed certs in development
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

var app = builder.Build();

app.UseRouting();
app.UseSession();
app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
