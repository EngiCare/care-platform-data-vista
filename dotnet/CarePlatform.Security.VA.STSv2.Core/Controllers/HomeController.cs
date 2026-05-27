using System;
using System.Net;
using System.Threading.Tasks;
using CarePlatform.Security.VA.STSv2.Configuration;
using CarePlatform.Security.VA.STSv2.Models;
using CarePlatform.Security.VA.STSv2.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace CarePlatform.Security.VA.STSv2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly OboTokenService _oboTokenService;
        private readonly StsTokenIssuer _stsTokenIssuer;
        private readonly SsoConfigOptions _sso;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            OboTokenService oboTokenService,
            StsTokenIssuer stsTokenIssuer,
            IOptions<SsoConfigOptions> ssoOptions,
            ILogger<HomeController> logger)
        {
            _oboTokenService = oboTokenService;
            _stsTokenIssuer = stsTokenIssuer;
            _sso = ssoOptions.Value;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index() => View();

        /// <summary>
        /// Performs the STS token exchange using the Entra ID bootstrap JWT
        /// (id_token) and returns an HTML auto-post form with the SAML token.
        /// </summary>
        [HttpGet]
        [AuthorizeForScopes(ScopeKeySection = "Sts:Scope")]
        public async Task<IActionResult> Login(string OriginalUri, string PostBackUrl)
        {
            if (string.IsNullOrEmpty(OriginalUri))
                throw new ArgumentNullException(nameof(OriginalUri));

            if (string.IsNullOrEmpty(PostBackUrl))
                throw new ArgumentNullException(nameof(PostBackUrl));

            var oboResult = await _oboTokenService.ExchangeForStsTokenAsync(User).ConfigureAwait(false);

            if (!oboResult.Success)
            {
                _logger.LogError("OBO token exchange failed: {Error}", oboResult.Error);
                throw new InvalidOperationException("OBO token exchange failed. See logs for details.");
            }

            _logger.LogDebug("SSOSTSv2: OBO token obtained (aud={Scope}, expires_in={Exp}s).",
                oboResult.RequestedScope, oboResult.ExpiresIn);

            var soapRequest = _stsTokenIssuer.BuildSoapRequest(oboResult.AccessToken);
            var responseString = await _stsTokenIssuer.SendAsync(soapRequest).ConfigureAwait(false);

            var assertion = StsTokenIssuer.ExtractSamlAssertion(responseString);
            var encodedToken = assertion is null ? string.Empty : WebUtility.UrlEncode(assertion);

            var model = new LoginViewModel
            {
                OriginalUri = OriginalUri,
                PostBackUrl = PostBackUrl,
                EncodedToken = encodedToken
            };

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = _sso.AppUrl ?? "/" });

            return new EmptyResult();
        }

        [AllowAnonymous]
        public IActionResult Error(string message)
        {
            ViewBag.ErrorMessage = message;
            return View("Error");
        }
    }
}
