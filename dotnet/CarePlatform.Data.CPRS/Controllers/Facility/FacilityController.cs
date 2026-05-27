// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Facility and remote data access — CIRN, HL7 link, VistaWeb, site params.
    /// Migrated from cprs/rCore.pas — ORWCIRN *, XWB DIRECT RPC, ORWRP GET DEFAULT PRINTER RPCs.
    /// </summary>
    public class FacilityController : BaseController
    {
        public FacilityController() : base() { }

        /// <summary>
        /// Get the list of remote facilities with data for a patient (+/- indicator).
        /// RPC: ORWCIRN FACLIST
        /// Returns: list of SiteId^SiteName^HasData per line.
        /// First line '-1^...' means error/no remote data.
        /// </summary>
        [HttpGet, Route("api/facility/remotefacilities")]
        public async Task<ActionResult<List<string>>> RemoteFacilities([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWCIRN FACLIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if the HL7 TCP link is active.
        /// RPC: ORWCIRN CHECKLINK
        /// Returns: '1' if link is active
        /// </summary>
        [HttpGet, Route("api/facility/checkhl7link")]
        public async Task<ActionResult<string>> CheckHl7Link()
        {
            var vq = new VistaQuery("ORWCIRN CHECKLINK");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the VistaWeb URL for a given value/context.
        /// RPC: ORWCIRN WEBADDR
        /// </summary>
        [HttpGet, Route("api/facility/vistawebaddress")]
        public async Task<ActionResult<string>> VistaWebAddress([FromQuery] string value)
        {
            var vq = new VistaQuery("ORWCIRN WEBADDR");
            vq.addParameter(VistaQuery.LITERAL, value);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the JLV (Joint Longitudinal Viewer) label name.
        /// RPC: ORWCIRN JLV LABEL
        /// </summary>
        [HttpGet, Route("api/facility/jlvlabel")]
        public async Task<ActionResult<string>> JlvLabel()
        {
            var vq = new VistaQuery("ORWCIRN JLV LABEL");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check remote patient access at a remote site (via XWB DIRECT RPC → ORWCIRN RESTRICT).
        /// RPC: XWB DIRECT RPC
        /// Returns: access status and message from remote site
        /// </summary>
        [HttpGet, Route("api/facility/checkremotepatient")]
        public async Task<ActionResult<List<string>>> CheckRemotePatient(
            [FromQuery] string patient,
            [FromQuery] string site)
        {
            var vq = new VistaQuery("XWB DIRECT RPC");
            vq.addParameter(VistaQuery.LITERAL, site);
            vq.addParameter(VistaQuery.LITERAL, "ORWCIRN RESTRICT");
            vq.addParameter(VistaQuery.LITERAL, "0");
            vq.addParameter(VistaQuery.LITERAL, patient);
            return await this.Session.tQuery(vq);
        }
    }
}
