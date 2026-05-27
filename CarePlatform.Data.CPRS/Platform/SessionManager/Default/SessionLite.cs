using CarePlatform.Data.VistA;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Xml.Linq;
using Newtonsoft.Json;

using Claim = System.Security.Claims.Claim;

namespace CarePlatform.Data.CPRS
{
    public class SessionLite : ISession
    {
        private readonly ILogger _logger = ECConfiguration.LoggerFactory?.CreateLogger<SessionLite>();
        private List<KeyValuePair<string, string>> _attributes = new List<KeyValuePair<string, string>>();
        private string ActiveSiteId { get; set; }

        private DateTime LastUsed { get; set; }

        public int SessionTimeoutDurationInMinutes { get { return 60; } internal set { } }
        public int SessionTimeoutDurationInSeconds { get { return 3640; } internal set { } }


        public SessionLite(String SSOToken = "")
        {
            _logger?.LogDebug("SSO Token received: {Token}", System.Net.WebUtility.UrlDecode(SSOToken));

            LastUsed = DateTime.Now;

            var doc = XDocument.Parse("<token>" + SSOToken + " </token>");
            doc.Root.Descendants("Attribute")?.ToList()
                .ForEach(e => {
                    var name = (string)e.Attribute("Name").Value;
                    var value = e.Element("AttributeValue").Value;
                    _logger?.LogDebug("Adding attribute: {Name}", name);
                    _attributes.Add(new KeyValuePair<string, string>(name, value));
                });

            _logger?.LogDebug("Parsed {Count} attributes from SSO token", _attributes.Count);
        }

        public ClaimsPrincipal UserToClaimsPrincipal(string SessionId)
        {
            ClaimsIdentity id = new ClaimsIdentity("SSOi");

            id.AddClaim(new System.Security.Claims.Claim("http://schemas.token.engineerecare.org/claims/ContextId", SessionId));

            foreach (var attribute in _attributes)
            {
                _logger?.LogDebug("SamlAttribute: {Key} - {Value}", attribute.Key, attribute.Value);
                switch (attribute.Key)
                {
                    case "urn:va:vrm:iam:lastname":
                        id.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Surname, attribute.Value));
                        break;

                    case "urn:va:vrm:iam:firstname":
                        id.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Name, attribute.Value));
                        break;

                    case "urn:va:vrm:iam:secid":
                        id.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Sid, attribute.Value));
                        break;

                    case "uniqueUserId":
                        id.AddClaim(new System.Security.Claims.Claim(ClaimTypes.PrimarySid, attribute.Value));
                        break;

                    case "vistaid":
                        id.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Locality, attribute.Value));
                        break;
                }
            }

            return new ClaimsPrincipal(id);
        }




        public List<string> CachedDivisionLines { get { return null; } }
        public bool IsContextSet { get { return true; } }
        public async Task SelectDivisionAndSetContext(string stationNumber) { }
        public string ConnectionSiteId { get { return ActiveSiteId; } internal set { ActiveSiteId = value; } }
        public DateTime ConnectionLastUsed { get { return this.LastUsed; } }

        public async Task heartbeat(bool updateSessionLastUsedTime = false)
        {
            if( updateSessionLastUsedTime)
                LastUsed = DateTime.Now;
        }


        public async Task disconnect()
        {
            return;
        }
        



        public async Task<string> query(VistaQuery vq)
        {
            throw new NotImplementedException();
        }
        public async Task<List<string>> tQuery(VistaQuery vq)
        {
            throw new NotImplementedException();
        }
        public async Task<string> sQuery(VistaQuery vq)
        {
            throw new NotImplementedException();
        }


        public async Task<string> VistaReportQuery(string PatientId, string ReportValue, string FromDate, string ToDate, string DaysBack)
        {
            throw new NotImplementedException();
        }

        public async Task<string> RemoteVistaReportQuery(string SiteId, string PatientId, string ReportValue, string FromDate, string ToDate)
        {
            throw new NotImplementedException();
        }


        public async Task<string> RemoteVistaReportQuerySchedule(string SiteId, string PatientId, string ReportValue, string FromDate, string ToDate)
        {
            throw new NotImplementedException();
        }


        public async Task<string> RemoteQueryGetData(string remoteKey)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoteQueryCompletionCheck(string remoteKey)
        {
            throw new NotImplementedException();
        }

    }
}