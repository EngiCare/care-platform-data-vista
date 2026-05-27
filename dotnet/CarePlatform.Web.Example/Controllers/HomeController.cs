// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CarePlatform.Web.Example.Controllers;

/// <summary>
/// Minimal example controller that demonstrates the full auth flow
/// against the Care Platform Data VistA service.
/// </summary>
public class HomeController(IHttpClientFactory httpClientFactory) : Controller
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    /// <summary>Show login form (or redirect to dashboard if already connected).</summary>
    public async Task<IActionResult> Index()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Dashboard");

        // Fetch available sites from the data service
        var client = httpClientFactory.CreateClient("DataApi");
        try
        {
            var sites = await client.GetFromJsonAsync<List<SiteInfo>>("api/connection/sites", JsonOpts);
            ViewBag.Sites = sites ?? [];
        }
        catch (Exception ex)
        {
            ViewBag.Sites = new List<SiteInfo>();
            ViewBag.Error = $"Cannot reach data service: {ex.Message}";
        }

        return View();
    }

    /// <summary>Handle login form submission.</summary>
    [HttpPost]
    public async Task<IActionResult> Login(string siteId, string accessCode, string verifyCode)
    {
        var client = httpClientFactory.CreateClient("DataApi");

        var url = $"api/connectbysite?SiteId={Uri.EscapeDataString(siteId)}"
                + $"&AccessCode={Uri.EscapeDataString(accessCode)}"
                + $"&VerifyCode={Uri.EscapeDataString(verifyCode)}";

        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = $"Login failed: {await response.Content.ReadAsStringAsync()}";
            ViewBag.Sites = await client.GetFromJsonAsync<List<SiteInfo>>("api/connection/sites", JsonOpts) ?? [];
            return View("Index");
        }

        // The data service returns the JWT as a plain string
        var token = (await response.Content.ReadAsStringAsync()).Trim('"');
        HttpContext.Session.SetString("JwtToken", token);
        HttpContext.Session.SetString("SiteId", siteId);

        return RedirectToAction("Dashboard");
    }

    /// <summary>Show VistA connection info: user details and session status.</summary>
    public async Task<IActionResult> Dashboard()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Index");

        var client = httpClientFactory.CreateClient("DataApi");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        try
        {
            // Fetch user info
            var userInfo = await client.GetFromJsonAsync<UserInfo>("api/user/info", JsonOpts);
            ViewBag.UserInfo = userInfo;

            // Fetch session status
            var statusResponse = await client.GetAsync("api/session/status");
            ViewBag.SessionStatus = statusResponse.IsSuccessStatusCode
                ? await statusResponse.Content.ReadAsStringAsync()
                : "Unknown";

            ViewBag.SiteId = HttpContext.Session.GetString("SiteId");
            ViewBag.Token = token;
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Failed to load connection info: {ex.Message}";
        }

        return View();
    }

    /// <summary>Disconnect from VistA and clear session.</summary>
    [HttpPost]
    public async Task<IActionResult> Disconnect()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
        {
            var client = httpClientFactory.CreateClient("DataApi");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            await client.PostAsync("api/session/disconnect", null);
        }

        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    // ── DTOs ────────────────────────────────────────────────────
    public record SiteInfo(string SiteId, string Name, string VisnName);

    public record UserInfo(
        string Name,
        string Duz,
        string Title,
        string ServiceSection,
        string Division,
        string Language,
        string Dtime);
}
