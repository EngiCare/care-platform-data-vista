using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Vitals — last vitals, vitals by note/date, rate validation, store, grid, detail.
    /// Migrated from cprs/Vitals/rVitals.pas + cprs/fVitals.pas —
    /// ORQQVI *, ORQQVI2 *, GMV ORQQVI1 * RPCs.
    /// </summary>
    public class VitalsController : BaseController
    {
        public VitalsController() : base() { }

        /// <summary>
        /// Get the most recent vitals for a patient (or vitals at a specific encounter date).
        /// RPC: ORQQVI VITALS
        /// </summary>
        [HttpGet, Route("api/vitals/latest")]
        public async Task<ActionResult<List<string>>> Latest(
            [FromQuery] string dfn,
            [FromQuery] string encDate = "")
        {
            var vq = new VistaQuery("ORQQVI VITALS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            if (!string.IsNullOrEmpty(encDate))
                vq.addParameter(VistaQuery.LITERAL, encDate);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get vitals associated with a specific TIU note.
        /// RPC: ORQQVI NOTEVIT
        /// </summary>
        [HttpGet, Route("api/vitals/bynote")]
        public async Task<ActionResult<List<string>>> ByNote(
            [FromQuery] string dfn,
            [FromQuery] int noteIen)
        {
            var vq = new VistaQuery("ORQQVI NOTEVIT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Verify a vital value (rate check).
        /// RPC: ORQQVI2 VITALS RATE CHECK
        /// Returns: '1' if valid, '0' if invalid
        /// </summary>
        [HttpGet, Route("api/vitals/ratecheck")]
        public async Task<ActionResult<string>> RateCheck(
            [FromQuery] string type,
            [FromQuery] string rate,
            [FromQuery] string unit)
        {
            var vq = new VistaQuery("ORQQVI2 VITALS RATE CHECK");
            vq.addParameter(VistaQuery.LITERAL, type);
            vq.addParameter(VistaQuery.LITERAL, rate);
            vq.addParameter(VistaQuery.LITERAL, unit);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Validate and store vitals.
        /// RPC: ORQQVI2 VITALS VAL &amp; STORE
        /// Returns: 'True' if stored successfully, or error message describing failed value.
        /// </summary>
        [HttpPost, Route("api/vitals/store")]
        public async Task<ActionResult<List<string>>> Store([FromBody] List<string> vitalList)
        {
            var vq = new VistaQuery("ORQQVI2 VITALS VAL & STORE");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < vitalList.Count; i++)
                dhl.Add(i.ToString(), vitalList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get vitals grid data for a date range.
        /// RPC: GMV ORQQVI1 GRID
        /// </summary>
        [HttpGet, Route("api/vitals/grid")]
        public async Task<ActionResult<List<string>>> Grid(
            [FromQuery] string dfn,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromQuery] int restrictDates = 0,
            [FromQuery] string tests = "")
        {
            var vq = new VistaQuery("GMV ORQQVI1 GRID");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            vq.addParameter(VistaQuery.LITERAL, restrictDates.ToString());
            vq.addParameter(VistaQuery.LITERAL, tests);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get vitals detail/report for a date range.
        /// RPC: GMV ORQQVI1 DETAIL
        /// </summary>
        [HttpGet, Route("api/vitals/detail")]
        public async Task<ActionResult<List<string>>> Detail(
            [FromQuery] string dfn,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromQuery] string tests = "")
        {
            var vq = new VistaQuery("GMV ORQQVI1 DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            vq.addParameter(VistaQuery.LITERAL, "0"); // reserved
            vq.addParameter(VistaQuery.LITERAL, tests);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get cover-sheet vital detail (by type).
        /// RPC: GMV ORQQVI1 DETAIL
        /// </summary>
        [HttpGet, Route("api/vitals/coverdetail")]
        public async Task<ActionResult<List<string>>> CoverDetail(
            [FromQuery] string dfn,
            [FromQuery] string type = "")
        {
            var vq = new VistaQuery("GMV ORQQVI1 DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            vq.addParameter(VistaQuery.LITERAL, type);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Mark a vital as entered-in-error.
        /// RPC: GMV MARK ERROR
        /// </summary>
        [HttpPost, Route("api/vitals/markerror")]
        public async Task<ActionResult<string>> MarkError(
            [FromQuery] string dfn,
            [FromQuery] string ien,
            [FromQuery] string reason = "")
        {
            var vq = new VistaQuery("GMV MARK ERROR");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            vq.addParameter(VistaQuery.LITERAL, reason ?? "");
            return await this.Session.sQuery(vq);
        }

        #region Simple convenience endpoints

        /// <summary>All vitals data. RPC: GMV V/M ALLDATA</summary>
        [HttpGet, Route("api/vitals/list")]
        public async Task<ActionResult<List<string>>> AllData([FromQuery] string dfn)
        {
            var vq = new VistaQuery("GMV V/M ALLDATA");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Add vital measurement. RPC: GMV ADD VM</summary>
        [HttpPost, Route("api/vitals/add")]
        public async Task<ActionResult<string>> AddVital(
            [FromQuery] string dfn, [FromQuery] string vitalType = "",
            [FromQuery] string reading = "", [FromQuery] string dateTime = "")
        {
            var vq = new VistaQuery("GMV ADD VM");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, vitalType);
            vq.addParameter(VistaQuery.LITERAL, reading);
            vq.addParameter(VistaQuery.LITERAL, dateTime);
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
