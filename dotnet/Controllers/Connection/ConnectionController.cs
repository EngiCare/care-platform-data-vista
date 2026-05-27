using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using CarePlatform.Data.VistA;
using CarePlatform.Data.CPRS.Security;
using CarePlatform.Models.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CarePlatform.Data.CPRS
{
    [ApiController]
    public class ConnectionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConnectionController> _logger;

        public ConnectionController(IConfiguration configuration, ILogger<ConnectionController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("api/connect")]
        public ActionResult<string> ConnectToVista(
            [FromQuery] string SiteId,
            [FromQuery] string HostName,
            [FromQuery] string Port,
            [FromQuery] string AccessCode,
            [FromQuery] string VerifyCode,
            [FromQuery] string SSOToken)
        {
            ClaimsPrincipal principal;
            try
            {
                principal = InitializeSession(_configuration, _logger, SiteId, HostName, Port, AccessCode, VerifyCode, SSOToken);
            }
            catch (AggregateException ex) when (ex.InnerException is UnauthorizedAccessException)
            {
                return Unauthorized(ex.InnerException.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }

            // Build the token
            X509Certificate2 signingCert = CertificateUtil.GetCertificate(
                StoreName.My, StoreLocation.LocalMachine,
                TokenServiceConfiguration.SigningCertificateName);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = (ClaimsIdentity)principal.Identity,
                Issuer = TokenServiceConfiguration.IssuerName,
                Audience = TokenServiceConfiguration.Realm,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(20),
                SigningCredentials = new SigningCredentials(
                    new X509SecurityKey(signingCert), SecurityAlgorithms.RsaSha256),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var finaltoken = tokenHandler.WriteToken(token);

            return finaltoken;
        }

        [HttpGet("api/login")]
        public ActionResult<string> LoginToVista(
            [FromQuery] string username,
            [FromQuery] string password,
            [FromQuery] string siteId = "640",
            [FromQuery] string SSOToken = "")
        {
            _logger.LogDebug("Login was called");
            return ConnectToVista(siteId, username, password);
        }

        /// <summary>
        /// Returns the list of configured sites available for connection.
        /// Used by CPRS Web login page to populate the site selector dropdown.
        /// </summary>
        [HttpGet("api/connection/sites")]
        public ActionResult<List<SiteEntry>> GetSites()
        {
            return Ok(ECConfiguration.GetSiteList());
        }

        [HttpGet("api/connectbysite")]
        public ActionResult<string> ConnectToVista(
            [FromQuery] string SiteId,
            [FromQuery] string AccessCode,
            [FromQuery] string VerifyCode,
            [FromQuery] string SSOToken = "")
        {
            _logger.LogDebug("Connect was called (GET)");
            return ConnectBySite(SiteId, AccessCode, VerifyCode, SSOToken);
        }

        [HttpPost("api/connectbysite")]
        public ActionResult<string> ConnectToVistaPost(
            [FromForm] string SiteId,
            [FromForm] string AccessCode,
            [FromForm] string VerifyCode,
            [FromForm] string SSOToken = "")
        {
            _logger.LogDebug("Connect was called (POST)");
            return ConnectBySite(SiteId, AccessCode, VerifyCode, SSOToken);
        }

        private ActionResult<string> ConnectBySite(string SiteId, string AccessCode, string VerifyCode, string SSOToken)
        {
            if (string.IsNullOrEmpty(SiteId))
                return BadRequest("SiteId is required.");

            if (string.IsNullOrEmpty(SSOToken) && (string.IsNullOrEmpty(AccessCode) || string.IsNullOrEmpty(VerifyCode)))
                return BadRequest("AccessCode and VerifyCode are required when SSOToken is not provided.");

            string HostName = SiteConfigManager.LookupHostNameForSite(SiteId);
            string Port = SiteConfigManager.LookupPortForSite(SiteId);

            return ConnectToVista(SiteId, HostName, Port, AccessCode, VerifyCode, SSOToken);
        }

        public static ClaimsPrincipal InitializeSession(IConfiguration configuration, ILogger logger, string SiteId, string AccessCode, string VerifyCode, string SSOToken)
        {
            string HostName = SiteConfigManager.LookupHostNameForSite(SiteId);
            string Port = SiteConfigManager.LookupPortForSite(SiteId);

            logger.LogDebug("Site: {SiteId} - {HostName} - {Port}", SiteId, HostName, Port);

            return InitializeSession(configuration, logger, SiteId, HostName, Port, AccessCode, VerifyCode, SSOToken);
        }

        public static ClaimsPrincipal InitializeSession(IConfiguration configuration, ILogger logger, string SiteId, string HostName, string Port, string AccessCode, string VerifyCode, string SSOToken)
        {
            var Session = SessionInfoFactory.GetDefaultSession(configuration, logger, SiteId, HostName, Port, AccessCode, VerifyCode, SSOToken);
            string SessionId = Guid.NewGuid().ToString();
            ECConfiguration.SessionManager.AddSession(SessionId, Session);
            return Session.UserToClaimsPrincipal(SessionId);
        }
    }
}
