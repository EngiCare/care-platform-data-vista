// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Radiology order dialogs — imaging types, procedures, sources, isolation.
    /// Migrated from cprs/Orders/rODRad.pas — ORWDRA32 RPCs.
    /// </summary>
    public class OrderRadController : BaseController
    {
        public OrderRadController() : base() { }

        [HttpGet, Route("api/orderrad/defaults")]
        public async Task<ActionResult<List<string>>> ODForRad(
            [FromQuery] string dfn, [FromQuery] string eventDiv, [FromQuery] int imagingType)
        {
            var vq = new VistaQuery("ORWDRA32 DEF");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, eventDiv);
            vq.addParameter(VistaQuery.LITERAL, imagingType.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderrad/islonglist")]
        public async Task<ActionResult<string>> IsRadProcsLongList([FromQuery] int imagingType)
        {
            var vq = new VistaQuery("ORWDRA32 RADLONG");
            vq.addParameter(VistaQuery.LITERAL, imagingType.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderrad/procedures")]
        public async Task<ActionResult<List<string>>> SubsetOfRadProcs(
            [FromQuery] int imagingType, [FromQuery] string startFrom, [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWDRA32 RAORDITM");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            vq.addParameter(VistaQuery.LITERAL, imagingType.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderrad/procmessage")]
        public async Task<ActionResult<string>> ImagingMessage([FromQuery] int ien)
        {
            var vq = new VistaQuery("ORWDRA32 PROCMSG");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderrad/isolation")]
        public async Task<ActionResult<string>> PatientOnIsolation([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWDRA32 ISOLATN");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderrad/radiologists")]
        public async Task<ActionResult<List<string>>> SubsetOfRadiologists()
        {
            var vq = new VistaQuery("ORWDRA32 APPROVAL");
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderrad/imagingtypes")]
        public async Task<ActionResult<List<string>>> SubsetOfImagingTypes()
        {
            var vq = new VistaQuery("ORWDRA32 IMTYPSEL");
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderrad/sources")]
        public async Task<ActionResult<List<string>>> SubsetOfRadSources([FromQuery] string srcType)
        {
            var vq = new VistaQuery("ORWDRA32 RADSRC");
            vq.addParameter(VistaQuery.LITERAL, srcType);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderrad/locationtype")]
        public async Task<ActionResult<string>> LocationType([FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDRA32 LOCTYPE");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }
    }
}
