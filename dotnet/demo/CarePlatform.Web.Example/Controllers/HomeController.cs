// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System.Text.Json;
using CarePlatform.Web.Example.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarePlatform.Web.Example.Controllers;

/// <summary>
/// Minimal example controller that demonstrates the full auth flow
/// by connecting directly to a VistA RPC broker (Mock VistA).
/// Supports two login paths (mirrors CPRS AccountController):
///   1. PIV / Smart Card (simulated — no real SSOi in the demo)
///   2. Access Code / Verify Code — traditional VistA authentication
/// </summary>
public class HomeController(VistaRpcClient rpcClient, IConfiguration config) : Controller
{
    // ── GET / ────────────────────────────────────────────
    // Landing page: "Sign In with PIV" vs "Sign In with Access/Verify Code"
    public IActionResult Index()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            return RedirectToAction("Dashboard");

        ViewData["HideChrome"] = true;
        if (ViewData["Error"] is not string)
            ViewData["Error"] = TempData["Error"] as string;
        return View();
    }

    // ── GET /Home/Login ──────────────────────────────────
    // Access Code / Verify Code form
    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            return RedirectToAction("Dashboard");

        ViewData["HideChrome"] = true;
        ViewBag.Sites = GetSites();
        return View();
    }

    // ── POST /Home/Login ─────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Login(string siteId, string accessCode, string verifyCode)
    {
        ViewData["HideChrome"] = true;

        if (string.IsNullOrEmpty(siteId) || string.IsNullOrEmpty(accessCode) || string.IsNullOrEmpty(verifyCode))
        {
            ViewBag.Error = "All fields are required.";
            ViewBag.Sites = GetSites();
            return View();
        }

        var site = GetSites().FirstOrDefault(s => s.SiteId == siteId);
        var host = site?.Host ?? "127.0.0.1";
        var port = site?.Port ?? 9200;

        try
        {
            var result = await rpcClient.LoginAsync(host, port, accessCode, verifyCode);

            if (!result.Success || result.User is null)
            {
                ViewBag.Error = $"Login failed: {result.Error}";
                ViewBag.Sites = GetSites();
                return View();
            }

            StoreUserSession(result.User, siteId);
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Connection failed: {ex.Message}";
            ViewBag.Sites = GetSites();
            return View();
        }
    }

    // ── GET /Home/PivLogin ───────────────────────────────
    // Simulated PIV login — in production this would redirect to SSOi.
    // For the demo, we show a confirmation page then auto-login.
    public IActionResult PivLogin()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            return RedirectToAction("Dashboard");

        ViewData["HideChrome"] = true;
        ViewBag.Sites = GetSites();
        return View();
    }

    // ── POST /Home/PivLogin ──────────────────────────────
    [HttpPost]
    public async Task<IActionResult> PivLogin(string siteId)
    {
        ViewData["HideChrome"] = true;

        var site = GetSites().FirstOrDefault(s => s.SiteId == siteId);
        var host = site?.Host ?? "127.0.0.1";
        var port = site?.Port ?? 9200;

        try
        {
            // In a real system, the PIV/SSOi token would be used here.
            // For the demo, we authenticate with the default credentials.
            var result = await rpcClient.LoginAsync(host, port, "cprs", "cprs1234");

            if (!result.Success || result.User is null)
            {
                TempData["Error"] = $"PIV login failed: {result.Error}";
                return RedirectToAction("Index");
            }

            StoreUserSession(result.User, siteId ?? "128");
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"PIV connection failed: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    /// <summary>Show VistA connection info from the session.</summary>
    public IActionResult Dashboard()
    {
        var name = HttpContext.Session.GetString("UserName");
        if (string.IsNullOrEmpty(name))
            return RedirectToAction("Index");

        ViewBag.UserInfo = new UserInfo(
            Name: name,
            Duz: HttpContext.Session.GetString("Duz") ?? "",
            Title: HttpContext.Session.GetString("Title") ?? "",
            ServiceSection: HttpContext.Session.GetString("ServiceSection") ?? "",
            Division: HttpContext.Session.GetString("Division") ?? "",
            Language: HttpContext.Session.GetString("Language") ?? "",
            Dtime: HttpContext.Session.GetString("Dtime") ?? "");

        ViewBag.SiteId = HttpContext.Session.GetString("SiteId");
        return View();
    }

    /// <summary>Disconnect from VistA and clear session.</summary>
    [HttpPost]
    public IActionResult Disconnect()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    // ── Helpers ─────────────────────────────────────────────────

    private void StoreUserSession(VistaRpcClient.UserInfo user, string siteId)
    {
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("Duz", user.Duz);
        HttpContext.Session.SetString("Title", user.Title);
        HttpContext.Session.SetString("ServiceSection", user.ServiceSection);
        HttpContext.Session.SetString("Division", user.Division);
        HttpContext.Session.SetString("Language", user.Language);
        HttpContext.Session.SetString("Dtime", user.Dtime);
        HttpContext.Session.SetString("SiteId", siteId);
    }

    private List<SiteInfo> GetSites()
    {
        var sites = config.GetSection("VistaSites").Get<List<SiteInfo>>();
        return sites ?? [new SiteInfo("128", "Mock VistA (localhost)", "127.0.0.1", 9200)];
    }

    // ── DTOs ────────────────────────────────────────────────────
    public record SiteInfo(string SiteId, string Name, string Host, int Port);

    public record UserInfo(
        string Name,
        string Duz,
        string Title,
        string ServiceSection,
        string Division,
        string Language,
        string Dtime);
}
