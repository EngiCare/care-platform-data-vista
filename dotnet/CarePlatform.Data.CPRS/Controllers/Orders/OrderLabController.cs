// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Lab order entry dialogs — collection, specimens, samples, blood bank.
    /// Migrated from cprs/Orders/rODLab.pas — ORWDLR32/33, ORWDXVB, ORCDLR2 RPCs.
    /// </summary>
    public class OrderLabController : BaseController
    {
        public OrderLabController() : base() { }

        #region Lab Dialog Defaults

        [HttpGet, Route("api/orderlab/defaults")]
        public async Task<ActionResult<List<string>>> ODForLab(
            [FromQuery] long location, [FromQuery] int division = 0)
        {
            var vq = new VistaQuery("ORWDLR32 DEF");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, division.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderlab/loadtest")]
        public async Task<ActionResult<List<string>>> LoadLabTestData([FromQuery] string labTestIen)
        {
            var vq = new VistaQuery("ORWDLR32 LOAD");
            vq.addParameter(VistaQuery.LITERAL, labTestIen);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Samples & Specimens

        [HttpGet, Route("api/orderlab/allsamples")]
        public async Task<ActionResult<List<string>>> AllSamples()
        {
            var vq = new VistaQuery("ORWDLR32 ALLSAMP");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderlab/allspecimens")]
        public async Task<ActionResult<List<string>>> AllSpecimens(
            [FromQuery] string startFrom, [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWDLR32 ALLSPEC");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderlab/abbrevspecimens")]
        public async Task<ActionResult<List<string>>> AbbrevSpecimens()
        {
            var vq = new VistaQuery("ORWDLR32 ABBSPEC");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderlab/onesample")]
        public async Task<ActionResult<List<string>>> OneSample([FromQuery] int sampleIen)
        {
            var vq = new VistaQuery("ORWDLR32 ONE SAMPLE");
            vq.addParameter(VistaQuery.LITERAL, sampleIen.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderlab/onespecimen")]
        public async Task<ActionResult<string>> OneSpecimen([FromQuery] int specimenIen)
        {
            var vq = new VistaQuery("ORWDLR32 ONE SPECIMEN");
            vq.addParameter(VistaQuery.LITERAL, specimenIen.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Collection Times / Scheduling

        [HttpGet, Route("api/orderlab/stopdate")]
        public async Task<ActionResult<string>> CalcStopDate([FromQuery] string text)
        {
            var vq = new VistaQuery("ORWDLR32 STOP");
            vq.addParameter(VistaQuery.LITERAL, text);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderlab/maxdays")]
        public async Task<ActionResult<string>> MaxDays(
            [FromQuery] long location, [FromQuery] int schedule)
        {
            var vq = new VistaQuery("ORWDLR32 MAXDAYS");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, schedule.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderlab/iscollecttime")]
        public async Task<ActionResult<string>> IsLabCollectTime(
            [FromQuery] string dateTime, [FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDLR32 LAB COLL TIME");
            vq.addParameter(VistaQuery.LITERAL, dateTime);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderlab/futurecollectdays")]
        public async Task<ActionResult<string>> FutureCollectDays(
            [FromQuery] long location, [FromQuery] int division)
        {
            var vq = new VistaQuery("ORWDLR33 FUTURE LAB COLLECTS");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, division.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderlab/immedcollecttimes")]
        public async Task<ActionResult<List<string>>> ImmediateCollectTimes()
        {
            var vq = new VistaQuery("ORWDLR32 IMMED COLLECT");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderlab/defaultimmedcolltime")]
        public async Task<ActionResult<string>> DefaultImmediateCollectTime()
        {
            var vq = new VistaQuery("ORWDLR32 IC DEFAULT");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderlab/validimmedcolltime")]
        public async Task<ActionResult<string>> ValidImmediateCollectTime([FromQuery] string collTime)
        {
            var vq = new VistaQuery("ORWDLR32 IC VALID");
            vq.addParameter(VistaQuery.LITERAL, collTime);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderlab/labtimes")]
        public async Task<ActionResult<List<string>>> GetLabTimes(
            [FromQuery] string labDate, [FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDLR32 GET LAB TIMES");
            vq.addParameter(VistaQuery.LITERAL, labDate);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderlab/lastcollectiontime")]
        public async Task<ActionResult<string>> LastCollectionTime()
        {
            var vq = new VistaQuery("ORWDLR33 LASTTIME");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderlab/lctowcinstructions")]
        public async Task<ActionResult<string>> LCtoWCInstructions([FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDLR33 LC TO WC");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region LC to WC Checks

        [HttpPost, Route("api/orderlab/checkonelargetowardcoll")]
        public async Task<ActionResult<List<string>>> CheckOneLCtoWC(
            [FromQuery] long location, [FromQuery] string startDate,
            [FromQuery] string collType, [FromQuery] string schedule,
            [FromQuery] string duration)
        {
            var vq = new VistaQuery("ORCDLR2 CHECK ONE LC TO WC");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, "");
            vq.addParameter(VistaQuery.LITERAL, startDate);
            vq.addParameter(VistaQuery.LITERAL, collType);
            vq.addParameter(VistaQuery.LITERAL, schedule);
            vq.addParameter(VistaQuery.LITERAL, duration);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/orderlab/checkalllargetowardcoll")]
        public async Task<ActionResult<List<string>>> CheckAllLCtoWC(
            [FromQuery] long location, [FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORCDLR2 CHECK ALL LC TO WC");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Blood Bank

        [HttpGet, Route("api/orderlab/bloodcomponents")]
        public async Task<ActionResult<List<string>>> BloodComponents()
        {
            var vq = new VistaQuery("ORWDXVB COMPORD");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderlab/diagnostictests")]
        public async Task<ActionResult<List<string>>> DiagnosticTests()
        {
            var vq = new VistaQuery("ORWDXVB3 DIAGORD");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderlab/nursadminsuppress")]
        public async Task<ActionResult<string>> NursAdminSuppress()
        {
            var vq = new VistaQuery("ORWDXVB NURSADMN");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderlab/statallowed")]
        public async Task<ActionResult<string>> StatAllowed([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWDXVB STATALOW");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderlab/removecolldefault")]
        public async Task<ActionResult<string>> RemoveCollTimeDefault()
        {
            var vq = new VistaQuery("ORWDXVB3 COLLTIM");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderlab/swappanel")]
        public async Task<ActionResult<string>> SwapPanelLocation()
        {
            var vq = new VistaQuery("ORWDXVB3 SWPANEL");
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/orderlab/bloodresultsraw")]
        public async Task<ActionResult<List<string>>> GetPatientBloodResultsRaw(
            [FromQuery] string dfn, [FromBody] List<string> tests)
        {
            var vq = new VistaQuery("ORWDXVB RAW");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < tests.Count; i++)
                dhl.Add(i.ToString(), tests[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/orderlab/bloodresults")]
        public async Task<ActionResult<List<string>>> GetPatientBloodResults(
            [FromQuery] string dfn, [FromBody] List<string> tests)
        {
            var vq = new VistaQuery("ORWDXVB RESULTS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < tests.Count; i++)
                dhl.Add(i.ToString(), tests[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderlab/patientbbinfo")]
        public async Task<ActionResult<List<string>>> GetPatientBBInfo(
            [FromQuery] string dfn, [FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDXVB GETALL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderlab/subtype")]
        public async Task<ActionResult<string>> GetSubType([FromQuery] string testName)
        {
            var vq = new VistaQuery("ORWDXVB SUBCHK");
            vq.addParameter(VistaQuery.LITERAL, testName);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderlab/tnsdaysback")]
        public async Task<ActionResult<string>> TNSDaysBack()
        {
            var vq = new VistaQuery("ORWDXVB VBTNS");
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
