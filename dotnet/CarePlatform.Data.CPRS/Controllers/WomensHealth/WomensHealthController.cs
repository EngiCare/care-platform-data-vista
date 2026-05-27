// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Women's Health — pregnancy, lactation, and conception status tracking.
    /// Migrated from cprs/Womens Health/oWVController.pas, oWVPatient.pas.
    /// RPCs: WV PREGNANCY STATUS, WV LACTATION STATUS, WV ABLE TO CONCEIVE, etc.
    /// </summary>
    public class WomensHealthController : BaseController
    {
        public WomensHealthController() : base() { }

        /// <summary>
        /// Check if the patient is a valid Women's Health patient (female, of age).
        /// RPC: WV IS PATIENT FEMALE
        /// </summary>
        [HttpGet, Route("api/womenshealth/isvalid")]
        public async Task<ActionResult<string>> IsValidPatient([FromQuery] string dfn)
        {
            var vq = new VistaQuery("WV IS PATIENT FEMALE");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get current pregnancy status.
        /// RPC: WV PREGNANCY STATUS
        /// </summary>
        [HttpGet, Route("api/womenshealth/pregnancystatus")]
        public async Task<ActionResult<string>> GetPregnancyStatus([FromQuery] string dfn)
        {
            var vq = new VistaQuery("WV PREGNANCY STATUS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get current lactation status.
        /// RPC: WV LACTATION STATUS
        /// </summary>
        [HttpGet, Route("api/womenshealth/lactationstatus")]
        public async Task<ActionResult<string>> GetLactationStatus([FromQuery] string dfn)
        {
            var vq = new VistaQuery("WV LACTATION STATUS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get ability to conceive status.
        /// RPC: WV ABLE TO CONCEIVE
        /// </summary>
        [HttpGet, Route("api/womenshealth/abletoconceive")]
        public async Task<ActionResult<string>> GetAbleToConceive([FromQuery] string dfn)
        {
            var vq = new VistaQuery("WV ABLE TO CONCEIVE");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get combined Women's Health status for a patient.
        /// RPC: WV PATIENT STATUS
        /// Returns: PregnancyStatus^LactationStatus^AbleToConceive^LMP^Hysterectomy^Menopause
        /// </summary>
        [HttpGet, Route("api/womenshealth/patientstatus")]
        public async Task<ActionResult<string>> GetPatientStatus([FromQuery] string dfn)
        {
            var vq = new VistaQuery("WV PATIENT STATUS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save updated pregnancy and lactation data.
        /// RPC: WV SAVE PREGLAC DATA
        /// </summary>
        [HttpPost, Route("api/womenshealth/savepreglac")]
        public async Task<ActionResult<string>> SavePregLacData(
            [FromQuery] string dfn,
            [FromBody] List<string> data)
        {
            var vq = new VistaQuery("WV SAVE PREGLAC DATA");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < data.Count; i++)
                dhl.Add(i.ToString(), data[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Mark a Women's Health record as entered in error.
        /// RPC: WV ENTERED IN ERROR
        /// </summary>
        [HttpPost, Route("api/womenshealth/enteredinerror")]
        public async Task<ActionResult<string>> MarkEnteredInError(
            [FromQuery] string itemId,
            [FromQuery] string reason)
        {
            var vq = new VistaQuery("WV ENTERED IN ERROR");
            vq.addParameter(VistaQuery.LITERAL, itemId);
            vq.addParameter(VistaQuery.LITERAL, reason ?? "");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if WH data prompts should be shown for this encounter.
        /// RPC: WV ASK FOR DATA
        /// </summary>
        [HttpGet, Route("api/womenshealth/askfordata")]
        public async Task<ActionResult<string>> AskForData([FromQuery] string dfn)
        {
            var vq = new VistaQuery("WV ASK FOR DATA");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get WH-related external website links.
        /// RPC: WV GET WEBSITES
        /// </summary>
        [HttpGet, Route("api/womenshealth/websites")]
        public async Task<ActionResult<List<string>>> GetWebsites()
        {
            var vq = new VistaQuery("WV GET WEBSITES");
            return await this.Session.tQuery(vq);
        }
    }
}
