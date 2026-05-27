using System;
using CarePlatform.Data.VistA;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CarePlatform.Data.CPRS
{

    public class SessionController : BaseController
    {
        private readonly ILogger<SessionController> _logger;

        public SessionController(ILogger<SessionController> logger) : base()
        {
            _logger = logger;
        }

        [HttpGet, Route("api/session/timeremaining")]
        public async Task<int> SessionStatus()
        {
            var vq = new VistaQuery("XWB GET BROKER INFO");
            var result = await this.Session.sQuery(vq);
            if (int.TryParse(result, out var seconds))
                return seconds;

            // Fallback to local calculation
            if (0 <= DateTime.Compare(DateTime.Now, this.Session.ConnectionLastUsed.AddSeconds(this.Session.SessionTimeoutDurationInSeconds)))
            {
                return -1;
            }
            else
            {
                var secondsLeft = (int)(this.Session.ConnectionLastUsed.AddSeconds(this.Session.SessionTimeoutDurationInSeconds) - DateTime.Now).TotalSeconds;
                return secondsLeft;
            }
        }

        [HttpGet, Route("api/session/pulse")]
        public bool SessionHeartBeat()
        {
            this.Session.heartbeat(true);
            _logger.LogDebug("Pulse received");
            return true;
        }

        [HttpGet, Route("api/session/currenttime")]
        public async Task<string> CurrentTime()
        {
            return VistaTimestamp.toUtcString(await this.getCurrentTime());
        }

        [HttpPost, Route("api/session/disconnect")]
        public async Task<IActionResult> Disconnect()
        {
            try
            {
                var identity = HttpContext.User.Identity as System.Security.Claims.ClaimsIdentity;
                var contextIdClaim = identity?.FindFirst("http://schemas.token.engineerecare.org/claims/ContextId");
                var sessionGuid = contextIdClaim?.Value;

                _logger.LogInformation("Disconnect requested for session {SessionId}", sessionGuid);

                await this.Session.disconnect();

                if (!string.IsNullOrEmpty(sessionGuid))
                    ECConfiguration.SessionManager.RemoveSession(sessionGuid);

                _logger.LogInformation("Session {SessionId} disconnected and removed", sessionGuid);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting session");
                return Ok(); // Still return OK — caller is logging out regardless
            }
        }


    }
}
