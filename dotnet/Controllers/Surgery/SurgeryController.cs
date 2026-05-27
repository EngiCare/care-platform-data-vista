using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Surgery — case lists, case details, surgery context, non-OR procedures.
    /// Migrated from cprs/rSurgery.pas — ORWSR, TIU surgery RPCs.
    /// </summary>
    public class SurgeryController : BaseController
    {
        public SurgeryController() : base() { }

        [HttpGet, Route("api/surgery/showtab")]
        public async Task<ActionResult<string>> ShowSurgeryTab()
        {
            var vq = new VistaQuery("ORWSR SHOW SURG TAB");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/surgery/identifyclass")]
        public async Task<ActionResult<string>> IdentifySurgeryClass([FromQuery] string className)
        {
            var vq = new VistaQuery("TIU IDENTIFY SURGERY CLASS");
            vq.addParameter(VistaQuery.LITERAL, className);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/surgery/titles")]
        public async Task<ActionResult<List<string>>> SubsetOfSurgeryTitles(
            [FromQuery] string startFrom = "", [FromQuery] int direction = 1, [FromQuery] string className = "")
        {
            var vq = new VistaQuery("TIU LONG LIST SURGERY TITLES");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            vq.addParameter(VistaQuery.LITERAL, className);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Long list of boilerplated (template-linked) surgery titles.
        /// RPC: TIU LONG LIST BOILERPLATED
        /// </summary>
        [HttpGet, Route("api/surgery/templatelist")]
        public async Task<ActionResult<List<string>>> BoilerplatedSurgeryTitles(
            [FromQuery] string startFrom = "", [FromQuery] int direction = 1)
        {
            var vq = new VistaQuery("TIU LONG LIST BOILERPLATED");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/surgery/issurgerytitle")]
        public async Task<ActionResult<string>> IsSurgeryTitle([FromQuery] int titleIen)
        {
            var vq = new VistaQuery("TIU IS THIS A SURGERY?");
            vq.addParameter(VistaQuery.LITERAL, titleIen.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/surgery/caselist")]
        public async Task<ActionResult<List<string>>> GetSurgCaseList(
            [FromQuery] string dfn, [FromQuery] string early, [FromQuery] string late,
            [FromQuery] int context, [FromQuery] int max)
        {
            var vq = new VistaQuery("ORWSR LIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, early);
            vq.addParameter(VistaQuery.LITERAL, late);
            vq.addParameter(VistaQuery.LITERAL, context.ToString());
            vq.addParameter(VistaQuery.LITERAL, max.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/surgery/cases")]
        public async Task<ActionResult<List<string>>> ListSurgeryCases([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWSR CASELIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/surgery/reporttext")]
        public async Task<ActionResult<List<string>>> LoadReportText([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU GET RECORD TEXT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/surgery/reportdetail")]
        public async Task<ActionResult<List<string>>> LoadReportDetail([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU DETAILED DISPLAY");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/surgery/context")]
        public async Task<ActionResult<string>> GetSurgCaseContext([FromQuery] long userDuz)
        {
            var vq = new VistaQuery("ORWSR GET SURG CONTEXT");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/surgery/context")]
        public async Task<ActionResult<string>> SaveSurgCaseContext([FromQuery] string context)
        {
            var vq = new VistaQuery("ORWSR SAVE SURG CONTEXT");
            vq.addParameter(VistaQuery.LITERAL, context);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/surgery/request")]
        public async Task<ActionResult<string>> GetCaseRefForNote([FromQuery] int noteIen)
        {
            var vq = new VistaQuery("TIU GET REQUEST");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/surgery/onecase")]
        public async Task<ActionResult<List<string>>> GetSingleCase([FromQuery] int noteIen)
        {
            var vq = new VistaQuery("ORWSR ONECASE");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get user's personal short list of surgery titles for a surgery class.
        /// RPC: TIU PERSONAL TITLE LIST
        /// Used to populate the title picker and determine the default title for surgery notes.
        /// </summary>
        [HttpGet, Route("api/surgery/personaltitles")]
        public async Task<ActionResult<List<string>>> PersonalTitles(
            [FromQuery] long duz,
            [FromQuery] int surgeryClass)
        {
            var vq = new VistaQuery("TIU PERSONAL TITLE LIST");
            vq.addParameter(VistaQuery.LITERAL, duz.ToString());
            vq.addParameter(VistaQuery.LITERAL, surgeryClass.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/surgery/isnonor")]
        public async Task<ActionResult<string>> IsNonORProcedure([FromQuery] int caseIen)
        {
            var vq = new VistaQuery("ORWSR IS NON-OR PROCEDURE");
            vq.addParameter(VistaQuery.LITERAL, caseIen.ToString());
            return await this.Session.sQuery(vq);
        }

        #region Simple convenience endpoints

        /// <summary>List surgery reports. RPC: ORWSR RPTLIST</summary>
        [HttpGet, Route("api/surgery/list")]
        public async Task<ActionResult<List<string>>> ListReports([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWSR RPTLIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Report text. RPC: ORWSR RPTTEXT</summary>
        [HttpGet, Route("api/surgery/report")]
        public async Task<ActionResult<List<string>>> Report([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORWSR RPTTEXT");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Create surgery record. RPC: TIU CREATE RECORD</summary>
        [HttpPost, Route("api/surgery/create")]
        public async Task<ActionResult<string>> WsCreate(
            [FromQuery] string dfn, [FromQuery] string titleIen = "",
            [FromQuery] string text = "", [FromQuery] string encounterDate = "")
        {
            var vq = new VistaQuery("TIU CREATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, titleIen);
            vq.addParameter(VistaQuery.LITERAL, text);
            vq.addParameter(VistaQuery.LITERAL, encounterDate);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Save surgery text. RPC: TIU UPDATE RECORD</summary>
        [HttpPost, Route("api/surgery/save")]
        public async Task<ActionResult<string>> WsSave([FromQuery] string ien, [FromQuery] string text = "")
        {
            var vq = new VistaQuery("TIU UPDATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            var dhl = new DictionaryHashList();
            if (!string.IsNullOrEmpty(text))
                dhl.Add("0", text);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Sign surgery report. RPC: TIU SIGN RECORD</summary>
        [HttpPost, Route("api/surgery/sign")]
        public async Task<ActionResult<string>> WsSign([FromQuery] string ien, [FromQuery] string esCode)
        {
            var vq = new VistaQuery("TIU SIGN RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, esCode);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Delete surgery report. RPC: TIU DELETE RECORD</summary>
        [HttpPost, Route("api/surgery/delete")]
        public async Task<ActionResult<string>> WsDelete([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU DELETE RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.sQuery(vq);
        }

        /// <summary>Add addendum. RPC: TIU CREATE ADDENDUM RECORD</summary>
        [HttpPost, Route("api/surgery/addendum")]
        public async Task<ActionResult<string>> WsAddendum([FromQuery] string parentIen, [FromQuery] string text = "")
        {
            var vq = new VistaQuery("TIU CREATE ADDENDUM RECORD");
            vq.addParameter(VistaQuery.LITERAL, parentIen);
            var dhl = new DictionaryHashList();
            if (!string.IsNullOrEmpty(text))
                dhl.Add("0", text);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, "1");
            return await this.Session.sQuery(vq);
        }

        /// <summary>Change title. RPC: TIU SET DOCUMENT TITLE</summary>
        [HttpPost, Route("api/surgery/changetitle")]
        public async Task<ActionResult<string>> WsChangeTitle([FromQuery] string ien, [FromQuery] string titleIen)
        {
            var vq = new VistaQuery("TIU SET DOCUMENT TITLE");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, titleIen);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Lock a surgery document. RPC: TIU LOCK RECORD</summary>
        [HttpPost, Route("api/surgery/lock")]
        public async Task<ActionResult<string>> WsLock([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU LOCK RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Unlock a surgery document. RPC: TIU UNLOCK RECORD</summary>
        [HttpPost, Route("api/surgery/unlock")]
        public async Task<ActionResult<string>> WsUnlock([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU UNLOCK RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Check if document requires cosignature. RPC: TIU REQUIRES COSIGNATURE</summary>
        [HttpGet, Route("api/surgery/requirescosignature")]
        public async Task<ActionResult<string>> WsRequiresCosignature(
            [FromQuery] string titleIen = "", [FromQuery] string authorDuz = "")
        {
            var vq = new VistaQuery("TIU REQUIRES COSIGNATURE");
            vq.addParameter(VistaQuery.LITERAL, titleIen);
            vq.addParameter(VistaQuery.LITERAL, authorDuz);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Can this user change the cosigner? RPC: TIU CAN CHANGE COSIGNER</summary>
        [HttpGet, Route("api/surgery/canchangecosigner")]
        public async Task<ActionResult<string>> WsCanChangeCosigner([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU CAN CHANGE COSIGNER");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Get additional signers for a document. RPC: TIU GET ADDITIONAL SIGNERS</summary>
        [HttpGet, Route("api/surgery/additionalsigners")]
        public async Task<ActionResult<List<string>>> WsGetAdditionalSigners([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU GET ADDITIONAL SIGNERS");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Update additional signers. RPC: TIU UPDATE ADDITIONAL SIGNERS</summary>
        [HttpPost, Route("api/surgery/additionalsigners")]
        public async Task<ActionResult<string>> WsUpdateAdditionalSigners(
            [FromQuery] string ien, [FromQuery] string signerDuz)
        {
            var vq = new VistaQuery("TIU UPDATE ADDITIONAL SIGNERS");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, signerDuz);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Validate document action. RPC: TIU AUTHORIZATION</summary>
        [HttpGet, Route("api/surgery/authorization")]
        public async Task<ActionResult<string>> WsAuthorization(
            [FromQuery] string ien, [FromQuery] string action)
        {
            var vq = new VistaQuery("TIU AUTHORIZATION");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, action);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Check if delete justification is required. RPC: TIU JUSTIFY DELETE?</summary>
        [HttpGet, Route("api/surgery/justifydelete")]
        public async Task<ActionResult<string>> WsJustifyDelete([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU JUSTIFY DELETE?");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Delete surgery report with reason. RPC: TIU DELETE RECORD</summary>
        [HttpPost, Route("api/surgery/deletewreason")]
        public async Task<ActionResult<string>> WsDeleteWithReason(
            [FromQuery] string ien, [FromQuery] string reason = "")
        {
            var vq = new VistaQuery("TIU DELETE RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, reason);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Check if last save was clean. RPC: TIU WAS THIS SAVED?</summary>
        [HttpGet, Route("api/surgery/lastsaveclean")]
        public async Task<ActionResult<string>> WsLastSaveClean([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU WAS THIS SAVED?");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Check if this is a cosigned document. RPC: TIU IS THIS A COSIGNED DOC?</summary>
        [HttpGet, Route("api/surgery/iscosigneddoc")]
        public async Task<ActionResult<string>> WsIsCosignedDoc([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU COSIGN DOCUMENT");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Get boilerplate text for a title. RPC: TIU TEMPLATE GET BOILERPLATE</summary>
        [HttpGet, Route("api/surgery/boilerplate")]
        public async Task<ActionResult<List<string>>> WsBoilerplate([FromQuery] string titleIen)
        {
            var vq = new VistaQuery("TIU TEMPLATE GET BOILERPLATE");
            vq.addParameter(VistaQuery.LITERAL, titleIen);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Get formatted print text. RPC: TIU GET PRINT NAME</summary>
        [HttpGet, Route("api/surgery/printtext")]
        public async Task<ActionResult<List<string>>> WsPrintText([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU DETAILED DISPLAY");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Get PCE encounter data for a document. RPC: ORWPCE PCE4NOTE</summary>
        [HttpGet, Route("api/surgery/pcedata")]
        public async Task<ActionResult<List<string>>> WsPceData([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORWPCE PCE4NOTE");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        #endregion
    }
}
