using CarePlatform.Data.VistA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    public static class ReportQuery
    {
        public static async Task<string> VistaReportQuery(VistaConnection Connection, string PatientId, string ReportValue, string FromDate, string ToDate)
        {
            VistaQuery vqVitals = new VistaQuery("ORWRP REPORT TEXT");
            vqVitals.addParameter(VistaQuery.LITERAL, PatientId);
            vqVitals.addParameter(VistaQuery.LITERAL, ReportValue);
            vqVitals.addParameter(VistaQuery.LITERAL, "");
            vqVitals.addParameter(VistaQuery.LITERAL, "");
            vqVitals.addParameter(VistaQuery.LITERAL, "");
            vqVitals.addParameter(VistaQuery.LITERAL, FromDate);
            vqVitals.addParameter(VistaQuery.LITERAL, ToDate);
            return await Connection.query(vqVitals);
        }

        public static async Task<string> RemoteVistaReportQuery(VistaConnection Connection, string SiteId, string PatientId, string ReportValue, string FromDate, string ToDate)
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

    }
}
