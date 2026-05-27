using CarePlatform.Data.VistA;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    public class SessionManager : ISessionManager
    {
        private ConcurrentDictionary<string, ISession> Sessions { get; set; }
        private CancellationToken cancellationToken = new CancellationToken();
        private readonly ILogger _logger;

        public SessionManager()
        {
            _logger = ECConfiguration.LoggerFactory?.CreateLogger<SessionManager>();
            Sessions = new ConcurrentDictionary<string, ISession>();

            Task.Run(() => this.MaintainSessionsAsync());
        }

        public async Task MaintainSessionsAsync()
        {
            //Console.WriteLine("[Debug] " + "Maintaining sessions Async was called");
            TimeSpan interval = new TimeSpan(0, 0, 30);
        
            while (true)
            {
                //Console.WriteLine("[Debug] " + "Active session maintenance started.");
                await SessionManager.MaintainSessions(this);
                //Console.WriteLine("[Debug] " + "Active session maintenance completed.");
                await Task.Delay(interval, cancellationToken);
            }
        }

        public void AddSession(string SessionId, ISession Session)
        {
            Sessions.TryAdd( SessionId, Session);
        }

        public bool SessionExists(string SessionId)
        {
            return Sessions.ContainsKey(SessionId);
        }

        public ISession GetSession(string SessionId)
        {
            return Sessions[SessionId];
        }

        public bool RemoveSession(string SessionId)
        {
            ISession outSession = null;
            return Sessions.TryRemove(SessionId, out outSession);
        }

        public void DumpDebugInfo()
        {
            _logger?.LogDebug("Session count: {Count}", Sessions.Count);
            foreach (string SessionId in Sessions.Keys)
            {
                _logger?.LogDebug("Dumping session: {SessionId}", SessionId);
            }
        }


        internal static async Task MaintainSessions(object o)
        {
            //Console.WriteLine("[Debug] " + "Maintaining the sessions");
            SessionManager SessionManager = (SessionManager)o;

            var sessionsForRemoval = new List<String>();

            foreach (var sessionKey in SessionManager.Sessions.Keys.ToList())
            {
                ISession session = null;
                try
                {
                    SessionManager.Sessions.TryGetValue(sessionKey, out session);
                    SessionManager._logger?.LogDebug("Last used time: {LastUsed}", session.ConnectionLastUsed);
                    if (0 <= DateTime.Compare(DateTime.Now, session.ConnectionLastUsed.AddSeconds(session.SessionTimeoutDurationInSeconds)))
                    {
                        SessionManager._logger?.LogDebug("Connection has expired");
                        await session.disconnect();
                        SessionManager._logger?.LogDebug("Connection is disconnected. Removing");
                        sessionsForRemoval.Add(sessionKey);
                    }
                    else
                    {
                        await session.heartbeat();
                    }
                }
                catch (Exception e)
                {
                    SessionManager._logger?.LogError(e, "Error during session maintenance");
                }

                //Console.WriteLine("[Debug] " + "Done connection maintenance.");
            }


            foreach (var sess in sessionsForRemoval)
                SessionManager.RemoveSession(sess);               

        }

    }
}
