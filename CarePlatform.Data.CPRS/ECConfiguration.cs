using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using CarePlatform.Models.Session;

namespace CarePlatform.Data.CPRS
{
    internal static class ECConfiguration
    {
        private static XNamespace SiteNs => "http://med.va.gov/vistaweb/sitesTable";

        public static Dictionary<string, Dictionary<string, string>> AuthenticationSites { get; set; }

        public static Dictionary<string, string> VAFacilityLookupSites { get; set; }

        public static ISessionManager SessionManager;
        public static string SiteConfigFilePath { get; set; }

        /// <summary>
        /// Shared ILoggerFactory for non-DI classes (VistaConnection, SessionManager, etc.).
        /// Set once at startup from Program.cs.
        /// </summary>
        public static ILoggerFactory LoggerFactory { get; set; }

        public static void LoadConfiguration(IConfiguration configuration, ILogger logger)
        {
            SessionManager = SessionInfoFactory.GetDefaultSessionManager(configuration, logger);

            SiteConfigFilePath = configuration["SiteConfig:FilePath"] ?? "resources/xml/LocalVEHUSites.xml";

            logger.LogDebug("Loading VistaAuthenticationProvider");

            AuthenticationSites = new Dictionary<string, Dictionary<string, string>>();
            VAFacilityLookupSites = new Dictionary<string, string>();

            logger.LogDebug("Loading site config: {Path}", SiteConfigFilePath);
            XDocument xDoc = XDocument.Load(SiteConfigFilePath);

            // Support both namespaced and non-namespaced XML
            var visns = xDoc.Root.Elements(SiteNs + "VhaVisn");
            if (!visns.Any()) visns = xDoc.Root.Elements("VhaVisn");

            foreach (XElement visn in visns.OrderBy(x => Int16.Parse(x.Attribute("ID").Value)))
            {
                var siteValue = "VISN " + (string)visn.Attribute("ID") + " - " + (string)visn.Attribute("name");
                var siteDict = new Dictionary<string, string>();

                var sites = visn.Elements(SiteNs + "VhaSite");
                if (!sites.Any()) sites = visn.Elements("VhaSite");

                foreach (XElement site in sites)
                {
                    siteDict.Add((string)site.Attribute("ID"), (string)site.Attribute("name"));
                    VAFacilityLookupSites[(string)site.Attribute("ID")] = (string)site.Attribute("name");
                }

                AuthenticationSites[siteValue] = siteDict;
            }

            logger.LogInformation("Loaded {Count} site(s) from {Path}", VAFacilityLookupSites.Count, SiteConfigFilePath);
        }

        /// <summary>
        /// Returns a flat list of all configured sites for the login UI.
        /// </summary>
        public static List<SiteEntry> GetSiteList()
        {
            var result = new List<SiteEntry>();
            foreach (var (visnName, sites) in AuthenticationSites)
            {
                foreach (var (siteId, siteName) in sites)
                {
                    result.Add(new SiteEntry
                    {
                        SiteId = siteId,
                        Name = siteName,
                        VisnName = visnName
                    });
                }
            }
            return result;
        }
    }
}
