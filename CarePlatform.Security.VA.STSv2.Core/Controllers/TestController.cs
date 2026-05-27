using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CarePlatform.Security.VA.STSv2.Models;
using CarePlatform.Security.VA.STSv2.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

namespace CarePlatform.Security.VA.STSv2.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        private readonly OboTokenService _oboTokenService;
        private readonly StsTokenIssuer _stsTokenIssuer;
        private readonly ILogger<TestController> _logger;

        public TestController(
            OboTokenService oboTokenService,
            StsTokenIssuer stsTokenIssuer,
            ILogger<TestController> logger)
        {
            _oboTokenService = oboTokenService;
            _stsTokenIssuer = stsTokenIssuer;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var identity = User.Identity as ClaimsIdentity;

            var claims = identity?.Claims
                .Select(c => new KeyValuePair<string, string>(c.Type, c.Value))
                ?? Enumerable.Empty<KeyValuePair<string, string>>();

            var claimsString = string.Join(Environment.NewLine,
                claims.Select(c => c.Key + "=" + c.Value));

            var bootstrapJwt = identity?.Claims.FirstOrDefault(c => c.Type == "id_token")?.Value
                ?? await HttpContext.GetTokenAsync("id_token").ConfigureAwait(false);

            var headers = Request.Headers
                .Select(h => new KeyValuePair<string, string>(h.Key, h.Value.ToString()));
            var headerString = string.Join(Environment.NewLine,
                Request.Headers.Select(h => h.Key + "=" + h.Value));

            var model = new TestHeaderViewModel
            {
                BootstrapJwt = bootstrapJwt ?? "No bootstrap JWT found.",
                UserName = User.Identity?.Name ?? "(unknown)",
                Claims = claims,
                ClaimsString = claimsString,
                Headers = headers,
                HeaderString = headerString
            };

            return View(model);
        }

        [AuthorizeForScopes(ScopeKeySection = "Sts:Scope")]
        public async Task<IActionResult> Issue()
        {
            var model = new TestIssueViewModel();

            model.RequestHeaders = Request.Headers
                .Select(h => new KeyValuePair<string, string>(h.Key, h.Value.ToString()));
            model.RequestHeaderString = string.Join(Environment.NewLine,
                Request.Headers.Select(h => h.Key + "=" + h.Value));

            model.ResponseHeaders = Response.Headers
                .Select(h => new KeyValuePair<string, string>(h.Key, h.Value.ToString()));
            model.ResponseHeaderString = string.Join(Environment.NewLine,
                Response.Headers.Select(h => h.Key + "=" + h.Value));

            var identity = User.Identity as ClaimsIdentity;
            var bootstrapJwt = identity?.Claims.FirstOrDefault(c => c.Type == "id_token")?.Value
                ?? await HttpContext.GetTokenAsync("id_token").ConfigureAwait(false);

            model.BootstrapJwt = bootstrapJwt ?? "No bootstrap JWT (id_token) found in claims.";
            model.UserName = User.Identity?.Name ?? "(unknown)";

            var oboResult = await _oboTokenService.ExchangeForStsTokenAsync(User).ConfigureAwait(false);

            model.OboSuccess = oboResult.Success;
            model.OboRequestedScope = oboResult.RequestedScope;
            model.OboTokenEndpoint = oboResult.TokenEndpoint;

            if (!oboResult.Success)
            {
                model.OboError = oboResult.Error;
                _logger.LogError("TestController.Issue: OBO exchange failed: {Error}", oboResult.Error);
                return View(model);
            }

            model.OboAccessToken = oboResult.AccessToken;
            model.OboExpiresIn = oboResult.ExpiresIn;

            model.RequestString = _stsTokenIssuer.BuildSoapRequest(oboResult.AccessToken);
            model.Endpoint = _stsTokenIssuer.SsoEndpoint;
            model.CertificateName = _stsTokenIssuer.CertificateName;

            try
            {
                model.ResponseString = await _stsTokenIssuer.SendAsync(model.RequestString).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TestController.Issue: STS request failed.");
                throw;
            }

            model.Token = StsTokenIssuer.ExtractSamlAssertion(model.ResponseString);

            return View(model);
        }
    }
}
