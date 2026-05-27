// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using CarePlatform.Models.Reports;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Reports — report lists, column headers, text retrieval, printing,
    /// health summary components, imaging exams, patient procedures, nutrition,
    /// remote-data access (CIRN / HDR / XWB).
    /// Migrated from cprs/rReports.pas — ORWRP *, ORWRP2 *, ORWRP3 *, ORWRP4 *,
    /// ORWCS *, ORWRA *, ORWMC *, ORWSR *, ORWCH *, ORWTPD *, ORWCIRN *,
    /// XWB REMOTE/DIRECT/DEFERRED RPCs.
    /// </summary>
    public class ReportController : BaseController
    {
        public ReportController() : base() { }

        #region Report Lists

        /// <summary>
        /// Get the report tree with hierarchy.
        /// RPC: ORWRP3 EXPAND COLUMNS (matches CPRS LoadTree('REPORTS') in rReports.pas).
        /// Returns tree-structured ReportDefinitions with Children for parent nodes.
        /// </summary>
        [HttpGet, Route("api/report/lists")]
        public async Task<ActionResult<List<ReportDefinition>>> ReportLists()
        {
            var vq = new VistaQuery("ORWRP3 EXPAND COLUMNS");
            vq.addParameter(VistaQuery.LITERAL, "REPORTS");
            var results = await this.Session.tQuery(vq);
            var tree = ReportDefinition.ParseTreeFromExpandColumns(results);
            await StampQualifierTypesAsync(tree);
            return tree;
        }

        /// <summary>
        /// Get the labs report tree (matches CPRS LoadTree('LABS') in rReports.pas — the
        /// data source for fLabs.pas tvReports). Returns a hierarchical list of
        /// <see cref="ReportDefinition"/>s ready for the web Labs tab tree.
        /// RPC: ORWRP3 EXPAND COLUMNS with 'LABS'.
        /// </summary>
        [HttpGet, Route("api/report/labtree")]
        public async Task<ActionResult<List<ReportDefinition>>> LabReportTree()
        {
            var vq = new VistaQuery("ORWRP3 EXPAND COLUMNS");
            vq.addParameter(VistaQuery.LITERAL, "LABS");
            var results = await this.Session.tQuery(vq);
            var tree = ReportDefinition.ParseTreeFromExpandColumns(results);
            await StampQualifierTypesAsync(tree);
            return tree;
        }

        /// <summary>
        /// Build IFN→qualifierType map from ORWRP REPORT LISTS [REPORT LIST] section
        /// and stamp QualifierType onto every node + child of the supplied tree.
        /// Mirrors ReportQualifierType in cprs/rReports.pas line 380-388, which iterates
        /// uReportsList comparing piece 1 to the requested IFN and returning piece 3.
        /// </summary>
        private async Task StampQualifierTypesAsync(List<ReportDefinition> tree)
        {
            try
            {
                var vq = new VistaQuery("ORWRP REPORT LISTS");
                var rows = await this.Session.tQuery(vq);
                var section = ExtractSection(rows, "[REPORT LIST]");
                var map = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (var line in section)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var p = line.Split('^');
                    if (p.Length < 3) continue;
                    var ifn = p[0];
                    if (!int.TryParse(p[2], out var qt)) continue;
                    map[ifn] = qt;
                }
                ApplyQualifierTypes(tree, map);
            }
            catch
            {
                // Non-fatal: if the lookup fails, JS falls back to the prefix
                // heuristic and date-range/most-recent reports still work in
                // text-only mode.
            }
        }

        private static void ApplyQualifierTypes(List<ReportDefinition> nodes, Dictionary<string, int> map)
        {
            foreach (var n in nodes)
            {
                if (!string.IsNullOrEmpty(n.IFN) && map.TryGetValue(n.IFN, out var qt))
                    n.QualifierType = qt;
                if (n.Children.Count > 0)
                    ApplyQualifierTypes(n.Children, map);
            }
        }


        /// <summary>
        /// Get date ranges for report filtering.
        /// RPC: ORWRP REPORT LISTS — extracts [DATE RANGES] section.
        /// </summary>
        [HttpGet, Route("api/report/dateranges")]
        public async Task<ActionResult<List<string>>> DateRanges()
        {
            var vq = new VistaQuery("ORWRP REPORT LISTS");
            var results = await this.Session.tQuery(vq);
            return ExtractSection(results, "[DATE RANGES]");
        }

        /// <summary>
        /// Get health summary types for report filtering.
        /// RPC: ORWRP REPORT LISTS — extracts [HEALTH SUMMARY TYPES] section.
        /// </summary>
        [HttpGet, Route("api/report/hstypes")]
        public async Task<ActionResult<List<string>>> HealthSummaryTypes()
        {
            var vq = new VistaQuery("ORWRP REPORT LISTS");
            var results = await this.Session.tQuery(vq);
            return ExtractSection(results, "[HEALTH SUMMARY TYPES]");
        }

        /// <summary>
        /// Extract a section from the multi-section ORWRP REPORT LISTS response.
        /// Matches ExtractSection in cprs/rReports.pas.
        /// </summary>
        private static List<string> ExtractSection(List<string> lines, string section)
        {
            var result = new List<string>();
            bool inSection = false;
            foreach (var line in lines)
            {
                if (line == section) { inSection = true; continue; }
                if (line == "$$END") { if (inSection) break; continue; }
                if (inSection)
                    result.Add(line);
            }
            return result;
        }

        /// <summary>
        /// Get lab-specific report lists.
        /// RPC: ORWRP LAB REPORT LISTS
        /// </summary>
        [HttpGet, Route("api/report/lablists")]
        public async Task<ActionResult<List<string>>> LabReportLists()
        {
            var vq = new VistaQuery("ORWRP LAB REPORT LISTS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get column headers for a report.
        /// RPC: ORWRP COLUMN HEADERS
        /// </summary>
        [HttpGet, Route("api/report/columnheaders")]
        public async Task<ActionResult<List<string>>> ColumnHeaders([FromQuery] string reportId)
        {
            var vq = new VistaQuery("ORWRP COLUMN HEADERS");
            vq.addParameter(VistaQuery.LITERAL, reportId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Expand columns for a report.
        /// RPC: ORWRP3 EXPAND COLUMNS
        /// </summary>
        [HttpGet, Route("api/report/expandcolumns")]
        public async Task<ActionResult<List<string>>> ExpandColumns(
            [FromQuery] string dfn,
            [FromQuery] string reportId,
            [FromQuery] string item)
        {
            var vq = new VistaQuery("ORWRP3 EXPAND COLUMNS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, reportId);
            vq.addParameter(VistaQuery.LITERAL, item);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save column widths for a user.
        /// RPC: ORWCH SAVECOL — takes ONE composite parameter
        /// "&lt;colName&gt;^&lt;w1,w2,w3,&gt;" per cprs/rReports.pas line 213
        /// (CallV('ORWCH SAVECOL', [aColumn])) and cprs/fReports.pas line 2486-2488.
        /// </summary>
        [HttpPost, Route("api/report/savecol")]
        public async Task<ActionResult<string>> SaveColumns(
            [FromQuery] string column)
        {
            var vq = new VistaQuery("ORWCH SAVECOL");
            vq.addParameter(VistaQuery.LITERAL, column ?? string.Empty);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get nutrition report list.
        /// RPC: ORWRP1 LISTNUTR (rReports.pas line 268: CallV('ORWRP1 LISTNUTR', [Patient.DFN]))
        /// </summary>
        [HttpGet, Route("api/report/nutrition")]
        public async Task<ActionResult<List<string>>> NutritionList([FromQuery] string dfn = "")
        {
            var vq = new VistaQuery("ORWRP1 LISTNUTR");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get surgery report list.
        /// RPC: ORWSR RPTLIST (rReports.pas line 288: CallV('ORWSR RPTLIST', [Patient.DFN]))
        /// </summary>
        [HttpGet, Route("api/report/surgerylist")]
        public async Task<ActionResult<List<string>>> SurgeryReportList([FromQuery] string dfn = "")
        {
            var vq = new VistaQuery("ORWSR RPTLIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Consult Reports

        /// <summary>
        /// Get list of consult reports.
        /// RPC: ORWCS LIST OF CONSULT REPORTS
        /// </summary>
        [HttpGet, Route("api/report/consultlist")]
        public async Task<ActionResult<List<string>>> ConsultReportList()
        {
            var vq = new VistaQuery("ORWCS LIST OF CONSULT REPORTS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get consult report text.
        /// RPC: ORWCS REPORT TEXT
        /// </summary>
        [HttpGet, Route("api/report/consulttext")]
        public async Task<ActionResult<List<string>>> ConsultReportText(
            [FromQuery] string dfn,
            [FromQuery] string reportId,
            [FromQuery] string consultIen,
            [FromQuery] string alpha = "",
            [FromQuery] string omega = "",
            [FromQuery] int nRpts = 0,
            [FromQuery] string ien = "")
        {
            var (a, o) = FileManDate.ToFMRange(alpha, omega);
            var vq = new VistaQuery("ORWCS REPORT TEXT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, reportId);
            vq.addParameter(VistaQuery.LITERAL, consultIen);
            vq.addParameter(VistaQuery.LITERAL, a);
            vq.addParameter(VistaQuery.LITERAL, o);
            vq.addParameter(VistaQuery.LITERAL, nRpts.ToString());
            if (!string.IsNullOrEmpty(ien))
                vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Imaging & Procedures

        /// <summary>
        /// Get imaging exams for a patient.
        /// RPC: ORWRA IMAGING EXAMS1
        /// </summary>
        [HttpGet, Route("api/report/imagingexams")]
        public async Task<ActionResult<List<string>>> ImagingExams(
            [FromQuery] string dfn,
            [FromQuery] string alpha = "",
            [FromQuery] string omega = "")
        {
            var (a, o) = FileManDate.ToFMRange(alpha, omega);
            var vq = new VistaQuery("ORWRA IMAGING EXAMS1");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            if (!string.IsNullOrEmpty(a))
            {
                vq.addParameter(VistaQuery.LITERAL, a);
                vq.addParameter(VistaQuery.LITERAL, o);
            }
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get patient procedures.
        /// RPC: ORWMC PATIENT PROCEDURES1
        /// </summary>
        [HttpGet, Route("api/report/patientprocedures")]
        public async Task<ActionResult<List<string>>> PatientProcedures(
            [FromQuery] string dfn,
            [FromQuery] string alpha = "",
            [FromQuery] string omega = "")
        {
            var (a, o) = FileManDate.ToFMRange(alpha, omega);
            var vq = new VistaQuery("ORWMC PATIENT PROCEDURES1");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            if (!string.IsNullOrEmpty(a))
            {
                vq.addParameter(VistaQuery.LITERAL, a);
                vq.addParameter(VistaQuery.LITERAL, o);
            }
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get image for a TIU progress note.
        /// RPC: ORWTPD GETIMG
        /// </summary>
        [HttpGet, Route("api/report/getimg")]
        public async Task<ActionResult<string>> GetImg([FromQuery] int noteIen)
        {
            var vq = new VistaQuery("ORWTPD GETIMG");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Printing

        /// <summary>
        /// Print a Vista report.
        /// RPC: ORWRP PRINT V REPORT
        /// </summary>
        [HttpPost, Route("api/report/printvreport")]
        public async Task<ActionResult<List<string>>> PrintVistaReport([FromBody] List<string> reportData)
        {
            var vq = new VistaQuery("ORWRP PRINT V REPORT");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < reportData.Count; i++)
                dhl.Add((i + 1).ToString(), reportData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Print a remote report.
        /// RPC: ORWRP PRINT REMOTE REPORT
        /// </summary>
        [HttpPost, Route("api/report/printremotereport")]
        public async Task<ActionResult<List<string>>> PrintRemoteReport([FromBody] List<string> reportData)
        {
            var vq = new VistaQuery("ORWRP PRINT REMOTE REPORT");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < reportData.Count; i++)
                dhl.Add((i + 1).ToString(), reportData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Print a report.
        /// RPC: ORWRP PRINT REPORT
        /// </summary>
        [HttpPost, Route("api/report/printreport")]
        public async Task<ActionResult<List<string>>> PrintReport([FromBody] List<string> reportData)
        {
            var vq = new VistaQuery("ORWRP PRINT REPORT");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < reportData.Count; i++)
                dhl.Add((i + 1).ToString(), reportData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Print a report to a Windows printer (remote).
        /// RPC: ORWRP PRINT WINDOWS REMOTE
        /// </summary>
        [HttpPost, Route("api/report/printwindowsremote")]
        public async Task<ActionResult<List<string>>> PrintWindowsRemote([FromBody] List<string> reportData)
        {
            var vq = new VistaQuery("ORWRP PRINT WINDOWS REMOTE");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < reportData.Count; i++)
                dhl.Add((i + 1).ToString(), reportData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Print a report to a Windows printer.
        /// RPC: ORWRP PRINT WINDOWS REPORT
        /// </summary>
        [HttpPost, Route("api/report/printwindowsreport")]
        public async Task<ActionResult<List<string>>> PrintWindowsReport([FromBody] List<string> reportData)
        {
            var vq = new VistaQuery("ORWRP PRINT WINDOWS REPORT");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < reportData.Count; i++)
                dhl.Add((i + 1).ToString(), reportData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the default Windows-print setting.
        /// RPC: ORWRP WINPRINT DEFAULT
        /// </summary>
        [HttpGet, Route("api/report/winprintdefault")]
        public async Task<ActionResult<string>> WinPrintDefault()
        {
            var vq = new VistaQuery("ORWRP WINPRINT DEFAULT");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save the default printer for reports.
        /// RPC: ORWRP SAVE DEFAULT PRINTER
        /// </summary>
        [HttpPost, Route("api/report/savedefaultprinter")]
        public async Task<ActionResult<string>> SaveDefaultPrinter([FromQuery] string printer)
        {
            var vq = new VistaQuery("ORWRP SAVE DEFAULT PRINTER");
            vq.addParameter(VistaQuery.LITERAL, printer);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Health Summary Components

        /// <summary>
        /// Health summary file lookup.
        /// RPC: ORWRP2 HS FILE LOOKUP
        /// </summary>
        [HttpGet, Route("api/report/hsfilelookup")]
        public async Task<ActionResult<List<string>>> HsFileLookup(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWRP2 HS FILE LOOKUP");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get health summary composite files.
        /// RPC: ORWRP2 HS COMP FILES
        /// </summary>
        [HttpGet, Route("api/report/hscompfiles")]
        public async Task<ActionResult<List<string>>> HsCompFiles([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORWRP2 HS COMP FILES");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get health summary sub-items.
        /// RPC: ORWRP2 HS SUBITEMS
        /// </summary>
        [HttpGet, Route("api/report/hssubitems")]
        public async Task<ActionResult<List<string>>> HsSubitems([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORWRP2 HS SUBITEMS");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get health summary report text.
        /// RPC: ORWRP2 HS REPORT TEXT
        /// </summary>
        [HttpGet, Route("api/report/hsreporttext")]
        public async Task<ActionResult<List<string>>> HsReportText(
            [FromQuery] string dfn,
            [FromQuery] string components,
            [FromQuery] string reportId = "")
        {
            var vq = new VistaQuery("ORWRP2 HS REPORT TEXT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, components);
            if (!string.IsNullOrEmpty(reportId))
                vq.addParameter(VistaQuery.LITERAL, reportId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get health summary components.
        /// RPC: ORWRP2 HS COMPONENTS
        /// </summary>
        [HttpGet, Route("api/report/hscomponents")]
        public async Task<ActionResult<List<string>>> HsComponents()
        {
            var vq = new VistaQuery("ORWRP2 HS COMPONENTS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get component abbreviation.
        /// RPC: ORWRP2 COMPABV
        /// </summary>
        [HttpGet, Route("api/report/compabv")]
        public async Task<ActionResult<string>> CompAbv([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORWRP2 COMPABV");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get component display info.
        /// RPC: ORWRP2 COMPDISP
        /// </summary>
        [HttpGet, Route("api/report/compdisp")]
        public async Task<ActionResult<List<string>>> CompDisp([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORWRP2 COMPDISP");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get health summary component sub-sections.
        /// RPC: ORWRP2 HS COMPONENT SUBS
        /// </summary>
        [HttpGet, Route("api/report/hscomponentsubs")]
        public async Task<ActionResult<List<string>>> HsComponentSubs([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORWRP2 HS COMPONENT SUBS");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get saved lookup value.
        /// RPC: ORWRP2 GETLKUP
        /// </summary>
        [HttpGet, Route("api/report/getlkup")]
        public async Task<ActionResult<string>> GetLookup()
        {
            var vq = new VistaQuery("ORWRP2 GETLKUP");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save lookup value.
        /// RPC: ORWRP2 SAVLKUP
        /// </summary>
        [HttpPost, Route("api/report/savlkup")]
        public async Task<ActionResult<string>> SaveLookup([FromQuery] string value)
        {
            var vq = new VistaQuery("ORWRP2 SAVLKUP");
            vq.addParameter(VistaQuery.LITERAL, value);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Remote Data (CIRN / HDR / XWB)

        /// <summary>
        /// Check if auto-remote data view is on.
        /// RPC: ORWCIRN AUTORDV
        /// </summary>
        [HttpGet, Route("api/report/autordv")]
        public async Task<ActionResult<string>> AutoRdv()
        {
            var vq = new VistaQuery("ORWCIRN AUTORDV");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if HDR (Health Data Repository) is enabled.
        /// RPC: ORWCIRN HDRON
        /// </summary>
        [HttpGet, Route("api/report/hdron")]
        public async Task<ActionResult<string>> HdrOn()
        {
            var vq = new VistaQuery("ORWCIRN HDRON");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// HDR modify parameters.
        /// RPC: ORWRP4 HDR MODIFY
        /// </summary>
        [HttpPost, Route("api/report/hdrmodify")]
        public async Task<ActionResult<List<string>>> HdrModify(
            [FromQuery] string dfn,
            [FromBody] List<string> hdrParams)
        {
            var vq = new VistaQuery("ORWRP4 HDR MODIFY");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < hdrParams.Count; i++)
                dhl.Add((i + 1).ToString(), hdrParams[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Clear all deferred remote calls.
        /// RPC: XWB DEFERRED CLEARALL
        /// </summary>
        [HttpPost, Route("api/report/deferredclearall")]
        public async Task<ActionResult<string>> DeferredClearAll()
        {
            var vq = new VistaQuery("XWB DEFERRED CLEARALL");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Execute a remote RPC call.
        /// RPC: XWB REMOTE RPC
        /// </summary>
        [HttpPost, Route("api/report/remoterpc")]
        public async Task<ActionResult<List<string>>> RemoteRpc([FromBody] List<string> rpcData)
        {
            var vq = new VistaQuery("XWB REMOTE RPC");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < rpcData.Count; i++)
                dhl.Add((i + 1).ToString(), rpcData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Execute a direct RPC call.
        /// RPC: XWB DIRECT RPC
        /// </summary>
        [HttpPost, Route("api/report/directrpc")]
        public async Task<ActionResult<List<string>>> DirectRpc([FromBody] List<string> rpcData)
        {
            var vq = new VistaQuery("XWB DIRECT RPC");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < rpcData.Count; i++)
                dhl.Add((i + 1).ToString(), rpcData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check status of a remote RPC call.
        /// RPC: XWB REMOTE STATUS CHECK
        /// </summary>
        [HttpGet, Route("api/report/remotestatuscheck")]
        public async Task<ActionResult<List<string>>> RemoteStatusCheck([FromQuery] string handle)
        {
            var vq = new VistaQuery("XWB REMOTE STATUS CHECK");
            vq.addParameter(VistaQuery.LITERAL, handle);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get data from a completed remote call.
        /// RPC: XWB REMOTE GETDATA
        /// </summary>
        [HttpGet, Route("api/report/remotegetdata")]
        public async Task<ActionResult<List<string>>> RemoteGetData([FromQuery] string handle)
        {
            var vq = new VistaQuery("XWB REMOTE GETDATA");
            vq.addParameter(VistaQuery.LITERAL, handle);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Simple convenience endpoints

        /// <summary>
        /// Run a report. RPC: uses the rpcName from the report definition (default: ORWRP REPORT TEXT).
        /// The reportId parameter should already be in the format "ID~HSTag" as constructed by the portal.
        /// Matches LoadReportText in cprs/rReports.pas:
        ///   AReport := ReportType + '~' + AHSTag
        ///   CallV(ARpc, [Patient.DFN, AReport, HSType, DaysBack, ExamID, Alpha, Omega])
        ///
        /// Phase 3 additions:
        /// - alpha/omega run through <see cref="FileManDate.ToFMRange"/> so callers may
        ///   pass ISO dates, T-relative shorthand, or already-FM values.
        /// - maxOcc, when supplied, is substituted into HSTag piece 4 per
        ///   rReports.pas line 320 / 327 + fReports.pas line 2480.
        /// </summary>
        [HttpGet, Route("api/report/run")]
        public async Task<ActionResult<List<string>>> RunReport(
            [FromQuery] string dfn, [FromQuery] string reportId,
            [FromQuery] string hsTag = "", [FromQuery] string hsType = "",
            [FromQuery] string daysBack = "", [FromQuery] string examId = "",
            [FromQuery] string alpha = "", [FromQuery] string omega = "",
            [FromQuery] string rpcName = "", [FromQuery] string maxOcc = "")
        {
            // FileMan date normalization + alpha/omega swap (rReports.pas line 314)
            var (a, o) = FileManDate.ToFMRange(alpha, omega);

            // MaxOcc substitution into HSTag piece 4 when caller asked for an
            // override; empty string clears the limit (chkMaxFreq behavior).
            var effectiveHsTag = hsTag ?? "";
            if (maxOcc != null)
            {
                var pieces = effectiveHsTag.Split('^');
                if (pieces.Length < 4) Array.Resize(ref pieces, 4);
                pieces[3] = maxOcc;
                effectiveHsTag = string.Join("^", pieces);
            }

            // Build the compound report identifier: ID~HSTag (matches CPRS)
            var rpc = string.IsNullOrEmpty(rpcName) ? "ORWRP REPORT TEXT" : rpcName;
            var aReport = string.IsNullOrEmpty(effectiveHsTag) ? reportId : reportId + "~" + effectiveHsTag;

            var vq = new VistaQuery(rpc);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, aReport);
            vq.addParameter(VistaQuery.LITERAL, hsType);
            vq.addParameter(VistaQuery.LITERAL, daysBack);
            vq.addParameter(VistaQuery.LITERAL, examId);
            vq.addParameter(VistaQuery.LITERAL, a);
            vq.addParameter(VistaQuery.LITERAL, o);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Table data for a report. RPC: ORWRP REPORT TEXT</summary>
        [HttpGet, Route("api/report/tabledata")]
        public async Task<ActionResult<List<string>>> TableData(
            [FromQuery] string dfn, [FromQuery] string reportId)
        {
            var vq = new VistaQuery("ORWRP REPORT TEXT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, reportId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Run adhoc report. RPC: ORWRP2 HS REPORT TEXT</summary>
        [HttpPost, Route("api/report/runadhoc")]
        public async Task<ActionResult<string>> RunAdhoc(
            [FromQuery] string dfn, [FromQuery] string components = "",
            [FromQuery] string dateFrom = "", [FromQuery] string dateTo = "")
        {
            var vq = new VistaQuery("ORWRP2 HS REPORT TEXT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, components);
            vq.addParameter(VistaQuery.LITERAL, dateFrom);
            vq.addParameter(VistaQuery.LITERAL, dateTo);
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
