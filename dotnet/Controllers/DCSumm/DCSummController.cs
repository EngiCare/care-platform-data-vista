using CarePlatform.Data.VistA;
using CarePlatform.Models.Common;
using CarePlatform.Models.DCSumm;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Discharge Summaries — titles, context, create/edit/sign, urgencies, document params.
    /// Migrated from cprs/rDCSumm.pas — TIU DC-summary RPCs, ORWTIU context RPCs.
    /// </summary>
    public class DCSummController : BaseController
    {
        public DCSummController() : base() { }

        #region Titles / Preferences

        [HttpGet, Route("api/dcsumm/titles")]
        public async Task<ActionResult<List<string>>> SubsetOfDCSummTitles(
            [FromQuery] string startFrom, [FromQuery] int direction,
            [FromQuery] bool idNoteTitlesOnly = false)
        {
            var vq = new VistaQuery("TIU LONG LIST OF TITLES");
            vq.addParameter(VistaQuery.LITERAL, "244"); // CLS_DC_SUMM
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            if (idNoteTitlesOnly)
                vq.addParameter(VistaQuery.LITERAL, "1");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/dcsumm/personaltitles")]
        public async Task<ActionResult<List<string>>> PersonalTitles([FromQuery] long userDuz)
        {
            var vq = new VistaQuery("TIU PERSONAL TITLE LIST");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            vq.addParameter(VistaQuery.LITERAL, "244"); // CLS_DC_SUMM
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/dcsumm/preferences")]
        public async Task<ActionResult<string>> PersonalPreferences([FromQuery] long userDuz)
        {
            var vq = new VistaQuery("TIU GET PERSONAL PREFERENCES");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/dcsumm/urgencies")]
        public async Task<ActionResult<List<string>>> DSUrgencies()
        {
            var vq = new VistaQuery("TIU GET DS URGENCIES");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Listing / Display

        /// <summary>
        /// List discharge summaries for tree view.
        /// RPC: TIU DOCUMENTS BY CONTEXT (class=244, CLS_DC_SUMM)
        /// Matches cprs/rDCSumm.pas ListSummsForTree.
        /// </summary>
        [HttpGet, Route("api/dcsumm/list")]
        public async Task<ActionResult<List<TiuDocument>>> ListSummsForTree(
            [FromQuery] int context, [FromQuery] string dfn,
            [FromQuery] string early = "", [FromQuery] string late = "",
            [FromQuery] long person = 0, [FromQuery] int occLim = 0,
            [FromQuery] bool sortAscending = false, [FromQuery] bool showAddenda = false,
            [FromQuery] string view = "")
        {
            var sortSeq = sortAscending ? "A" : "D";
            var vq = new VistaQuery("TIU DOCUMENTS BY CONTEXT");
            vq.addParameter(VistaQuery.LITERAL, "244"); // CLS_DC_SUMM
            vq.addParameter(VistaQuery.LITERAL, context.ToString());
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, early);
            vq.addParameter(VistaQuery.LITERAL, late);
            vq.addParameter(VistaQuery.LITERAL, person.ToString());
            vq.addParameter(VistaQuery.LITERAL, occLim.ToString());
            vq.addParameter(VistaQuery.LITERAL, sortSeq);
            vq.addParameter(VistaQuery.LITERAL, showAddenda ? "1" : "0");
            var results = await this.Session.tQuery(vq);
            return TiuDocument.ParseList(results);
        }

        [HttpGet, Route("api/dcsumm/authorization")]
        public async Task<ActionResult<string>> ActOnDCDocument(
            [FromQuery] int ien, [FromQuery] string actionName)
        {
            var vq = new VistaQuery("TIU AUTHORIZATION");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, actionName);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Edit / Text

        [HttpGet, Route("api/dcsumm/loadforedit")]
        public async Task<ActionResult<List<string>>> GetDCSummForEdit([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU LOAD RECORD FOR EDIT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, ".01;.06;.07;.09;1202;1205;1208;1209;1301;1302;1307;1701");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/dcsumm/loadtextonly")]
        public async Task<ActionResult<List<string>>> GetDCSummTextOnly([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU LOAD RECORD TEXT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Create / Save / Sign

        [HttpPost, Route("api/dcsumm/create")]
        public async Task<ActionResult<List<string>>> CreateDCSumm(
            [FromQuery] string dfn, [FromQuery] int title,
            [FromQuery] string visitStr, [FromBody] List<string> fieldData)
        {
            var vq = new VistaQuery("TIU CREATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, title.ToString());
            vq.addParameter(VistaQuery.LITERAL, "");
            vq.addParameter(VistaQuery.LITERAL, "");
            vq.addParameter(VistaQuery.LITERAL, "");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < fieldData.Count; i++)
                dhl.Add(i.ToString(), fieldData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, visitStr);
            vq.addParameter(VistaQuery.LITERAL, "1");
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/dcsumm/createaddendum")]
        public async Task<ActionResult<List<string>>> CreateAddendum(
            [FromQuery] int addendumTo, [FromBody] List<string> fieldData)
        {
            var vq = new VistaQuery("TIU CREATE ADDENDUM RECORD");
            vq.addParameter(VistaQuery.LITERAL, addendumTo.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < fieldData.Count; i++)
                dhl.Add(i.ToString(), fieldData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, "1");
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/dcsumm/update")]
        public async Task<ActionResult<List<string>>> UpdateDCSumm(
            [FromQuery] int noteIen, [FromBody] List<string> fieldData)
        {
            var vq = new VistaQuery("TIU UPDATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < fieldData.Count; i++)
                dhl.Add(i.ToString(), fieldData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/dcsumm/sign")]
        public async Task<ActionResult<string>> SignDCDocument(
            [FromQuery] int ien, [FromQuery] string esCode)
        {
            var vq = new VistaQuery("TIU SIGN RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, esCode);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Document Parameters

        [HttpGet, Route("api/dcsumm/docparams")]
        public async Task<ActionResult<string>> GetDocumentParameters(
            [FromQuery] int noteIen, [FromQuery] int typeIen = 0)
        {
            var vq = new VistaQuery("TIU GET DOCUMENT PARAMETERS");
            if (noteIen > 0)
                vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            else
            {
                vq.addParameter(VistaQuery.LITERAL, "0");
                vq.addParameter(VistaQuery.LITERAL, typeIen.ToString());
            }
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Attending / Discharge

        [HttpGet, Route("api/dcsumm/attending")]
        public async Task<ActionResult<string>> GetAttending([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQPT ATTENDING/PRIMARY");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/dcsumm/dischargedate")]
        public async Task<ActionResult<string>> GetDischargeDate(
            [FromQuery] string dfn, [FromQuery] string admitDateTime)
        {
            var vq = new VistaQuery("ORWPT DISCHARGE");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, admitDateTime);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Context

        [HttpGet, Route("api/dcsumm/context")]
        public async Task<ActionResult<string>> GetDCSummContext([FromQuery] long userDuz)
        {
            var vq = new VistaQuery("ORWTIU GET DCSUMM CONTEXT");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/dcsumm/context")]
        public async Task<ActionResult<string>> SaveDCSummContext([FromQuery] string context)
        {
            var vq = new VistaQuery("ORWTIU SAVE DCSUMM CONTEXT");
            vq.addParameter(VistaQuery.LITERAL, context);
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/dcsumm/changeattending")]
        public async Task<ActionResult<List<string>>> ChangeAttending(
            [FromQuery] int ien, [FromBody] List<string> fieldData)
        {
            var vq = new VistaQuery("TIU UPDATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < fieldData.Count; i++)
                dhl.Add(i.ToString(), fieldData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Simple convenience endpoints (ws/ prefix)

        /// <summary>Simple list endpoint. RPC: ORWCS LIST OF REPORTS</summary>
        [HttpGet, Route("api/dcsumm/ws/list")]
        public async Task<ActionResult<List<string>>> WsList([FromQuery] string dfn, [FromQuery] string view = "")
        {
            var vq = new VistaQuery("ORWCS LIST OF REPORTS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, view);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Get record text. RPC: TIU GET RECORD TEXT</summary>
        [HttpGet, Route("api/dcsumm/text")]
        public async Task<ActionResult<List<string>>> GetRecordText([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU GET RECORD TEXT");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Simple create. RPC: TIU CREATE RECORD</summary>
        [HttpPost, Route("api/dcsumm/ws/create")]
        public async Task<ActionResult<string>> WsCreate(
            [FromQuery] string dfn, [FromQuery] string titleIen,
            [FromQuery] string text = "", [FromQuery] string encounterDate = "")
        {
            var vq = new VistaQuery("TIU CREATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, titleIen);
            vq.addParameter(VistaQuery.LITERAL, "");
            vq.addParameter(VistaQuery.LITERAL, "");
            vq.addParameter(VistaQuery.LITERAL, "");
            var dhl = new DictionaryHashList();
            if (!string.IsNullOrEmpty(text))
                dhl.Add("0", text);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, encounterDate);
            vq.addParameter(VistaQuery.LITERAL, "1");
            return await this.Session.sQuery(vq);
        }

        /// <summary>Simple save text. RPC: TIU UPDATE RECORD</summary>
        [HttpPost, Route("api/dcsumm/ws/savetext")]
        public async Task<ActionResult<string>> WsSaveText([FromQuery] string ien, [FromQuery] string text = "")
        {
            var vq = new VistaQuery("TIU UPDATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            var dhl = new DictionaryHashList();
            if (!string.IsNullOrEmpty(text))
                dhl.Add("0", text);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Simple sign. RPC: TIU SIGN RECORD</summary>
        [HttpPost, Route("api/dcsumm/ws/sign")]
        public async Task<ActionResult<string>> WsSign([FromQuery] string ien, [FromQuery] string esCode)
        {
            var vq = new VistaQuery("TIU SIGN RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, esCode);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Simple addendum. RPC: TIU CREATE ADDENDUM RECORD</summary>
        [HttpPost, Route("api/dcsumm/ws/addendum")]
        public async Task<ActionResult<string>> WsAddendum(
            [FromQuery] string parentIen, [FromQuery] string text = "")
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

        /// <summary>Simple delete with optional reason. RPC: TIU DELETE RECORD</summary>
        [HttpPost, Route("api/dcsumm/ws/delete")]
        public async Task<ActionResult<string>> WsDelete([FromQuery] string ien, [FromQuery] string reason = "")
        {
            var vq = new VistaQuery("TIU DELETE RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, reason);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Simple change title. RPC: TIU SET DOCUMENT TITLE</summary>
        [HttpPost, Route("api/dcsumm/ws/changetitle")]
        public async Task<ActionResult<string>> WsChangeTitle([FromQuery] string ien, [FromQuery] string titleIen)
        {
            var vq = new VistaQuery("TIU SET DOCUMENT TITLE");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, titleIen);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Lock / Unlock

        /// <summary>Lock a D/C summary for editing. RPC: TIU LOCK RECORD</summary>
        [HttpPost, Route("api/dcsumm/lock")]
        public async Task<ActionResult<string>> LockRecord([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU LOCK RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Unlock a D/C summary. RPC: TIU UNLOCK RECORD</summary>
        [HttpPost, Route("api/dcsumm/unlock")]
        public async Task<ActionResult<string>> UnlockRecord([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU UNLOCK RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Additional Signers

        /// <summary>Get additional signers. RPC: TIU GET ADDITIONAL SIGNERS</summary>
        [HttpGet, Route("api/dcsumm/additionalsigners")]
        public async Task<ActionResult<List<string>>> GetAdditionalSigners([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU GET ADDITIONAL SIGNERS");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>Add an additional signer. RPC: TIU ADD ADDITIONAL SIGNER</summary>
        [HttpPost, Route("api/dcsumm/addadditionalsigner")]
        public async Task<ActionResult<string>> AddAdditionalSigner(
            [FromQuery] string ien, [FromQuery] string signerDuz)
        {
            var vq = new VistaQuery("TIU ADD ADDITIONAL SIGNER");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, signerDuz);
            return await this.Session.sQuery(vq);
        }

        /// <summary>Remove an additional signer. RPC: TIU REMOVE ADDITIONAL SIGNER</summary>
        [HttpPost, Route("api/dcsumm/removeadditionalsigner")]
        public async Task<ActionResult<string>> RemoveAdditionalSigner(
            [FromQuery] string ien, [FromQuery] string signerDuz)
        {
            var vq = new VistaQuery("TIU REMOVE ADDITIONAL SIGNER");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, signerDuz);
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
