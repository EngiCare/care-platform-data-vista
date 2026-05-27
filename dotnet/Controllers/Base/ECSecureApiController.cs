using System;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Exception filter that handles unauthorized access and general exceptions.
    /// </summary>
    public class ECWebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var loggerFactory = context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var logger = loggerFactory?.CreateLogger<ECWebApiExceptionFilterAttribute>();

            logger?.LogDebug("Processing an exception");

            if (context.Exception is UnauthorizedAccessException)
            {
                logger?.LogWarning(context.Exception, "Unauthorized access exception handled");
                context.Result = new ObjectResult("Please log in again.") { StatusCode = 401 };
                context.ExceptionHandled = true;
                return;
            }

            logger?.LogError(context.Exception, "Unhandled exception");

            context.Result = new ObjectResult("An error occurred, please try again or contact the administrator.") { StatusCode = 500 };
            context.ExceptionHandled = true;
        }
    }

    /// <summary>
    /// Base API controller with exception filter and [ApiController] behavior.
    /// </summary>
    [ECWebApiExceptionFilter]
    [ApiController]
    [ApiVersion("1.0")]
    public class ECApiController : ControllerBase
    {
    }

    /// <summary>
    /// Secure API controller base class. Requires JWT authentication.
    /// </summary>
    [Authorize]
    public class ECSecureApiController : ECApiController
    {
    }
}
