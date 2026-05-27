// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using CarePlatform.Models.Consults;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Consults &amp; Procedures — listing, detail, actions, services, titles, medicine results, SF513.
    /// Migrated from cprs/Consults/rConsults.pas — ORQQCN*, ORWDCN32, TIU consult RPCs.
    /// </summary>
    public class ConsultController : BaseController
    {
        public ConsultController() : base() { }

        #region Listing / Detail

        [HttpGet, Route("api/consult/list")]
        public async Task<ActionResult<List<Models.Consults.Consult>>> GetConsultsList(
            [FromQuery] string dfn, [FromQuery] string early, [FromQuery] string late,
            [FromQuery] string service, [FromQuery] string status)
        {
            var vq = new VistaQuery("ORQQCN LIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, early);
            vq.addParameter(VistaQuery.LITERAL, late);
            vq.addParameter(VistaQuery.LITERAL, service);
            vq.addParameter(VistaQuery.LITERAL, status);
            var results = await this.Session.tQuery(vq);
            return Models.Consults.Consult.ParseList(results);
        }

        [HttpGet, Route("api/consult/detail")]
        public async Task<ActionResult<List<string>>> LoadConsultDetail([FromQuery] int ien)
        {
            var vq = new VistaQuery("ORQQCN DETAIL");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/get")]
        public async Task<ActionResult<List<string>>> GetConsultRec(
            [FromQuery] int ien, [FromQuery] bool showAddenda = false)
        {
            var vq = new VistaQuery("ORQQCN GET CONSULT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, showAddenda ? "1" : "0");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Return TIU documents linked to a consult, filtering out medicine results (MCAR).
        /// Mirrors Delphi rConsults.pas GetConsultRec — calls ORQQCN GET CONSULT, strips
        /// the zero-node and MCAR lines, returns only TIU document lines.
        /// </summary>
        [HttpGet, Route("api/consult/linkednotes")]
        public async Task<ActionResult<List<string>>> GetLinkedNotes([FromQuery] string consultIen)
        {
            var vq = new VistaQuery("ORQQCN GET CONSULT");
            vq.addParameter(VistaQuery.LITERAL, consultIen);
            vq.addParameter(VistaQuery.LITERAL, "1"); // SHOW_ADDENDA = true
            var lines = await this.Session.tQuery(vq);
            if (lines == null || lines.Count == 0)
                return new List<string>();
            // First line is the consult zero-node — check for error
            var firstPiece = lines[0].Split('^')[0];
            if (firstPiece == "-1")
                return new List<string>();
            // Skip line 0, filter out MCAR (medicine results) — same logic as Delphi
            var tiuDocs = new List<string>();
            for (int i = 1; i < lines.Count; i++)
            {
                var piece1 = lines[i].Split('^')[0];
                var parts = piece1.Split(';');
                if (parts.Length >= 2 && parts[1].StartsWith("MCAR", System.StringComparison.OrdinalIgnoreCase))
                    continue;
                tiuDocs.Add(lines[i]);
            }
            return tiuDocs;
        }

        [HttpGet, Route("api/consult/find")]
        public async Task<ActionResult<string>> FindConsult([FromQuery] int consultIen)
        {
            var vq = new VistaQuery("ORQQCN FIND CONSULT");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/unresolved")]
        public async Task<ActionResult<string>> UnresolvedConsults([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQCN UNRESOLVED");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Services / Lookups

        [HttpGet, Route("api/consult/statuses")]
        public async Task<ActionResult<List<string>>> SubsetOfStatuses()
        {
            var vq = new VistaQuery("ORQQCN STATUS");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/urgencies")]
        public async Task<ActionResult<List<string>>> SubsetOfUrgencies([FromQuery] int consultIen)
        {
            var vq = new VistaQuery("ORQQCN URGENCIES");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/servicetree")]
        public async Task<ActionResult<List<string>>> LoadServiceTree([FromQuery] int purpose)
        {
            var vq = new VistaQuery("ORQQCN SVCTREE");
            vq.addParameter(VistaQuery.LITERAL, purpose.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/serviceswithsynonyms")]
        public async Task<ActionResult<List<string>>> LoadServicesWithSynonyms(
            [FromQuery] int startService, [FromQuery] int purpose,
            [FromQuery] bool showSynonyms = true, [FromQuery] int consultIen = -1)
        {
            var vq = new VistaQuery("ORQQCN SVC W/SYNONYMS");
            vq.addParameter(VistaQuery.LITERAL, startService.ToString());
            vq.addParameter(VistaQuery.LITERAL, purpose.ToString());
            vq.addParameter(VistaQuery.LITERAL, showSynonyms ? "1" : "0");
            if (consultIen >= 0)
                vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/servicelist")]
        public async Task<ActionResult<List<string>>> SubsetOfServices(
            [FromQuery] string startFrom, [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORQQCN SVCLIST");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/serviceien")]
        public async Task<ActionResult<string>> GetServiceIEN([FromQuery] string orderIen)
        {
            var vq = new VistaQuery("ORQQCN GET SERVICE IEN");
            vq.addParameter(VistaQuery.LITERAL, orderIen);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/procien")]
        public async Task<ActionResult<string>> GetProcedureIEN([FromQuery] string orderIen)
        {
            var vq = new VistaQuery("ORQQCN GET PROC IEN");
            vq.addParameter(VistaQuery.LITERAL, orderIen);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/procsvcs")]
        public async Task<ActionResult<List<string>>> GetProcedureServices([FromQuery] int procIen)
        {
            var vq = new VistaQuery("ORQQCN GET PROC SVCS");
            vq.addParameter(VistaQuery.LITERAL, procIen.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/ordernumber")]
        public async Task<ActionResult<string>> GetConsultOrderIEN([FromQuery] int consultIen)
        {
            var vq = new VistaQuery("ORQQCN GET ORDER NUMBER");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/provdx")]
        public async Task<ActionResult<string>> GetProvDxMode([FromQuery] string svcIen)
        {
            var vq = new VistaQuery("ORQQCN PROVDX");
            vq.addParameter(VistaQuery.LITERAL, svcIen);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/isprosvc")]
        public async Task<ActionResult<string>> IsProstheticsService([FromQuery] long svcIen)
        {
            var vq = new VistaQuery("ORQQCN ISPROSVC");
            vq.addParameter(VistaQuery.LITERAL, svcIen.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/userauth")]
        public async Task<ActionResult<string>> GetServiceUserLevel([FromQuery] int serviceIen)
        {
            var vq = new VistaQuery("ORQQCN GET USER AUTH");
            vq.addParameter(VistaQuery.LITERAL, serviceIen.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Titles

        [HttpGet, Route("api/consult/identifyconsultsclass")]
        public async Task<ActionResult<string>> IdentifyConsultsClass()
        {
            var vq = new VistaQuery("TIU IDENTIFY CONSULTS CLASS");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/identifyclinprocclass")]
        public async Task<ActionResult<string>> IdentifyClinProcClass()
        {
            var vq = new VistaQuery("TIU IDENTIFY CLINPROC CLASS");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/consulttitles")]
        public async Task<ActionResult<List<string>>> SubsetOfConsultTitles(
            [FromQuery] string startFrom, [FromQuery] int direction)
        {
            var vq = new VistaQuery("TIU LONG LIST CONSULT TITLES");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/clinproctitles")]
        public async Task<ActionResult<List<string>>> SubsetOfClinProcTitles(
            [FromQuery] string startFrom, [FromQuery] int direction)
        {
            var vq = new VistaQuery("TIU LONG LIST CLINPROC TITLES");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Order Dialog

        [HttpGet, Route("api/consult/oddefconsult")]
        public async Task<ActionResult<List<string>>> ODForConsults()
        {
            var vq = new VistaQuery("ORWDCN32 DEF");
            vq.addParameter(VistaQuery.LITERAL, "C");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/oddefprocedure")]
        public async Task<ActionResult<List<string>>> ODForProcedures()
        {
            var vq = new VistaQuery("ORWDCN32 DEF");
            vq.addParameter(VistaQuery.LITERAL, "P");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/procedures")]
        public async Task<ActionResult<List<string>>> SubsetOfProcedures(
            [FromQuery] string startFrom, [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWDCN32 PROCEDURES");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/ordermsg")]
        public async Task<ActionResult<string>> ConsultMessage([FromQuery] int ien)
        {
            var vq = new VistaQuery("ORWDCN32 ORDRMSG");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/newdlg")]
        public async Task<ActionResult<string>> GetNewDialog(
            [FromQuery] string orderType, [FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDCN32 NEWDLG");
            vq.addParameter(VistaQuery.LITERAL, orderType);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/defaultreason")]
        public async Task<ActionResult<List<string>>> GetDefaultReasonForRequest(
            [FromQuery] string service, [FromQuery] string dfn,
            [FromQuery] bool resolve = false)
        {
            var vq = new VistaQuery("ORQQCN DEFAULT REQUEST REASON");
            vq.addParameter(VistaQuery.LITERAL, service);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, resolve ? "1" : "0");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/editdefaultreason")]
        public async Task<ActionResult<string>> ReasonForRequestEditable([FromQuery] string service)
        {
            var vq = new VistaQuery("ORQQCN EDIT DEFAULT REASON");
            vq.addParameter(VistaQuery.LITERAL, service);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/prerequisite")]
        public async Task<ActionResult<List<string>>> GetServicePrerequisites(
            [FromQuery] string service, [FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQCN PREREQ CHK");
            vq.addParameter(VistaQuery.LITERAL, service);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Actions

        [HttpGet, Route("api/consult/actionmenus")]
        public async Task<ActionResult<string>> GetActionMenuLevel([FromQuery] int consultIen)
        {
            var vq = new VistaQuery("ORQQCN SET ACT MENUS");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/receive")]
        public async Task<ActionResult<List<string>>> ReceiveConsult(
            [FromQuery] int ien, [FromQuery] long receivedBy,
            [FromQuery] string rcptDate, [FromBody] List<string> comments)
        {
            var vq = new VistaQuery("ORQQCN RECEIVE");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, receivedBy.ToString());
            vq.addParameter(VistaQuery.LITERAL, rcptDate);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < comments.Count; i++)
                dhl.Add(i.ToString(), comments[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/consult/schedule")]
        public async Task<ActionResult<List<string>>> ScheduleConsult(
            [FromQuery] int ien, [FromQuery] long scheduledBy,
            [FromQuery] string schdDate, [FromQuery] int alert,
            [FromQuery] string alertTo, [FromBody] List<string> comments)
        {
            var vq = new VistaQuery("ORQQCN2 SCHEDULE CONSULT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, scheduledBy.ToString());
            vq.addParameter(VistaQuery.LITERAL, schdDate);
            vq.addParameter(VistaQuery.LITERAL, alert.ToString());
            vq.addParameter(VistaQuery.LITERAL, alertTo);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < comments.Count; i++)
                dhl.Add(i.ToString(), comments[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/consult/deny")]
        public async Task<ActionResult<List<string>>> DenyConsult(
            [FromQuery] int ien, [FromQuery] long deniedBy,
            [FromQuery] string denialDate, [FromBody] List<string> comments)
        {
            var vq = new VistaQuery("ORQQCN DISCONTINUE");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, deniedBy.ToString());
            vq.addParameter(VistaQuery.LITERAL, denialDate);
            vq.addParameter(VistaQuery.LITERAL, "DY");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < comments.Count; i++)
                dhl.Add(i.ToString(), comments[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/consult/discontinue")]
        public async Task<ActionResult<List<string>>> DiscontinueConsult(
            [FromQuery] int ien, [FromQuery] long discontinuedBy,
            [FromQuery] string discontinueDate, [FromBody] List<string> comments)
        {
            var vq = new VistaQuery("ORQQCN DISCONTINUE");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, discontinuedBy.ToString());
            vq.addParameter(VistaQuery.LITERAL, discontinueDate);
            vq.addParameter(VistaQuery.LITERAL, "DC");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < comments.Count; i++)
                dhl.Add(i.ToString(), comments[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/consult/forward")]
        public async Task<ActionResult<List<string>>> ForwardConsult(
            [FromQuery] int ien, [FromQuery] int toService,
            [FromQuery] long forwarder, [FromQuery] long attentionOf,
            [FromQuery] int urgency, [FromQuery] string actionDate,
            [FromBody] List<string> comments)
        {
            var vq = new VistaQuery("ORQQCN FORWARD");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, toService.ToString());
            vq.addParameter(VistaQuery.LITERAL, forwarder.ToString());
            vq.addParameter(VistaQuery.LITERAL, attentionOf.ToString());
            vq.addParameter(VistaQuery.LITERAL, urgency.ToString());
            vq.addParameter(VistaQuery.LITERAL, actionDate);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < comments.Count; i++)
                dhl.Add(i.ToString(), comments[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/consult/addcomment")]
        public async Task<ActionResult<List<string>>> AddComment(
            [FromQuery] int ien, [FromQuery] int alert,
            [FromQuery] string alertTo, [FromQuery] string actionDate,
            [FromBody] List<string> comments)
        {
            var vq = new VistaQuery("ORQQCN ADDCMT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < comments.Count; i++)
                dhl.Add(i.ToString(), comments[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, alert.ToString());
            vq.addParameter(VistaQuery.LITERAL, alertTo);
            vq.addParameter(VistaQuery.LITERAL, actionDate);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/consult/admincomplete")]
        public async Task<ActionResult<List<string>>> AdminComplete(
            [FromQuery] int ien, [FromQuery] string sigFindings,
            [FromQuery] long respProv, [FromQuery] int alert,
            [FromQuery] string alertTo, [FromQuery] string actionDate,
            [FromBody] List<string> comments)
        {
            var vq = new VistaQuery("ORQQCN ADMIN COMPLETE");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, sigFindings);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < comments.Count; i++)
                dhl.Add(i.ToString(), comments[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, respProv.ToString());
            vq.addParameter(VistaQuery.LITERAL, alert.ToString());
            vq.addParameter(VistaQuery.LITERAL, alertTo);
            vq.addParameter(VistaQuery.LITERAL, actionDate);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/consult/sigfindings")]
        public async Task<ActionResult<List<string>>> SigFindings(
            [FromQuery] int ien, [FromQuery] string sigFindingsFlag,
            [FromQuery] int alert, [FromQuery] string alertTo,
            [FromQuery] string actionDate, [FromBody] List<string> comments)
        {
            var vq = new VistaQuery("ORQQCN SIGFIND");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, sigFindingsFlag);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < comments.Count; i++)
                dhl.Add(i.ToString(), comments[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, alert.ToString());
            vq.addParameter(VistaQuery.LITERAL, alertTo);
            vq.addParameter(VistaQuery.LITERAL, actionDate);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Medicine Results

        [HttpGet, Route("api/consult/medresults")]
        public async Task<ActionResult<List<string>>> DisplayMedResults([FromQuery] int ien)
        {
            var vq = new VistaQuery("ORQQCN MED RESULTS");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/assignablemedresults")]
        public async Task<ActionResult<List<string>>> GetAssignableMedResults([FromQuery] int consultIen)
        {
            var vq = new VistaQuery("ORQQCN ASSIGNABLE MED RESULTS");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/removablemedresults")]
        public async Task<ActionResult<List<string>>> GetRemovableMedResults([FromQuery] int consultIen)
        {
            var vq = new VistaQuery("ORQQCN REMOVABLE MED RESULTS");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/medresultdetails")]
        public async Task<ActionResult<List<string>>> GetDetailedMedicineResults([FromQuery] string resultId)
        {
            var vq = new VistaQuery("ORQQCN MED RESULT DETAILS");
            vq.addParameter(VistaQuery.LITERAL, resultId);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/consult/attachmedresult")]
        public async Task<ActionResult<string>> AttachMedicineResult(
            [FromQuery] int consultIen, [FromQuery] string resultId,
            [FromQuery] string dateTime, [FromQuery] long responsiblePerson,
            [FromQuery] string alertTo = "")
        {
            var vq = new VistaQuery("ORQQCN ATTACH MED RESULTS");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, resultId);
            vq.addParameter(VistaQuery.LITERAL, dateTime);
            vq.addParameter(VistaQuery.LITERAL, responsiblePerson.ToString());
            vq.addParameter(VistaQuery.LITERAL, alertTo);
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/removemedresult")]
        public async Task<ActionResult<string>> RemoveMedicineResult(
            [FromQuery] int consultIen, [FromQuery] string resultId,
            [FromQuery] string dateTime, [FromQuery] long responsiblePerson)
        {
            var vq = new VistaQuery("ORQQCN REMOVE MED RESULTS");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, resultId);
            vq.addParameter(VistaQuery.LITERAL, dateTime);
            vq.addParameter(VistaQuery.LITERAL, responsiblePerson.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Edit / Resubmit

        [HttpGet, Route("api/consult/canedit")]
        public async Task<ActionResult<string>> CanBeResubmitted([FromQuery] int consultIen)
        {
            var vq = new VistaQuery("ORQQCN CANEDIT");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/loadforedit")]
        public async Task<ActionResult<List<string>>> LoadConsultForEdit([FromQuery] int consultIen)
        {
            var vq = new VistaQuery("ORQQCN LOAD FOR EDIT");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/consult/resubmit")]
        public async Task<ActionResult<string>> ResubmitConsult(
            [FromQuery] int ien, [FromBody] List<string> editData)
        {
            var vq = new VistaQuery("ORQQCN RESUBMIT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < editData.Count; i++)
                dhl.Add(i.ToString(), editData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region SF513 / Print

        [HttpGet, Route("api/consult/showsf513")]
        public async Task<ActionResult<List<string>>> ShowSF513([FromQuery] int consultIen)
        {
            var vq = new VistaQuery("ORQQCN SHOW SF513");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/consult/printsf513")]
        public async Task<ActionResult<string>> PrintSF513(
            [FromQuery] int consultIen, [FromQuery] string chartCopy,
            [FromQuery] string device)
        {
            var vq = new VistaQuery("ORQQCN PRINT SF513");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, chartCopy);
            vq.addParameter(VistaQuery.LITERAL, device);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/windowsprintsf513")]
        public async Task<ActionResult<List<string>>> GetFormattedSF513(
            [FromQuery] int consultIen, [FromQuery] string chartCopy)
        {
            var vq = new VistaQuery("ORQQCN PRINT SF513");
            vq.addParameter(VistaQuery.LITERAL, consultIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, chartCopy);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Context

        [HttpGet, Route("api/consult/context")]
        public async Task<ActionResult<string>> GetCurrentContext([FromQuery] long userDuz)
        {
            var vq = new VistaQuery("ORQQCN2 GET CONTEXT");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/context")]
        public async Task<ActionResult<string>> SaveCurrentContext([FromQuery] string context)
        {
            var vq = new VistaQuery("ORQQCN2 SAVE CONTEXT");
            vq.addParameter(VistaQuery.LITERAL, context);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/consult/savedcpfields")]
        public async Task<ActionResult<string>> GetSavedCPFields([FromQuery] int noteIen)
        {
            var vq = new VistaQuery("ORWTIU GET SAVED CP FIELDS");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Web.CPRS Convenience Endpoints

        [HttpGet, Route("api/consult/services")]
        public async Task<ActionResult<List<string>>> Services()
        {
            var vq = new VistaQuery("ORQQCN SVC W/SYNONYMS");
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/consult/create")]
        public async Task<ActionResult<string>> Create([FromQuery] string service)
        {
            var vq = new VistaQuery("ORQQCN RECEIVE");
            vq.addParameter(VistaQuery.LITERAL, service ?? "");
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/addresult")]
        public async Task<ActionResult<string>> AddResult([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORQQCN SET ACT MENUS");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/comment")]
        public async Task<ActionResult<string>> Comment([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORQQCN ADDCMT");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/ws/receive")]
        public async Task<ActionResult<string>> SimpleReceive([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORQQCN RECEIVE");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/ws/schedule")]
        public async Task<ActionResult<string>> SimpleSchedule([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORQQCN RECEIVE");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/ws/deny")]
        public async Task<ActionResult<string>> SimpleDeny([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORQQCN DISCONTINUE");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/ws/discontinue")]
        public async Task<ActionResult<string>> SimpleDiscontinue([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORQQCN DISCONTINUE");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/ws/forward")]
        public async Task<ActionResult<string>> SimpleForward([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORQQCN FORWARD");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/ws/admincomplete")]
        public async Task<ActionResult<string>> SimpleAdminComplete([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORQQCN ADMIN COMPLETE");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/consult/ws/resubmit")]
        public async Task<ActionResult<string>> SimpleResubmit([FromQuery] string ien)
        {
            var vq = new VistaQuery("ORQQCN RESUBMIT");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
