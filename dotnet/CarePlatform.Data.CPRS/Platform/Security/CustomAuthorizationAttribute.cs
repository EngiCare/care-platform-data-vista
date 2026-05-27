using System;
using System.Security.Claims;
using CarePlatform.Data.CPRS;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarePlatform.Data.CPRS
{
    public class SessionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var logger = actionContext.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("SessionFilter");

            var thisController = ((BaseController)actionContext.Controller);

            var identity = actionContext.HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                logger.LogTrace("[SessionFilter] Identity found — IsAuthenticated={IsAuth}, AuthType={AuthType}, Claims={Count}",
                    identity.IsAuthenticated, identity.AuthenticationType ?? "(null)", identity.Claims.Count());

                var contextIdClaim = identity.FindFirst("http://schemas.token.engineerecare.org/claims/ContextId");
                if (contextIdClaim != null)
                {
                    string SessionGuid = contextIdClaim.Value;
                    if (!string.IsNullOrEmpty(SessionGuid))
                    {
                        if (ECConfiguration.SessionManager.SessionExists(SessionGuid))
                        {
                            thisController.Session = ECConfiguration.SessionManager.GetSession(SessionGuid);
                            logger.LogTrace("[SessionFilter] Session resolved for ContextId={CtxId}", SessionGuid);
                            base.OnActionExecuting(actionContext);
                            return;
                        }
                        else
                        {
                            logger.LogWarning("[SessionFilter] No session found for ContextId={CtxId} — session may have expired or been lost",
                                SessionGuid);
                        }
                    }
                    else
                    {
                        logger.LogWarning("[SessionFilter] ContextId claim is present but value is null/empty");
                    }
                }
                else
                {
                    logger.LogWarning("[SessionFilter] No ContextId claim in JWT — available claims: {Claims}",
                        string.Join(", ", identity.Claims.Select(c => c.Type)));
                }
            }
            else
            {
                logger.LogWarning("[SessionFilter] No ClaimsIdentity on request — User.Identity type={Type}",
                    actionContext.HttpContext.User.Identity?.GetType().Name ?? "(null)");
            }

            throw new UnauthorizedAccessException();
        }
    }
}