// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System;
using CarePlatform.Data.VistA;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CarePlatform.Data.CPRS
{
    public interface ISession
    {
        DateTime ConnectionLastUsed { get; }
        string ConnectionSiteId { get; }
        int SessionTimeoutDurationInSeconds { get; }
        ClaimsPrincipal UserToClaimsPrincipal(string SessionId);

        /// <summary>
        /// Division lines cached during login (fetched before context switch).
        /// </summary>
        List<string> CachedDivisionLines { get; }

        /// <summary>
        /// Whether the OR CPRS GUI CHART context has been set (Phase 2 complete).
        /// </summary>
        bool IsContextSet { get; }

        /// <summary>
        /// Phase 2 of login: set the selected division and switch to OR CPRS GUI CHART context.
        /// Must be called before any application RPCs (ORWU*, etc.).
        /// </summary>
        Task SelectDivisionAndSetContext(string stationNumber);

        Task disconnect();
        Task  heartbeat(bool updateSessionLastUsedTime = false);
        Task<string> query(VistaQuery vq);
        Task<List<string>> tQuery(VistaQuery vq);
        Task<string> sQuery(VistaQuery vq);

        Task<string> VistaReportQuery(string PatientId, string ReportValue, string FromDate = "0", string ToDate = "0", string DaysBack = "");


        Task<string> RemoteVistaReportQuery(string SiteId, string PatientId, string ReportValue, string FromDate, string ToDate);

        Task<string> RemoteVistaReportQuerySchedule(string SiteId, string PatientId, string ReportValue, string FromDate, string ToDate);

        Task<bool> RemoteQueryCompletionCheck(string remoteKey);

        Task<string> RemoteQueryGetData(string remoteKey);
    }
}