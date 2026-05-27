using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace CarePlatform.Security.VA.STSv2.Services
{
    /// <summary>
    /// Performs the Entra ID On-Behalf-Of (OBO) token exchange using
    /// <see cref="ITokenAcquisition"/> from Microsoft.Identity.Web.
    /// Returns a token whose <c>aud</c> claim targets the configured STS scope
    /// (e.g. <c>api://sqa.sts.va.gov/token</c>).
    /// </summary>
    public class OboTokenService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly Configuration.StsOptions _sts;
        private readonly ILogger<OboTokenService> _logger;

        public OboTokenService(
            ITokenAcquisition tokenAcquisition,
            IOptions<Configuration.StsOptions> stsOptions,
            ILogger<OboTokenService> logger)
        {
            _tokenAcquisition = tokenAcquisition;
            _sts = stsOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Exchanges the current user's signed-in token for an STS-audience access token.
        /// The bootstrap JWT is held by Microsoft.Identity.Web's token cache; callers no
        /// longer need to supply it explicitly.
        /// </summary>
        public async Task<OboTokenResult> ExchangeForStsTokenAsync(ClaimsPrincipal user)
        {
            if (string.IsNullOrEmpty(_sts.Scope))
            {
                throw new InvalidOperationException("Sts:Scope is not configured.");
            }

            _logger.LogDebug("OBO: Acquiring access token for scope {Scope}", _sts.Scope);

            try
            {
                var result = await _tokenAcquisition
                    .GetAuthenticationResultForUserAsync(new[] { _sts.Scope }, user: user)
                    .ConfigureAwait(false);

                _logger.LogDebug("OBO: Token acquired (expires {ExpiresOn}).", result.ExpiresOn);

                

                return new OboTokenResult
                {
                    Success = true,
                    AccessToken = result.AccessToken,
                    TokenType = result.TokenType,
                    ExpiresIn = (int)Math.Max(0, (result.ExpiresOn - DateTimeOffset.UtcNow).TotalSeconds),
                    RequestedScope = _sts.Scope,
                    TokenEndpoint = result.TenantId,
                };
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                // Let the AuthorizeForScopes filter trigger an interactive challenge.
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OBO: Token exchange failed for scope {Scope}.", _sts.Scope);

                return new OboTokenResult
                {
                    Success = false,
                    Error = ex.Message,
                    RequestedScope = _sts.Scope,
                };
            }
        }
    }

    public class OboTokenResult
    {
        public bool Success { get; set; }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Error { get; set; }
        public string TokenEndpoint { get; set; }
        public string RequestedScope { get; set; }
    }
}
