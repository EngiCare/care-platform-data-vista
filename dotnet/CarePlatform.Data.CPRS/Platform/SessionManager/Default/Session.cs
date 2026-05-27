// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;

namespace CarePlatform.Data.CPRS
{
    public class Session : ISession
    {
        private readonly ILogger _logger = ECConfiguration.LoggerFactory?.CreateLogger<Session>();
        private VistaConnection Connection;
        private User User { get; set; }



        public Session(string SiteId, string HostName, String Port, String AccessCode, String VerifyCode, String SSOToken = "")
        {
            // make the connection
            this.Connection = new VistaConnection(HostName, Port, SiteId);

            VistaCredentials creds = new VistaCredentials
            {
                AccountName = AccessCode,
                AccountPassword = VerifyCode,
                SSOToken = SSOToken
            };
            VistaOption permission = new VistaOption(VistaConstants.CPRS_CONTEXT);
            if (!string.IsNullOrEmpty(SSOToken))
                this.User = this.Connection.authorizedConnectSSO(creds, permission, null).Result;
            else
                this.User = this.Connection.authorizedConnect(creds, permission, null).Result;
        }

        public ClaimsPrincipal UserToClaimsPrincipal(string SessionId)
        {
            ClaimsIdentity id = new ClaimsIdentity();
            id.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Surname, User.Name.Lastname));
            id.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Name, User.Name.Firstname));
            id.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Sid, User.Uid));
            id.AddClaim(new System.Security.Claims.Claim(ClaimTypes.PrimarySid, User.Uid));

            _logger?.LogDebug("SiteId in token: {SiteId}", User.LogonSiteId.Id);

            id.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Locality, User.LogonSiteId.Id));
            id.AddClaim(new System.Security.Claims.Claim("http://schemas.token.engineerecare.org/claims/ContextId", SessionId));

            return new ClaimsPrincipal(id);
        }




        public List<string> CachedDivisionLines { get { return this.User?.DivisionLines; } }
        public bool IsContextSet { get; private set; }

        public async Task SelectDivisionAndSetContext(string stationNumber)
        {
            await this.Connection.Account.selectDivisionAndSetContext(stationNumber);
            IsContextSet = true;
        }

        public string ConnectionSiteId { get { return this.Connection?.SiteId; } internal set { } }
        public DateTime ConnectionLastUsed { get { return this.Connection.LastUsed; } }
        public int SessionTimeoutDurationInSeconds { get { return 900; } internal set { } }

        public async Task heartbeat(bool updateSessionLastUsedTime = false)
        {
            await this.Connection.heartbeat(updateSessionLastUsedTime);
        }
        public async Task disconnect()
        {
            await this.Connection.disconnect();
        }






        public async Task<string> query(VistaQuery vq)
        {
            return await this.Connection.query(vq);
        }


        
        public async Task<List<string>> tQuery(VistaQuery vq)
        {
            return (await query(vq)).Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
        }

        public async Task<string> sQuery(VistaQuery vq)
        {
            return (await query(vq)).Split(new string[] { "\r\n" }, StringSplitOptions.None).First();
        }


        public async Task<string> VistaReportQuery(string PatientId, string ReportValue, string FromDate = "0", string ToDate = "0", string DaysBack = "")
        {
            VistaQuery vqVitals = new VistaQuery("ORWRP REPORT TEXT");
            vqVitals.addParameter(VistaQuery.LITERAL, PatientId);
            vqVitals.addParameter(VistaQuery.LITERAL, ReportValue);
            vqVitals.addParameter(VistaQuery.LITERAL, "");
            vqVitals.addParameter(VistaQuery.LITERAL, DaysBack);
            vqVitals.addParameter(VistaQuery.LITERAL, "");
            vqVitals.addParameter(VistaQuery.LITERAL, FromDate);
            vqVitals.addParameter(VistaQuery.LITERAL, ToDate);
            return await Connection.query(vqVitals);
        }

        public async Task<string> RemoteVistaReportQuery(string SiteId, string PatientId, string ReportValue, string FromDate, string ToDate)
        {
            VistaQuery vqRemoteReport = new VistaQuery("ORWRP REPORT TEXT");
            vqRemoteReport.addParameter(VistaQuery.LITERAL, "0");
            vqRemoteReport.addParameter(VistaQuery.LITERAL, PatientId);
            vqRemoteReport.addParameter(VistaQuery.LITERAL, ReportValue);
            vqRemoteReport.addParameter(VistaQuery.LITERAL, "");
            vqRemoteReport.addParameter(VistaQuery.LITERAL, "");
            vqRemoteReport.addParameter(VistaQuery.LITERAL, "");
            vqRemoteReport.addParameter(VistaQuery.LITERAL, FromDate);
            vqRemoteReport.addParameter(VistaQuery.LITERAL, ToDate);
            return await Connection.remoteQuery(SiteId, vqRemoteReport);
        }


        public async Task<string> RemoteVistaReportQuerySchedule(string SiteId, string PatientId, string ReportValue, string FromDate, string ToDate)
        {
            VistaQuery vqRemoteReport = new VistaQuery("ORWRP REPORT TEXT");
            vqRemoteReport.addParameter(VistaQuery.LITERAL, "0");
            vqRemoteReport.addParameter(VistaQuery.LITERAL, PatientId);
            vqRemoteReport.addParameter(VistaQuery.LITERAL, ReportValue);
            vqRemoteReport.addParameter(VistaQuery.LITERAL, "");
            vqRemoteReport.addParameter(VistaQuery.LITERAL, "");
            vqRemoteReport.addParameter(VistaQuery.LITERAL, "");
            vqRemoteReport.addParameter(VistaQuery.LITERAL, FromDate);
            vqRemoteReport.addParameter(VistaQuery.LITERAL, ToDate);
            return await Connection.ScheduleRemoteQuery(SiteId, vqRemoteReport);
        }


        public async Task<string> RemoteQueryGetData(string remoteKey)
        {
            return await Connection.RemoteQueryGetData(remoteKey);
        }

        public async Task<bool> RemoteQueryCompletionCheck(string remoteKey)
        {
            return await Connection.RemoteQueryCompletionCheck(remoteKey);
        }

    }
}