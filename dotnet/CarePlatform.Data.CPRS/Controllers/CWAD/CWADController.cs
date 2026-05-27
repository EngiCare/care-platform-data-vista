// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// CWAD (Crisis / Warnings / Allergies / Directives) and cover-sheet postings.
    /// Problem detail, allergy detail/list, posting detail/list, patient inquiry,
    /// women's health posting, TIU record text.
    /// Migrated from cprs/rCover.pas — ORQQPL DETAIL, ORQQAL DETAIL,
    /// ORQQAL LIST REPORT, WVRPCOR POSTREP, TIU GET RECORD TEXT,
    /// ORQQAL LIST, ORQQPP LIST, ORWPT PTINQ RPCs.
    /// </summary>
    public class CWADController : BaseController
    {
        public CWADController() : base() { }

        #region Problem Detail

        /// <summary>
        /// Get detail for a specific problem (for CWAD display).
        /// RPC: ORQQPL DETAIL
        /// </summary>
        [HttpGet, Route("api/cwad/problemdetail")]
        public async Task<ActionResult<List<string>>> DetailProblem(
            [FromQuery] string dfn,
            [FromQuery] string encounterDateTime,
            [FromQuery] int ien)
        {
            var vq = new VistaQuery("ORQQPL DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn + "^" + encounterDateTime);
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Allergy Detail & List

        /// <summary>
        /// Get detail for a specific allergy.
        /// RPC: ORQQAL DETAIL
        /// </summary>
        [HttpGet, Route("api/cwad/allergydetail")]
        public async Task<ActionResult<List<string>>> DetailAllergy(
            [FromQuery] string dfn,
            [FromQuery] int ien)
        {
            var vq = new VistaQuery("ORQQAL DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a full allergy list report for a patient.
        /// RPC: ORQQAL LIST REPORT
        /// </summary>
        [HttpGet, Route("api/cwad/allergylistreport")]
        public async Task<ActionResult<List<string>>> AllergyListReport([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQAL LIST REPORT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the allergy list for a patient.
        /// RPC: ORQQAL LIST
        /// </summary>
        [HttpGet, Route("api/cwad/allergies")]
        public async Task<ActionResult<List<string>>> ListAllergies([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQAL LIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Postings

        /// <summary>
        /// Get detail for a posting. If the ID is "A" it returns the allergy
        /// list report; if the ID starts with "WH^" it returns a women's-health
        /// posting; otherwise it returns TIU record text.
        /// RPC: ORQQAL LIST REPORT | WVRPCOR POSTREP | TIU GET RECORD TEXT
        /// </summary>
        [HttpGet, Route("api/cwad/postingdetail")]
        public async Task<ActionResult<List<string>>> DetailPosting(
            [FromQuery] string dfn,
            [FromQuery] string id)
        {
            if (id == "A")
            {
                var vq = new VistaQuery("ORQQAL LIST REPORT");
                vq.addParameter(VistaQuery.LITERAL, dfn);
                return await this.Session.tQuery(vq);
            }
            else if (id.StartsWith("WH^"))
            {
                var vq = new VistaQuery("WVRPCOR POSTREP");
                vq.addParameter(VistaQuery.LITERAL, dfn);
                vq.addParameter(VistaQuery.LITERAL, id.Substring(3, 1));
                return await this.Session.tQuery(vq);
            }
            else
            {
                var vq = new VistaQuery("TIU GET RECORD TEXT");
                vq.addParameter(VistaQuery.LITERAL, id);
                return await this.Session.tQuery(vq);
            }
        }

        /// <summary>
        /// Get the postings list (CWAD flags) for a patient.
        /// RPC: ORQQPP LIST
        /// </summary>
        [HttpGet, Route("api/cwad/postings")]
        public async Task<ActionResult<List<string>>> ListPostings([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQPP LIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Patient Inquiry / Demographics

        /// <summary>
        /// Load patient demographics / inquiry data.
        /// RPC: ORWPT PTINQ
        /// </summary>
        [HttpGet, Route("api/cwad/demographics")]
        public async Task<ActionResult<List<string>>> LoadDemographics([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT PTINQ");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion
    }
}
