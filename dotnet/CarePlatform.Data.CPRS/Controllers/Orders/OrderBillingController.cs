// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Billing Awareness — treatment factors, diagnosis lists, copay, SC/EI/AO/IR/MST flags.
    /// Migrated from cprs/BA/UBACore.pas + UBAGlobals.pas — ORWDBA* RPCs.
    /// </summary>
    public class OrderBillingController : BaseController
    {
        public OrderBillingController() : base() { }

        #region Master Switch / Status

        [HttpGet, Route("api/orderbilling/masterstatus")]
        public async Task<ActionResult<string>> BAMasterStatus()
        {
            var vq = new VistaQuery("ORWDBA1 BASTATUS");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderbilling/getbauser")]
        public async Task<ActionResult<string>> GetBAUser([FromQuery] long provider)
        {
            var vq = new VistaQuery("ORWDBA4 GETBAUSR");
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderbilling/isinsured")]
        public async Task<ActionResult<string>> IsPatientInsured([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWDBA7 ISWITCH");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Order Package Type / Treatment Factors

        [HttpPost, Route("api/orderbilling/orderpkgtype")]
        public async Task<ActionResult<List<string>>> OrderPackageType([FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWDBA1 ORPKGTYP");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderbilling/sclist")]
        public async Task<ActionResult<List<string>>> GetSCList([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWDBA1 SCLST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/orderbilling/savecidcdata")]
        public async Task<ActionResult<string>> SaveCIDCData([FromBody] List<string> cidcList)
        {
            var vq = new VistaQuery("ORWDBA1 RCVORCI");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < cidcList.Count; i++)
                dhl.Add(i.ToString(), cidcList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/orderbilling/gettfci")]
        public async Task<ActionResult<List<string>>> GetTreatmentFactorInfo([FromBody] List<string> orderIds)
        {
            var vq = new VistaQuery("ORWDBA4 GETTFCI");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderIds.Count; i++)
                dhl.Add(i.ToString(), orderIds[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Personal Diagnosis List

        [HttpPost, Route("api/orderbilling/addtopdl")]
        public async Task<ActionResult<string>> AddToPersonalDxList(
            [FromQuery] long userDuz, [FromBody] List<string> dxCodes)
        {
            var vq = new VistaQuery("ORWDBA2 ADDPDL");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < dxCodes.Count; i++)
                dhl.Add(i.ToString(), dxCodes[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderbilling/getpdl")]
        public async Task<ActionResult<List<string>>> GetPersonalDxList([FromQuery] long userDuz)
        {
            var vq = new VistaQuery("ORWDBA2 GETPDL");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/orderbilling/deletepdl")]
        public async Task<ActionResult<string>> DeleteFromPersonalDxList(
            [FromQuery] long userDuz, [FromBody] List<string> dxCodes)
        {
            var vq = new VistaQuery("ORWDBA2 DELPDL");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < dxCodes.Count; i++)
                dhl.Add(i.ToString(), dxCodes[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderbilling/providerpatientdx")]
        public async Task<ActionResult<List<string>>> GetProviderPatientDaysDx(
            [FromQuery] string providerIen, [FromQuery] string patientIen)
        {
            var vq = new VistaQuery("ORWDBA2 GETDUDC");
            vq.addParameter(VistaQuery.LITERAL, providerIen);
            vq.addParameter(VistaQuery.LITERAL, patientIen);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Hints / ICD / Misc

        [HttpGet, Route("api/orderbilling/hints")]
        public async Task<ActionResult<List<string>>> GetTFHintData()
        {
            var vq = new VistaQuery("ORWDBA3 HINTS");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderbilling/geticdien")]
        public async Task<ActionResult<string>> GetICDIEN([FromQuery] string dxCode)
        {
            var vq = new VistaQuery("ORWDBA7 GETIEN9");
            vq.addParameter(VistaQuery.LITERAL, dxCode);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderbilling/activecode")]
        public async Task<ActionResult<string>> IsActiveCode(
            [FromQuery] string code, [FromQuery] string lexApp,
            [FromQuery] string date = "")
        {
            var vq = new VistaQuery("ORWPCE ACTIVE CODE");
            vq.addParameter(VistaQuery.LITERAL, code);
            vq.addParameter(VistaQuery.LITERAL, lexApp);
            vq.addParameter(VistaQuery.LITERAL, date);
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
