using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Other Than Honorable (OTH) discharge status — retrieves OTH eligibility
    /// clock information for a patient so the UI can display the OTH button,
    /// title, detail, and popup messages.
    /// Migrated from cprs/rOTH.pas — OROTHCL GET RPC.
    /// </summary>
    public class OTHController : BaseController
    {
        public OTHController() : base() { }

        /// <summary>
        /// Get the OTH discharge-clock status for a patient.
        /// The first result row contains a count (or negative error code ^message).
        /// Subsequent rows contain display/detail lines.
        /// RPC: OROTHCL GET
        /// </summary>
        [HttpGet, Route("api/oth/status")]
        public async Task<ActionResult<List<string>>> GetOTHStatus(
            [FromQuery] string dfn,
            [FromQuery] string dateOfService)
        {
            var vq = new VistaQuery("OROTHCL GET");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, dateOfService);
            return await this.Session.tQuery(vq);
        }
    }
}
