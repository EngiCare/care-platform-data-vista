// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System.Collections.Concurrent;

namespace CarePlatform.Data.CPRS
{
    public interface ISessionManager
    {
        void AddSession(string SessionId, ISession Session);
        ISession GetSession(string SessionId);
        bool RemoveSession(string SessionId);
        bool SessionExists(string SessionId);
        void DumpDebugInfo();
    }
}