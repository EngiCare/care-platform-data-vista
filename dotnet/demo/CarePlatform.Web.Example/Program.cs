// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.

// Minimal example: .NET MVC frontend that authenticates directly against
// a VistA RPC broker (Mock VistA) and displays connection info.

using CarePlatform.Web.Example.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(o => o.IdleTimeout = TimeSpan.FromMinutes(20));
builder.Services.AddSingleton<VistaRpcClient>();

var app = builder.Build();

app.UseRouting();
app.UseSession();
app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
