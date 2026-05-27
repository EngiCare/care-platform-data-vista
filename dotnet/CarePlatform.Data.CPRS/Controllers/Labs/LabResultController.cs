// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Lab results — atomics, specimens, tests, grids, worksheets, charts, interim,
    /// cumulative, micro, reports, printing.
    /// Migrated from cprs/rLabs.pas — ORWLRR *, ORWRP * RPCs.
    /// </summary>
    public class LabResultController : BaseController
    {
        public LabResultController() : base() { }

        #region Lookups (Tests, Specimens, Users)

        /// <summary>
        /// Long list of atomic (individual) lab tests.
        /// RPC: ORWLRR ATOMICS
        /// </summary>
        [HttpGet, Route("api/lab/atomics")]
        public async Task<ActionResult<List<string>>> Atomics(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWLRR ATOMICS");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Long list of specimens.
        /// RPC: ORWLRR SPEC
        /// </summary>
        [HttpGet, Route("api/lab/specimens")]
        public async Task<ActionResult<List<string>>> Specimens(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWLRR SPEC");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Long list of all tests.
        /// RPC: ORWLRR ALLTESTS
        /// </summary>
        [HttpGet, Route("api/lab/alltests")]
        public async Task<ActionResult<List<string>>> AllTests(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWLRR ALLTESTS");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Long list of chemistry tests.
        /// RPC: ORWLRR CHEMTEST
        /// </summary>
        [HttpGet, Route("api/lab/chemtests")]
        public async Task<ActionResult<List<string>>> ChemTests(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWLRR CHEMTEST");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Long list of lab users.
        /// RPC: ORWLRR USERS
        /// </summary>
        [HttpGet, Route("api/lab/users")]
        public async Task<ActionResult<List<string>>> Users(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWLRR USERS");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Specimen default IDs (blood, urine, serum, plasma).
        /// RPC: ORWLRR PARAM
        /// Returns: blood^urine^serum^plasma
        /// </summary>
        [HttpGet, Route("api/lab/specimendefaults")]
        public async Task<ActionResult<string>> SpecimenDefaults()
        {
            var vq = new VistaQuery("ORWLRR PARAM");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get info for a single test.
        /// RPC: ORWLRR INFO
        /// </summary>
        [HttpGet, Route("api/lab/testinfo")]
        public async Task<ActionResult<List<string>>> TestInfo([FromQuery] string test)
        {
            var vq = new VistaQuery("ORWLRR INFO");
            vq.addParameter(VistaQuery.LITERAL, test);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Test Groups

        /// <summary>
        /// Get user-defined test groups.
        /// RPC: ORWLRR TG
        /// </summary>
        [HttpGet, Route("api/lab/testgroups")]
        public async Task<ActionResult<List<string>>> TestGroups([FromQuery] long user = 0)
        {
            var vq = new VistaQuery("ORWLRR TG");
            vq.addParameter(VistaQuery.LITERAL, user.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get tests in a specific atomic test.
        /// RPC: ORWLRR ATESTS
        /// </summary>
        [HttpGet, Route("api/lab/atest")]
        public async Task<ActionResult<List<string>>> ATest([FromQuery] int test)
        {
            var vq = new VistaQuery("ORWLRR ATESTS");
            vq.addParameter(VistaQuery.LITERAL, test.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get tests in a test group.
        /// RPC: ORWLRR ATG
        /// </summary>
        [HttpGet, Route("api/lab/atestgroup")]
        public async Task<ActionResult<List<string>>> ATestGroup(
            [FromQuery] int testGroup,
            [FromQuery] long user)
        {
            var vq = new VistaQuery("ORWLRR ATG");
            vq.addParameter(VistaQuery.LITERAL, testGroup.ToString());
            vq.addParameter(VistaQuery.LITERAL, user.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Add a user test group.
        /// RPC: ORWLRR UTGA
        /// </summary>
        [HttpPost, Route("api/lab/testgroup/add")]
        public async Task<ActionResult<string>> TestGroupAdd([FromBody] List<string> tests)
        {
            var vq = new VistaQuery("ORWLRR UTGA");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < tests.Count; i++)
                dhl.Add(i.ToString(), tests[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Replace a user test group.
        /// RPC: ORWLRR UTGR
        /// </summary>
        [HttpPost, Route("api/lab/testgroup/replace")]
        public async Task<ActionResult<string>> TestGroupReplace(
            [FromBody] List<string> tests,
            [FromQuery] int testGroup)
        {
            var vq = new VistaQuery("ORWLRR UTGR");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < tests.Count; i++)
                dhl.Add(i.ToString(), tests[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, testGroup.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Delete a user test group.
        /// RPC: ORWLRR UTGD
        /// </summary>
        [HttpPost, Route("api/lab/testgroup/delete")]
        public async Task<ActionResult<string>> TestGroupDelete([FromQuery] int testGroup)
        {
            var vq = new VistaQuery("ORWLRR UTGD");
            vq.addParameter(VistaQuery.LITERAL, testGroup.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Results (Cumulative, Interim, Micro)

        /// <summary>
        /// Get cumulative lab results.
        /// RPC: dynamic (passed via rpc param, typically ORWLRR CUMULATIVE)
        /// </summary>
        [HttpGet, Route("api/lab/cumulative")]
        public async Task<ActionResult<List<string>>> Cumulative(
            [FromQuery] string dfn,
            [FromQuery] int daysBack = 365,
            [FromQuery] string date1 = "",
            [FromQuery] string date2 = "",
            [FromQuery] string rpc = "ORWLR CUMULATIVE REPORT")
        {
            var vq = new VistaQuery(rpc);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, daysBack.ToString());
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get interim lab results.
        /// RPC: dynamic (passed via rpc param, typically ORWLRR INTERIM)
        /// </summary>
        [HttpGet, Route("api/lab/interim")]
        public async Task<ActionResult<List<string>>> Interim(
            [FromQuery] string dfn,
            [FromQuery] string date1 = "",
            [FromQuery] string date2 = "",
            [FromQuery] string rpc = "ORWLR INTERIM REPORT")
        {
            var vq = new VistaQuery(rpc);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get interim lab results with selected tests.
        /// RPC: ORWLRR INTERIMS
        /// </summary>
        [HttpPost, Route("api/lab/interimselect")]
        public async Task<ActionResult<List<string>>> InterimSelect(
            [FromQuery] string dfn,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromBody] List<string> tests)
        {
            var vq = new VistaQuery("ORWLRR INTERIMS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < tests.Count; i++)
                dhl.Add(i.ToString(), tests[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get interim results in grid format.
        /// RPC: ORWLRR INTERIMG
        /// </summary>
        [HttpGet, Route("api/lab/interimgrid")]
        public async Task<ActionResult<List<string>>> InterimGrid(
            [FromQuery] string dfn,
            [FromQuery] string date1,
            [FromQuery] int direction,
            [FromQuery] int format)
        {
            var vq = new VistaQuery("ORWLRR INTERIMG");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            vq.addParameter(VistaQuery.LITERAL, format.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get microbiology results.
        /// RPC: dynamic (passed via rpc param, typically ORWLRR MICRO)
        /// </summary>
        [HttpGet, Route("api/lab/micro")]
        public async Task<ActionResult<List<string>>> Micro(
            [FromQuery] string dfn,
            [FromQuery] string date1 = "",
            [FromQuery] string date2 = "",
            [FromQuery] string rpc = "ORWLR MICRO REPORT")
        {
            var vq = new VistaQuery(rpc);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Worksheet & Chart

        /// <summary>
        /// Get lab worksheet data.
        /// RPC: ORWLRR GRID
        /// </summary>
        [HttpPost, Route("api/lab/worksheet")]
        public async Task<ActionResult<List<string>>> Worksheet(
            [FromQuery] string dfn,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromQuery] string spec,
            [FromBody] List<string> tests)
        {
            var vq = new VistaQuery("ORWLRR GRID");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            vq.addParameter(VistaQuery.LITERAL, spec);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < tests.Count; i++)
                dhl.Add(i.ToString(), tests[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get chart data for a single test.
        /// RPC: ORWLRR CHART
        /// </summary>
        [HttpGet, Route("api/lab/chart")]
        public async Task<ActionResult<List<string>>> Chart(
            [FromQuery] string dfn,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromQuery] string spec,
            [FromQuery] string test)
        {
            var vq = new VistaQuery("ORWLRR CHART");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            vq.addParameter(VistaQuery.LITERAL, spec);
            vq.addParameter(VistaQuery.LITERAL, test);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get newest and oldest lab dates for a patient.
        /// RPC: ORWLRR NEWOLD
        /// Returns: newest^oldest
        /// </summary>
        [HttpGet, Route("api/lab/daterange")]
        public async Task<ActionResult<string>> DateRange([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWLRR NEWOLD");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Reports & Printing

        /// <summary>
        /// Get a lab report by report ID.
        /// RPC: dynamic (passed via rpc param)
        /// </summary>
        [HttpGet, Route("api/lab/report")]
        public async Task<ActionResult<List<string>>> Report(
            [FromQuery] string dfn,
            [FromQuery] string reportId,
            [FromQuery] string hsType,
            [FromQuery] string date,
            [FromQuery] string section,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromQuery] string rpc)
        {
            var vq = new VistaQuery(rpc);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, reportId);
            vq.addParameter(VistaQuery.LITERAL, hsType);
            vq.addParameter(VistaQuery.LITERAL, date);
            vq.addParameter(VistaQuery.LITERAL, section);
            vq.addParameter(VistaQuery.LITERAL, date2);
            vq.addParameter(VistaQuery.LITERAL, date1);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Print lab reports to a device.
        /// RPC: ORWRP PRINT LAB REPORTS
        /// Returns: '0' on success, '1'^errorMsg on failure
        /// </summary>
        [HttpPost, Route("api/lab/print")]
        public async Task<ActionResult<string>> Print(
            [FromQuery] string device,
            [FromQuery] string dfn,
            [FromQuery] string report,
            [FromQuery] int daysBack,
            [FromBody] List<string> tests,
            [FromQuery] string date1 = "",
            [FromQuery] string date2 = "")
        {
            var vq = new VistaQuery("ORWRP PRINT LAB REPORTS");
            vq.addParameter(VistaQuery.LITERAL, device);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, report);
            vq.addParameter(VistaQuery.LITERAL, daysBack.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < tests.Count; i++)
                dhl.Add(i.ToString(), tests[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, date2);
            vq.addParameter(VistaQuery.LITERAL, date1);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get Windows-formatted lab report for local printing.
        /// RPC: ORWRP WINPRINT LAB REPORTS
        /// </summary>
        [HttpPost, Route("api/lab/formattedreport")]
        public async Task<ActionResult<List<string>>> FormattedReport(
            [FromQuery] string dfn,
            [FromQuery] string report,
            [FromQuery] int daysBack,
            [FromBody] List<string> tests,
            [FromQuery] string date1 = "",
            [FromQuery] string date2 = "")
        {
            var vq = new VistaQuery("ORWRP WINPRINT LAB REPORTS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, report);
            vq.addParameter(VistaQuery.LITERAL, daysBack.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < tests.Count; i++)
                dhl.Add(i.ToString(), tests[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, date2);
            vq.addParameter(VistaQuery.LITERAL, date1);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if radio button display is configured.
        /// RPC: ORWRP1A RADIO
        /// Returns: '1' if radio buttons should be used
        /// </summary>
        [HttpGet, Route("api/lab/useradiobuttons")]
        public async Task<ActionResult<string>> UseRadioButtons()
        {
            var vq = new VistaQuery("ORWRP1A RADIO");
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Remote Lab

        /// <summary>
        /// Get remote lab cumulative results via XWB REMOTE RPC.
        /// RPC: XWB REMOTE RPC (calling remote cumulative)
        /// </summary>
        [HttpGet, Route("api/lab/remote/cumulative")]
        public async Task<ActionResult<List<string>>> RemoteCumulative(
            [FromQuery] string dfn,
            [FromQuery] int daysBack,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromQuery] string site,
            [FromQuery] string remoteRpc)
        {
            var vq = new VistaQuery("XWB REMOTE RPC");
            vq.addParameter(VistaQuery.LITERAL, site);
            vq.addParameter(VistaQuery.LITERAL, remoteRpc);
            vq.addParameter(VistaQuery.LITERAL, "0");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, daysBack.ToString());
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get remote lab interim/micro results via XWB REMOTE RPC.
        /// RPC: XWB REMOTE RPC (calling remote interim or micro)
        /// </summary>
        [HttpGet, Route("api/lab/remote/interim")]
        public async Task<ActionResult<List<string>>> RemoteInterim(
            [FromQuery] string dfn,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromQuery] string site,
            [FromQuery] string remoteRpc)
        {
            var vq = new VistaQuery("XWB REMOTE RPC");
            vq.addParameter(VistaQuery.LITERAL, site);
            vq.addParameter(VistaQuery.LITERAL, remoteRpc);
            vq.addParameter(VistaQuery.LITERAL, "0");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get remote lab reports via XWB REMOTE RPC.
        /// RPC: XWB REMOTE RPC (calling remote report)
        /// </summary>
        [HttpGet, Route("api/lab/remote/report")]
        public async Task<ActionResult<List<string>>> RemoteReport(
            [FromQuery] string dfn,
            [FromQuery] string reportId,
            [FromQuery] string hsType,
            [FromQuery] string date,
            [FromQuery] string section,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromQuery] string site,
            [FromQuery] string remoteRpc)
        {
            var vq = new VistaQuery("XWB REMOTE RPC");
            vq.addParameter(VistaQuery.LITERAL, site);
            vq.addParameter(VistaQuery.LITERAL, remoteRpc);
            vq.addParameter(VistaQuery.LITERAL, "0");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, reportId + ";1");
            vq.addParameter(VistaQuery.LITERAL, hsType);
            vq.addParameter(VistaQuery.LITERAL, date);
            vq.addParameter(VistaQuery.LITERAL, section);
            vq.addParameter(VistaQuery.LITERAL, date2);
            vq.addParameter(VistaQuery.LITERAL, date1);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Print remote lab reports to device.
        /// RPC: ORWRP PRINT LAB REMOTE
        /// </summary>
        [HttpPost, Route("api/lab/remote/print")]
        public async Task<ActionResult<string>> RemotePrint(
            [FromQuery] string device,
            [FromQuery] string dfn,
            [FromQuery] string report,
            [FromBody] List<string> handles)
        {
            var vq = new VistaQuery("ORWRP PRINT LAB REMOTE");
            vq.addParameter(VistaQuery.LITERAL, device);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, report);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < handles.Count; i++)
                dhl.Add(i.ToString(), handles[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get Windows-formatted remote lab report.
        /// RPC: ORWRP PRINT WINDOWS LAB REMOTE
        /// </summary>
        [HttpPost, Route("api/lab/remote/formattedreport")]
        public async Task<ActionResult<List<string>>> RemoteFormattedReport(
            [FromQuery] string dfn,
            [FromQuery] string report,
            [FromBody] List<string> handles)
        {
            var vq = new VistaQuery("ORWRP PRINT WINDOWS LAB REMOTE");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, report);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < handles.Count; i++)
                dhl.Add(i.ToString(), handles[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Cover Sheet — Recent Labs

        /// <summary>
        /// Get recent lab results for cover sheet display.
        /// Called by CPRS Web CoverSheet for the "Recent Labs" widget.
        /// RPC: ORWCV LAB (structured caret-delimited data, matches CPRS desktop)
        /// </summary>
        [HttpGet, Route("api/lab/recent")]
        public async Task<ActionResult<List<string>>> Recent(
            [FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWCV LAB");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Simple convenience endpoints

        /// <summary>Graph data. RPC: ORWLR GRAPH DATA</summary>
        [HttpGet, Route("api/lab/graphdata")]
        public async Task<ActionResult<List<string>>> GraphData([FromQuery] string dfn, [FromQuery] string test = "", [FromQuery] int daysBack = 365)
        {
            var vq = new VistaQuery("ORWLR GRAPH DATA");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, test);
            vq.addParameter(VistaQuery.LITERAL, daysBack.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>Worksheet. RPC: ORWLR WORKSHEET</summary>
        [HttpGet, Route("api/lab/worksheet")]
        public async Task<ActionResult<List<string>>> WsWorksheet([FromQuery] string dfn, [FromQuery] int daysBack = 365)
        {
            var vq = new VistaQuery("ORWLR WORKSHEET");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, daysBack.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>Reference ranges. RPC: ORWLR REF RANGES</summary>
        [HttpGet, Route("api/lab/refranges")]
        public async Task<ActionResult<List<string>>> RefRanges([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWLR REF RANGES");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Categories. RPC: ORWLR CUMULATIVE CATEGORIES</summary>
        [HttpGet, Route("api/lab/categories")]
        public async Task<ActionResult<List<string>>> Categories([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWLR CUMULATIVE CATEGORIES");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Result detail. RPC: ORWLR RESULT DETAIL</summary>
        [HttpGet, Route("api/lab/resultdetail")]
        public async Task<ActionResult<List<string>>> ResultDetail([FromQuery] string dfn, [FromQuery] string test = "", [FromQuery] string date = "")
        {
            var vq = new VistaQuery("ORWLR RESULT DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, test);
            vq.addParameter(VistaQuery.LITERAL, date);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Test group names. RPC: ORWLR TEST GROUP NAMES</summary>
        [HttpGet, Route("api/lab/testgroupnames")]
        public async Task<ActionResult<List<string>>> TestGroupNames()
        {
            var vq = new VistaQuery("ORWLR TEST GROUP NAMES");
            return await this.Session.tQuery(vq);
        }

        /// <summary>Tests in a group. RPC: ORWLR GROUP TESTS</summary>
        [HttpGet, Route("api/lab/grouptests")]
        public async Task<ActionResult<List<string>>> GroupTests([FromQuery] string name = "")
        {
            var vq = new VistaQuery("ORWLR GROUP TESTS");
            vq.addParameter(VistaQuery.LITERAL, name);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Save test group. RPC: ORWLR SAVE TEST GROUP</summary>
        [HttpPost, Route("api/lab/savetestgroup")]
        public async Task<ActionResult<string>> SaveTestGroup([FromQuery] string groupName = "", [FromQuery] string tests = "")
        {
            var vq = new VistaQuery("ORWLR SAVE TEST GROUP");
            vq.addParameter(VistaQuery.LITERAL, groupName);
            vq.addParameter(VistaQuery.LITERAL, tests);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Delete test group. RPC: ORWLR DELETE TEST GROUP</summary>
        [HttpPost, Route("api/lab/deletetestgroup")]
        public async Task<ActionResult<string>> WsDeleteTestGroup([FromQuery] string name = "")
        {
            var vq = new VistaQuery("ORWLR DELETE TEST GROUP");
            vq.addParameter(VistaQuery.LITERAL, name);
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
