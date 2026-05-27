using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace CarePlatform.Data.CPRS
{
    public static class SiteConfigManager
    {

        private static XNamespace SiteNs => "http://med.va.gov/vistaweb/sitesTable";

        public static string LookupHostNameForSite(string SiteId)
        {
            XDocument xDoc = XDocument.Load(ECConfiguration.SiteConfigFilePath);
            // Support both namespaced and non-namespaced XML
            var visns = xDoc.Root.Elements(SiteNs + "VhaVisn");
            if (!visns.Any()) visns = xDoc.Root.Elements("VhaVisn");

            foreach (XElement visn in visns)
            {
                var sites = visn.Elements(SiteNs + "VhaSite");
                if (!sites.Any()) sites = visn.Elements("VhaSite");

                foreach (XElement site in sites)
                {
                    if (site.Attribute("ID")?.Value == SiteId)
                    {
                        var sources = site.Elements(SiteNs + "DataSource");
                        if (!sources.Any()) sources = site.Elements("DataSource");

                        foreach (XElement DataSource in sources)
                        {
                            if (DataSource.Attribute("protocol")?.Value == "VISTA")
                                return DataSource.Attribute("source").Value;
                        }
                    }
                }
            }

            throw new ApplicationException("Site not found in configuration: " + SiteId);
        }

        public static string LookupPortForSite(string SiteId)
        {
            var logger = ECConfiguration.LoggerFactory?.CreateLogger(typeof(SiteConfigManager));
            logger?.LogDebug("loading: {Path}", ECConfiguration.SiteConfigFilePath);
            XDocument xDoc = XDocument.Load(ECConfiguration.SiteConfigFilePath);
            var visns = xDoc.Root.Elements(SiteNs + "VhaVisn");
            if (!visns.Any()) visns = xDoc.Root.Elements("VhaVisn");

            foreach (XElement visn in visns)
            {
                var sites = visn.Elements(SiteNs + "VhaSite");
                if (!sites.Any()) sites = visn.Elements("VhaSite");

                foreach (XElement site in sites)
                {
                    if (site.Attribute("ID")?.Value == SiteId)
                    {
                        var sources = site.Elements(SiteNs + "DataSource");
                        if (!sources.Any()) sources = site.Elements("DataSource");

                        foreach (XElement DataSource in sources)
                        {
                            if (DataSource.Attribute("protocol")?.Value == "VISTA")
                                return DataSource.Attribute("port").Value;
                        }
                    }
                }
            }

            throw new ApplicationException("Site not found in configuration: " + SiteId);
        }

    }
}
