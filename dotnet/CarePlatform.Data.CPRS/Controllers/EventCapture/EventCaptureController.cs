// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Event Capture System (ECS) — check ESSO installation, visit/division IDs,
    /// user path, report generation and printing.
    /// Migrated from cprs/rECS.pas — ORECS01 * RPCs.
    /// </summary>
    public class EventCaptureController : BaseController
    {
        public EventCaptureController() : base() { }

        /// <summary>
        /// Check whether ESSO (Event-Capture) is installed on the server.
        /// Returns a positive integer if installed, else 0.
        /// RPC: ORECS01 CHKESSO
        /// </summary>
        [HttpGet, Route("api/eventcapture/isessoinstalled")]
        public async Task<ActionResult<string>> IsESSOInstalled()
        {
            var vq = new VistaQuery("ORECS01 CHKESSO");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the visit ID for Event Capture based on the encounter visit string
        /// and patient DFN (passed as "visitStr;DFN").
        /// RPC: ORECS01 VSITID
        /// </summary>
        [HttpGet, Route("api/eventcapture/visitid")]
        public async Task<ActionResult<string>> GetVisitId([FromQuery] string visitStr)
        {
            var vq = new VistaQuery("ORECS01 VSITID");
            vq.addParameter(VistaQuery.LITERAL, visitStr);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the division ID for Event Capture.
        /// RPC: ORECS01 GETDIV
        /// </summary>
        [HttpGet, Route("api/eventcapture/division")]
        public async Task<ActionResult<string>> GetDivisionId()
        {
            var vq = new VistaQuery("ORECS01 GETDIV");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save the user's ECS path. Returns the updated current path.
        /// RPC: ORECS01 SAVPATH
        /// </summary>
        [HttpPost, Route("api/eventcapture/path")]
        public async Task<ActionResult<string>> SaveUserPath([FromQuery] string pathInfo)
        {
            var vq = new VistaQuery("ORECS01 SAVPATH");
            vq.addParameter(VistaQuery.LITERAL, pathInfo);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Load an Event Capture report. Supply the report parameters as named
        /// key/value pairs: ECHNDL, ECPTYP, ECDEV, ECDFN, ECSD, ECED, ECRY, ECDUZ.
        /// RPC: ORECS01 ECRPT
        /// </summary>
        [HttpPost, Route("api/eventcapture/report")]
        public async Task<ActionResult<List<string>>> LoadReport(
            [FromQuery] string reportHandle,
            [FromQuery] string reportType,
            [FromQuery] string printDev,
            [FromQuery] string dfn,
            [FromQuery] string startDate,
            [FromQuery] string endDate,
            [FromQuery] string needReason,
            [FromQuery] string userId)
        {
            var vq = new VistaQuery("ORECS01 ECRPT");
            var dhl = new DictionaryHashList();
            dhl.Add("\"ECHNDL\"", reportHandle ?? "");
            dhl.Add("\"ECPTYP\"", reportType ?? "");
            dhl.Add("\"ECDEV\"", printDev ?? "");
            dhl.Add("\"ECDFN\"", dfn ?? "");
            dhl.Add("\"ECSD\"", startDate ?? "");
            dhl.Add("\"ECED\"", endDate ?? "");
            dhl.Add("\"ECRY\"", needReason ?? "");
            dhl.Add("\"ECDUZ\"", userId ?? "");
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Print an Event Capture report to a device. Supply the same named
        /// key/value parameters as the report endpoint.
        /// RPC: ORECS01 ECPRINT
        /// </summary>
        [HttpPost, Route("api/eventcapture/print")]
        public async Task<ActionResult<string>> PrintReport(
            [FromQuery] string reportHandle,
            [FromQuery] string reportType,
            [FromQuery] string printDev,
            [FromQuery] string dfn,
            [FromQuery] string startDate,
            [FromQuery] string endDate,
            [FromQuery] string needReason,
            [FromQuery] string userId)
        {
            var vq = new VistaQuery("ORECS01 ECPRINT");
            var dhl = new DictionaryHashList();
            dhl.Add("\"ECHNDL\"", reportHandle ?? "");
            dhl.Add("\"ECPTYP\"", reportType ?? "");
            dhl.Add("\"ECDEV\"", printDev ?? "");
            dhl.Add("\"ECDFN\"", dfn ?? "");
            dhl.Add("\"ECSD\"", startDate ?? "");
            dhl.Add("\"ECED\"", endDate ?? "");
            dhl.Add("\"ECRY\"", needReason ?? "");
            dhl.Add("\"ECDUZ\"", userId ?? "");
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }
    }
}
