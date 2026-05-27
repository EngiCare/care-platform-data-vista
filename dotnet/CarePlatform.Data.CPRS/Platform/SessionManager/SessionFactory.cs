// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarePlatform.Data.CPRS
{
    public static class SessionInfoFactory
    {
        public static ISessionManager GetDefaultSessionManager(IConfiguration configuration, ILogger logger)
        {
            var sessionManagerType = configuration["SessionManager:SessionManagerObject"];
            if (string.IsNullOrEmpty(sessionManagerType))
            {
                return new SessionManager();
            }
            else
            {
                logger.LogDebug("Loading non-default session manager: {Type}", sessionManagerType);
                var type = Type.GetType(sessionManagerType);
                if (type == null)
                    throw new InvalidOperationException($"Session manager type not found: {sessionManagerType}");
                return (ISessionManager)Activator.CreateInstance(type);
            }
        }

        public static ISession GetDefaultSession(IConfiguration configuration, ILogger logger,
            string SiteId, string HostName, string Port, string AccessCode, string VerifyCode, string SSOToken)
        {
            var sessionType = configuration["SessionManager:SessionObject"];
            if (string.IsNullOrEmpty(sessionType))
            {
                return new Session(SiteId, HostName, Port, AccessCode, VerifyCode, SSOToken);
            }
            else
            {
                logger.LogDebug("Loading non-default session object: {Type}", sessionType);
                var type = Type.GetType(sessionType);
                if (type == null)
                    throw new InvalidOperationException($"Session type not found: {sessionType}");
                return (ISession)Activator.CreateInstance(type);
            }
        }
    }
}
