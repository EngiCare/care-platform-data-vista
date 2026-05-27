// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CarePlatform.Security.VA.STSv2.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

namespace CarePlatform.Security.VA.STSv2.Controllers
{
    /// <summary>
    /// API endpoint that returns a raw SAML token for programmatic access.
    /// Uses the same Entra ID + OBO + VA STS flow as <see cref="HomeController.Login"/>,
    /// but returns JSON.
    /// </summary>
    [Authorize]
    [Route("TokenApi")]
    public class TokenApiController : Controller
    {
        private readonly OboTokenService _oboTokenService;
        private readonly StsTokenIssuer _stsTokenIssuer;
        private readonly ILogger<TokenApiController> _logger;

        public TokenApiController(
            OboTokenService oboTokenService,
            StsTokenIssuer stsTokenIssuer,
            ILogger<TokenApiController> logger)
        {
            _oboTokenService = oboTokenService;
            _stsTokenIssuer = stsTokenIssuer;
            _logger = logger;
        }

        /// <summary>GET /TokenApi/Token — returns <c>{ "token": "&lt;SAML XML&gt;" }</c>.</summary>
        [HttpGet("Token")]
        [AuthorizeForScopes(ScopeKeySection = "Sts:Scope")]
        public async Task<IActionResult> Token()
        {
            var identity = User.Identity as ClaimsIdentity;
            var bootstrapJwt = identity?.Claims.FirstOrDefault(c => c.Type == "id_token")?.Value
                ?? await HttpContext.GetTokenAsync("id_token").ConfigureAwait(false);

            if (string.IsNullOrEmpty(bootstrapJwt))
            {
                _logger.LogError("TokenApi: No bootstrap JWT found in claims.");
                return StatusCode(401, new { error = "No bootstrap JWT found. Re-authenticate with Entra ID." });
            }

            var oboResult = await _oboTokenService.ExchangeForStsTokenAsync(User).ConfigureAwait(false);
            if (!oboResult.Success)
            {
                _logger.LogError("TokenApi: OBO exchange failed: {Error}", oboResult.Error);
                return StatusCode(502, new { error = "OBO token exchange failed." });
            }

            string responseString;
            try
            {
                var soapRequest = _stsTokenIssuer.BuildSoapRequest(oboResult.AccessToken);
                responseString = await _stsTokenIssuer.SendAsync(soapRequest).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TokenApi: STS request failed.");
                return StatusCode(502, new { error = "STS request failed." });
            }

            var samlToken = StsTokenIssuer.ExtractSamlAssertion(responseString);
            if (string.IsNullOrEmpty(samlToken))
            {
                _logger.LogError("TokenApi: No SAML assertion in STS response.");
                return StatusCode(502, new { error = "No SAML assertion in STS response." });
            }

            return Json(new { token = samlToken });
        }
    }
}
