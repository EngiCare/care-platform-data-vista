// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Allergy management — search, add, remove, NKA.
    /// RPCs: ORWDAL32 ALLERGY MATCH, ORWDAL32 SAVE ALLERGY
    /// </summary>
    public class AllergyController : BaseController
    {
        public AllergyController() : base() { }

        /// <summary>
        /// Search for allergens.
        /// RPC: ORWDAL32 ALLERGY MATCH
        /// </summary>
        [HttpGet, Route("api/allergen/search")]
        public async Task<ActionResult<List<string>>> Search([FromQuery] string search)
        {
            var vq = new VistaQuery("ORWDAL32 ALLERGY MATCH");
            vq.addParameter(VistaQuery.LITERAL, search ?? "");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Add a new allergy.
        /// RPC: ORWDAL32 SAVE ALLERGY
        /// </summary>
        [HttpPost, Route("api/allergy/add")]
        public async Task<ActionResult<string>> Add(
            [FromQuery] string dfn,
            [FromQuery] string allergen = "",
            [FromQuery] string type = "",
            [FromQuery] string severity = "",
            [FromQuery] string symptoms = "")
        {
            var vq = new VistaQuery("ORWDAL32 SAVE ALLERGY");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            vq.addParameter(VistaQuery.LITERAL, allergen);
            vq.addParameter(VistaQuery.LITERAL, type);
            vq.addParameter(VistaQuery.LITERAL, severity);
            vq.addParameter(VistaQuery.LITERAL, symptoms);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Remove allergy (mark entered-in-error).
        /// RPC: ORWDAL32 SAVE ALLERGY
        /// </summary>
        [HttpPost, Route("api/allergy/remove")]
        public async Task<ActionResult<string>> Remove(
            [FromQuery] string dfn,
            [FromQuery] string ien)
        {
            var vq = new VistaQuery("ORWDAL32 SAVE ALLERGY");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            vq.addParameter(VistaQuery.LITERAL, "EIE"); // entered-in-error
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Mark No Known Allergies.
        /// RPC: ORWDAL32 SAVE ALLERGY
        /// </summary>
        [HttpPost, Route("api/allergy/nka")]
        public async Task<ActionResult<string>> MarkNka([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWDAL32 SAVE ALLERGY");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            vq.addParameter(VistaQuery.LITERAL, "NKA");
            return await this.Session.sQuery(vq);
        }
    }
}
