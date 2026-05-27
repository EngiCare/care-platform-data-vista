// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using CarePlatform.Models.Common;
using CarePlatform.Models.Notes;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// TIU progress notes — list, read, create, edit, sign, delete, print, cosigner management.
    /// Migrated from cprs/rTIU.pas — TIU *, ORWTIU *, ORWPCE NOTEVSTR RPCs.
    /// </summary>
    public class NoteController : BaseController
    {
        public NoteController() : base() { }

        #region Note Listing & Context

        /// <summary>
        /// List notes by context (signed, unsigned, by author, etc.).
        /// RPC: TIU DOCUMENTS BY CONTEXT
        /// Context: 1=All signed, 2=Unsigned, 3=Uncosigned, 4=By author, 5=By date range
        /// </summary>
        [HttpGet, Route("api/note/list")]
        public async Task<ActionResult<List<TiuDocument>>> ListNotes(
            [FromQuery] string dfn,
            [FromQuery] int context,
            [FromQuery] string early = "",
            [FromQuery] string late = "",
            [FromQuery] long person = 0,
            [FromQuery] int occurrenceLimit = 0,
            [FromQuery] string sortSequence = "",
            [FromQuery] bool showAddenda = false)
        {
            var vq = new VistaQuery("TIU DOCUMENTS BY CONTEXT");
            vq.addParameter(VistaQuery.LITERAL, "3"); // class = progress notes
            vq.addParameter(VistaQuery.LITERAL, context.ToString());
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, early);
            vq.addParameter(VistaQuery.LITERAL, late);
            vq.addParameter(VistaQuery.LITERAL, person.ToString());
            vq.addParameter(VistaQuery.LITERAL, occurrenceLimit.ToString());
            vq.addParameter(VistaQuery.LITERAL, sortSequence);
            vq.addParameter(VistaQuery.LITERAL, showAddenda ? "1" : "0");
            var results = await this.Session.tQuery(vq);
            return TiuDocument.ParseList(results);
        }

        /// <summary>
        /// List DC summaries.
        /// RPC: TIU SUMMARIES
        /// </summary>
        [HttpGet, Route("api/note/summaries")]
        public async Task<ActionResult<List<string>>> Summaries([FromQuery] string dfn)
        {
            var vq = new VistaQuery("TIU SUMMARIES");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get current TIU context preferences for the user.
        /// RPC: ORWTIU GET TIU CONTEXT
        /// Returns: semicolon-delimited context string
        /// </summary>
        [HttpGet, Route("api/note/context")]
        public async Task<ActionResult<string>> GetContext([FromQuery] long duz)
        {
            var vq = new VistaQuery("ORWTIU GET TIU CONTEXT");
            vq.addParameter(VistaQuery.LITERAL, duz.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save TIU context preferences.
        /// RPC: ORWTIU SAVE TIU CONTEXT
        /// </summary>
        [HttpPost, Route("api/note/savecontext")]
        public async Task<ActionResult<string>> SaveContext([FromQuery] string contextString)
        {
            var vq = new VistaQuery("ORWTIU SAVE TIU CONTEXT");
            vq.addParameter(VistaQuery.LITERAL, contextString);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get TIU site parameters.
        /// RPC: TIU GET SITE PARAMETERS
        /// </summary>
        [HttpGet, Route("api/note/siteparameters")]
        public async Task<ActionResult<string>> SiteParameters()
        {
            var vq = new VistaQuery("TIU GET SITE PARAMETERS");
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Note Reading

        /// <summary>
        /// Get the full text of a note.
        /// RPC: TIU GET RECORD TEXT
        /// </summary>
        [HttpGet, Route("api/note/text")]
        public async Task<ActionResult<List<string>>> NoteText([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU GET RECORD TEXT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get detailed display of a note (includes header/status info).
        /// RPC: TIU DETAILED DISPLAY
        /// </summary>
        [HttpGet, Route("api/note/detaileddisplay")]
        public async Task<ActionResult<List<string>>> DetailedDisplay([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU DETAILED DISPLAY");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load a note for editing (fields + text).
        /// RPC: TIU LOAD RECORD FOR EDIT
        /// </summary>
        [HttpGet, Route("api/note/loadforedit")]
        public async Task<ActionResult<List<string>>> LoadForEdit([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU LOAD RECORD FOR EDIT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, ".01;.06;.07;1301;1204;1208;1701;1205;1405;2101;70201;70202");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load note text only (for editing).
        /// RPC: TIU LOAD RECORD TEXT
        /// </summary>
        [HttpGet, Route("api/note/edittextonly")]
        public async Task<ActionResult<List<string>>> EditTextOnly([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU LOAD RECORD TEXT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load boilerplate text for a title.
        /// RPC: TIU LOAD BOILERPLATE TEXT
        /// </summary>
        [HttpGet, Route("api/note/boilerplate")]
        public async Task<ActionResult<List<string>>> Boilerplate(
            [FromQuery] int title,
            [FromQuery] string dfn,
            [FromQuery] string visitStr = "")
        {
            var vq = new VistaQuery("TIU LOAD BOILERPLATE TEXT");
            vq.addParameter(VistaQuery.LITERAL, title.ToString());
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, visitStr);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the listbox display item for a TIU document.
        /// RPC: ORWTIU GET LISTBOX ITEM
        /// </summary>
        [HttpGet, Route("api/note/listboxitem")]
        public async Task<ActionResult<string>> ListboxItem([FromQuery] long ien)
        {
            var vq = new VistaQuery("ORWTIU GET LISTBOX ITEM");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if note has text content.
        /// RPC: ORWTIU CHKTXT
        /// Returns: > '0' if note has text
        /// </summary>
        [HttpGet, Route("api/note/hastext")]
        public async Task<ActionResult<string>> HasText([FromQuery] int noteIen)
        {
            var vq = new VistaQuery("ORWTIU CHKTXT");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Total signed/unsigned note count for a patient — used to populate
        /// the Notes tab badge. RPC: ORCNOTE GET TOTAL.
        /// </summary>
        [HttpGet, Route("api/notes/count")]
        public async Task<ActionResult<int>> NoteCount([FromQuery] long dfn)
        {
            var vq = new VistaQuery("ORCNOTE GET TOTAL");
            vq.addParameter(VistaQuery.LITERAL, dfn.ToString());
            var raw = await this.Session.sQuery(vq);
            return int.TryParse((raw ?? "0").Trim(), out var n) ? n : 0;
        }

        #endregion

        #region Note Titles

        /// <summary>
        /// Get a subset of note titles (for long list box).
        /// RPC: TIU LONG LIST OF TITLES
        /// </summary>
        [HttpGet, Route("api/note/titles")]
        public async Task<ActionResult<List<string>>> Titles(
            [FromQuery] string startFrom,
            [FromQuery] int direction,
            [FromQuery] bool idNotesOnly = false)
        {
            var vq = new VistaQuery("TIU LONG LIST OF TITLES");
            vq.addParameter(VistaQuery.LITERAL, "3"); // class = progress notes
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            if (idNotesOnly)
                vq.addParameter(VistaQuery.LITERAL, "1");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the print name for a title.
        /// RPC: TIU GET PRINT NAME
        /// </summary>
        [HttpGet, Route("api/note/printname")]
        public async Task<ActionResult<string>> PrintName([FromQuery] int titleIen)
        {
            var vq = new VistaQuery("TIU GET PRINT NAME");
            vq.addParameter(VistaQuery.LITERAL, titleIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the document title IEN for a note.
        /// RPC: TIU GET DOCUMENT TITLE
        /// </summary>
        [HttpGet, Route("api/note/documenttitle")]
        public async Task<ActionResult<string>> DocumentTitle([FromQuery] long ien)
        {
            var vq = new VistaQuery("TIU GET DOCUMENT TITLE");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a title is a consult title.
        /// RPC: TIU IS THIS A CONSULT?
        /// </summary>
        [HttpGet, Route("api/note/isconsulttitle")]
        public async Task<ActionResult<string>> IsConsultTitle([FromQuery] int titleIen)
        {
            var vq = new VistaQuery("TIU IS THIS A CONSULT?");
            vq.addParameter(VistaQuery.LITERAL, titleIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a title is a PRF (Patient Record Flag) title.
        /// RPC: TIU ISPRF
        /// </summary>
        [HttpGet, Route("api/note/isprftitle")]
        public async Task<ActionResult<string>> IsPrfTitle([FromQuery] int titleIen)
        {
            var vq = new VistaQuery("TIU ISPRF");
            vq.addParameter(VistaQuery.LITERAL, titleIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a title is a clinical procedure title.
        /// RPC: TIU IS THIS A CLINPROC?
        /// </summary>
        [HttpGet, Route("api/note/isclinproctitle")]
        public async Task<ActionResult<string>> IsClinProcTitle([FromQuery] int titleIen)
        {
            var vq = new VistaQuery("TIU IS THIS A CLINPROC?");
            vq.addParameter(VistaQuery.LITERAL, titleIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get user's personal short list of note titles and default title.
        /// RPC: TIU PERSONAL TITLE LIST
        /// Returns: list of IEN^TitleName, with default flagged
        /// </summary>
        [HttpGet, Route("api/note/personaltitles")]
        public async Task<ActionResult<List<string>>> PersonalTitles(
            [FromQuery] long duz,
            [FromQuery] int docClass = 3)
        {
            var vq = new VistaQuery("TIU PERSONAL TITLE LIST");
            vq.addParameter(VistaQuery.LITERAL, duz.ToString());
            vq.addParameter(VistaQuery.LITERAL, docClass.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get user's TIU personal preferences (default location, cosigner, sort, max notes, etc.).
        /// RPC: TIU GET PERSONAL PREFERENCES
        /// Returns: caret-delimited preference string
        /// </summary>
        [HttpGet, Route("api/note/personalpreferences")]
        public async Task<ActionResult<string>> PersonalPreferences()
        {
            var vq = new VistaQuery("TIU GET PERSONAL PREFERENCES");
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Note Create / Edit / Save

        /// <summary>
        /// Create a new note.
        /// RPC: TIU CREATE RECORD
        /// Returns: IEN^ErrorText
        /// </summary>
        [HttpPost, Route("api/note/create")]
        public async Task<ActionResult<CreateNoteResult>> Create(
            [FromQuery] string dfn,
            [FromQuery] int title,
            [FromQuery] string visitStr,
            [FromQuery] long authorDuz,
            [FromQuery] string fmDate = "",
            [FromQuery] long cosignerDuz = 0,
            [FromQuery] string subject = "",
            [FromQuery] int consultIen = 0)
        {
            var vq = new VistaQuery("TIU CREATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, title.ToString());
            vq.addParameter(VistaQuery.LITERAL, ""); // reserved
            vq.addParameter(VistaQuery.LITERAL, ""); // reserved
            vq.addParameter(VistaQuery.LITERAL, ""); // reserved
            // Build field string — matches rTIU.pas PutNewNote field layout
            // 1205=Location(cosigner reuse), 1208=Subject, 1701=Subject(legacy), 1405=PkgRef
            var pkgRef = consultIen > 0 ? $"{consultIen};GMR(123," : "";
            string fields = string.Format(".01;{0}|1202;{1}|1301;{2}|1205;{3}|1208;{4}|1701;{5}",
                title, authorDuz, fmDate, cosignerDuz, subject, "");
            if (!string.IsNullOrEmpty(pkgRef))
                fields += $"|1405;{pkgRef}";
            vq.addParameter(VistaQuery.LITERAL, fields);
            vq.addParameter(VistaQuery.LITERAL, visitStr);
            vq.addParameter(VistaQuery.LITERAL, "1"); // suppress commit logic
            var result = await this.Session.sQuery(vq);
            return CreateNoteResult.Parse(result);
        }

        /// <summary>
        /// Create an addendum to an existing note.
        /// RPC: TIU CREATE ADDENDUM RECORD
        /// Returns: IEN^ErrorText
        /// </summary>
        [HttpPost, Route("api/note/createaddendum")]
        public async Task<ActionResult<CreateNoteResult>> CreateAddendum(
            [FromQuery] int addendumTo,
            [FromQuery] long authorDuz,
            [FromQuery] string fmDate = "",
            [FromQuery] long cosignerDuz = 0)
        {
            var vq = new VistaQuery("TIU CREATE ADDENDUM RECORD");
            vq.addParameter(VistaQuery.LITERAL, addendumTo.ToString());
            string fields = string.Format("1202;{0}|1301;{1}|1208;{2}",
                authorDuz, fmDate, cosignerDuz);
            vq.addParameter(VistaQuery.LITERAL, fields);
            vq.addParameter(VistaQuery.LITERAL, "1"); // suppress commit
            var result = await this.Session.sQuery(vq);
            return CreateNoteResult.Parse(result);
        }

        /// <summary>
        /// Update a note record (fields only).
        /// RPC: TIU UPDATE RECORD
        /// Returns: IEN^ErrorText
        /// </summary>
        [HttpPost, Route("api/note/update")]
        public async Task<ActionResult<string>> Update(
            [FromQuery] int noteIen,
            [FromQuery] string fields)
        {
            var vq = new VistaQuery("TIU UPDATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, fields);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Set the text of a note.
        /// RPC: TIU SET DOCUMENT TEXT
        /// </summary>
        [HttpPost, Route("api/note/settext")]
        public async Task<ActionResult<string>> SetText(
            [FromQuery] long noteIen,
            [FromBody] List<string> noteText,
            [FromQuery] int suppress = 0)
        {
            var vq = new VistaQuery("TIU SET DOCUMENT TEXT");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < noteText.Count; i++)
                dhl.Add("\"TEXT\"," + (i + 1) + ",0", noteText[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, suppress.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Note Actions (Sign, Delete, Lock, etc.)

        /// <summary>
        /// Check authorization to perform an action on a document.
        /// RPC: TIU AUTHORIZATION
        /// Returns: '1'^reason if authorized, '0'^reason if not
        /// </summary>
        [HttpGet, Route("api/note/authorization")]
        public async Task<ActionResult<string>> Authorization(
            [FromQuery] int ien,
            [FromQuery] string actionName)
        {
            var vq = new VistaQuery("TIU AUTHORIZATION");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, actionName);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Sign a document with electronic signature.
        /// RPC: TIU SIGN RECORD
        /// Returns: '0'^reason (success) or error text
        /// </summary>
        [HttpPost, Route("api/note/sign")]
        public async Task<ActionResult<string>> Sign(
            [FromQuery] int ien,
            [FromQuery] string esCode)
        {
            var vq = new VistaQuery("TIU SIGN RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, esCode);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Delete a document.
        /// RPC: TIU DELETE RECORD
        /// </summary>
        [HttpPost, Route("api/note/delete")]
        public async Task<ActionResult<List<string>>> Delete(
            [FromQuery] int ien,
            [FromQuery] string reason)
        {
            var vq = new VistaQuery("TIU DELETE RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, reason);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if delete justification is required.
        /// RPC: TIU JUSTIFY DELETE?
        /// Returns: '1' if justification is required
        /// </summary>
        [HttpGet, Route("api/note/justifydelete")]
        public async Task<ActionResult<string>> JustifyDelete([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU JUSTIFY DELETE?");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Lock a document for editing.
        /// RPC: TIU LOCK RECORD
        /// Returns: '0' = success, else error in piece 2
        /// </summary>
        [HttpPost, Route("api/note/lock")]
        public async Task<ActionResult<string>> Lock([FromQuery] long ien)
        {
            var vq = new VistaQuery("TIU LOCK RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Unlock a document.
        /// RPC: TIU UNLOCK RECORD
        /// </summary>
        [HttpPost, Route("api/note/unlock")]
        public async Task<ActionResult<string>> Unlock([FromQuery] long ien)
        {
            var vq = new VistaQuery("TIU UNLOCK RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if last save was clean (no unsaved changes).
        /// RPC: TIU WAS THIS SAVED?
        /// </summary>
        [HttpGet, Route("api/note/wassaved")]
        public async Task<ActionResult<string>> WasSaved([FromQuery] long ien)
        {
            var vq = new VistaQuery("TIU WAS THIS SAVED?");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if author has signed a document.
        /// RPC: TIU HAS AUTHOR SIGNED?
        /// </summary>
        [HttpGet, Route("api/note/authorsigned")]
        public async Task<ActionResult<string>> AuthorSigned(
            [FromQuery] int ien,
            [FromQuery] long duz)
        {
            var vq = new VistaQuery("TIU HAS AUTHOR SIGNED?");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, duz.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Determine which signature action is needed (SIGNATURE vs COSIGNATURE).
        /// RPC: TIU WHICH SIGNATURE ACTION
        /// </summary>
        [HttpGet, Route("api/note/whichsignatureaction")]
        public async Task<ActionResult<string>> WhichSignatureAction([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU WHICH SIGNATURE ACTION");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if cosignature is required.
        /// RPC: TIU REQUIRES COSIGNATURE
        /// </summary>
        [HttpGet, Route("api/note/requirescosignature")]
        public async Task<ActionResult<string>> RequiresCosignature(
            [FromQuery] int titleOrType,
            [FromQuery] int docOrZero,
            [FromQuery] long authorDuz,
            [FromQuery] string fmDate = "")
        {
            var vq = new VistaQuery("TIU REQUIRES COSIGNATURE");
            vq.addParameter(VistaQuery.LITERAL, titleOrType.ToString());
            vq.addParameter(VistaQuery.LITERAL, docOrZero.ToString());
            vq.addParameter(VistaQuery.LITERAL, authorDuz.ToString());
            if (!string.IsNullOrEmpty(fmDate))
                vq.addParameter(VistaQuery.LITERAL, fmDate);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if cosigner can be changed.
        /// RPC: TIU CAN CHANGE COSIGNER?
        /// </summary>
        [HttpGet, Route("api/note/canchangecosigner")]
        public async Task<ActionResult<string>> CanChangeCosigner([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU CAN CHANGE COSIGNER?");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Additional Signers

        /// <summary>
        /// Get additional signers for a note.
        /// RPC: TIU GET ADDITIONAL SIGNERS
        /// </summary>
        [HttpGet, Route("api/note/additionalsigners")]
        public async Task<ActionResult<List<string>>> AdditionalSigners([FromQuery] int ien)
        {
            var vq = new VistaQuery("TIU GET ADDITIONAL SIGNERS");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Update additional signers for a note.
        /// RPC: TIU UPDATE ADDITIONAL SIGNERS
        /// </summary>
        [HttpPost, Route("api/note/updateadditionalsigners")]
        public async Task<ActionResult<string>> UpdateAdditionalSigners(
            [FromQuery] int ien,
            [FromBody] List<string> signers)
        {
            var vq = new VistaQuery("TIU UPDATE ADDITIONAL SIGNERS");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < signers.Count; i++)
                dhl.Add(i.ToString(), signers[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Print & Formatted Output

        /// <summary>
        /// Print a note to a device.
        /// RPC: TIU PRINT RECORD
        /// Returns: '0'^msg on success, error otherwise
        /// </summary>
        [HttpPost, Route("api/note/print")]
        public async Task<ActionResult<string>> Print(
            [FromQuery] int noteIen,
            [FromQuery] string device,
            [FromQuery] bool chartCopy = false)
        {
            var vq = new VistaQuery("TIU PRINT RECORD");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, device);
            vq.addParameter(VistaQuery.LITERAL, chartCopy ? "1" : "0");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get a Windows-formatted note for printing.
        /// RPC: ORWTIU WINPRINT NOTE
        /// </summary>
        [HttpGet, Route("api/note/formattedtext")]
        public async Task<ActionResult<List<string>>> FormattedText(
            [FromQuery] int noteIen,
            [FromQuery] bool chartCopy = false)
        {
            var vq = new VistaQuery("ORWTIU WINPRINT NOTE");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, chartCopy ? "1" : "0");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if a note can be printed (work copy / chart copy).
        /// RPC: TIU CAN PRINT WORK/CHART COPY
        /// Returns: '0'=no, '1'=work copy only, '2'=chart copy allowed
        /// </summary>
        [HttpGet, Route("api/note/canprint")]
        public async Task<ActionResult<string>> CanPrint([FromQuery] int noteIen)
        {
            var vq = new VistaQuery("TIU CAN PRINT WORK/CHART COPY");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get document parameters (used for chart print determination).
        /// RPC: TIU GET DOCUMENT PARAMETERS
        /// </summary>
        [HttpGet, Route("api/note/documentparameters")]
        public async Task<ActionResult<string>> DocumentParameters([FromQuery] int noteIen)
        {
            var vq = new VistaQuery("TIU GET DOCUMENT PARAMETERS");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Consult / Visit References

        /// <summary>
        /// Get visit string for a note.
        /// RPC: ORWPCE NOTEVSTR
        /// </summary>
        [HttpGet, Route("api/note/visitstring")]
        public async Task<ActionResult<string>> VisitString([FromQuery] int ien)
        {
            var vq = new VistaQuery("ORWPCE NOTEVSTR");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the package reference (e.g., consult IEN) for a note.
        /// RPC: TIU GET REQUEST
        /// </summary>
        [HttpGet, Route("api/note/packagereference")]
        public async Task<ActionResult<string>> PackageReference([FromQuery] int noteIen)
        {
            var vq = new VistaQuery("TIU GET REQUEST");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get consult requests for the patient (used for note linking).
        /// RPC: GMRC LIST CONSULT REQUESTS
        /// </summary>
        [HttpGet, Route("api/note/consultrequests")]
        public async Task<ActionResult<List<string>>> ConsultRequests([FromQuery] string dfn)
        {
            var vq = new VistaQuery("GMRC LIST CONSULT REQUESTS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get unresolved consults info for the patient.
        /// RPC: ORQQCN UNRESOLVED — returns "unresolvedExist^showNagScreen".
        /// Mirrors desktop CPRS rConsults.GetUnresolvedConsultsInfo.
        /// </summary>
        [HttpGet, Route("api/note/unresolvedconsults")]
        public async Task<ActionResult<string>> UnresolvedConsults([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQCN UNRESOLVED");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check one-note-per-visit constraint.
        /// RPC: TIU ONE VISIT NOTE?
        /// Returns: > '0' if another note exists for this visit
        /// </summary>
        [HttpGet, Route("api/note/onevisitcheck")]
        public async Task<ActionResult<string>> OneVisitCheck(
            [FromQuery] string noteIen,
            [FromQuery] string dfn,
            [FromQuery] string visitStr)
        {
            var vq = new VistaQuery("TIU ONE VISIT NOTE?");
            vq.addParameter(VistaQuery.LITERAL, noteIen);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, visitStr);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get ancillary package messages for a document action.
        /// RPC: TIU ANCILLARY PACKAGE MESSAGE
        /// </summary>
        [HttpGet, Route("api/note/ancillarymessages")]
        public async Task<ActionResult<List<string>>> AncillaryMessages(
            [FromQuery] int ien,
            [FromQuery] string action)
        {
            var vq = new VistaQuery("TIU ANCILLARY PACKAGE MESSAGE");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, action);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region ID Notes & Linking

        /// <summary>
        /// Check if a title can be an ID child (linkable).
        /// RPC: ORWTIU CANLINK
        /// Returns: '1' or '0'^WhyNot
        /// </summary>
        [HttpGet, Route("api/note/canlink")]
        public async Task<ActionResult<string>> CanLink([FromQuery] int title)
        {
            var vq = new VistaQuery("ORWTIU CANLINK");
            vq.addParameter(VistaQuery.LITERAL, title.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a document can be attached to a parent.
        /// RPC: TIU ID CAN ATTACH
        /// Returns: '1' or '0'^WhyNot or '-1'
        /// </summary>
        [HttpGet, Route("api/note/canattach")]
        public async Task<ActionResult<string>> CanAttach([FromQuery] string docId)
        {
            var vq = new VistaQuery("TIU ID CAN ATTACH");
            vq.addParameter(VistaQuery.LITERAL, docId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a document can receive an attachment.
        /// RPC: TIU ID CAN RECEIVE
        /// Returns: '1' or '0'^WhyNot
        /// </summary>
        [HttpGet, Route("api/note/canreceive")]
        public async Task<ActionResult<string>> CanReceive([FromQuery] string docId)
        {
            var vq = new VistaQuery("TIU ID CAN RECEIVE");
            vq.addParameter(VistaQuery.LITERAL, docId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Attach a child entry to a parent document.
        /// RPC: TIU ID ATTACH ENTRY
        /// Returns: IEN^ or '0'^WhyNot
        /// </summary>
        [HttpPost, Route("api/note/attach")]
        public async Task<ActionResult<string>> Attach(
            [FromQuery] string docId,
            [FromQuery] string parentDocId)
        {
            var vq = new VistaQuery("TIU ID ATTACH ENTRY");
            vq.addParameter(VistaQuery.LITERAL, docId);
            vq.addParameter(VistaQuery.LITERAL, parentDocId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Detach an entry from its parent.
        /// RPC: TIU ID DETACH ENTRY
        /// Returns: IEN^ or '0'^WhyNot
        /// </summary>
        [HttpPost, Route("api/note/detach")]
        public async Task<ActionResult<string>> Detach([FromQuery] string docId)
        {
            var vq = new VistaQuery("TIU ID DETACH ENTRY");
            vq.addParameter(VistaQuery.LITERAL, docId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Update a single field on an existing TIU document.
        /// RPC: TIU UPDATE RECORD
        /// Used by "Use Existing Note" to set PkgRef (field 1405) linking a note to a consult.
        /// </summary>
        [HttpPost, Route("api/note/updatefield")]
        public async Task<ActionResult<string>> UpdateField(
            [FromQuery] long noteIen,
            [FromQuery] string field,
            [FromQuery] string value)
        {
            var vq = new VistaQuery("TIU UPDATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            var dhl = new DictionaryHashList();
            dhl.Add(field, value);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region User Class

        /// <summary>
        /// Get subset of user classes.
        /// RPC: TIU USER CLASS LONG LIST
        /// </summary>
        [HttpGet, Route("api/note/userclasses")]
        public async Task<ActionResult<List<string>>> UserClasses(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("TIU USER CLASS LONG LIST");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get division and class info for a user.
        /// RPC: TIU DIV AND CLASS INFO
        /// </summary>
        [HttpGet, Route("api/note/divclassinfo")]
        public async Task<ActionResult<List<string>>> DivClassInfo([FromQuery] long userIen)
        {
            var vq = new VistaQuery("TIU DIV AND CLASS INFO");
            vq.addParameter(VistaQuery.LITERAL, userIen.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if a user is inactive.
        /// RPC: TIU USER INACTIVE?
        /// Returns: > '0' if user is inactive
        /// </summary>
        [HttpGet, Route("api/note/userinactive")]
        public async Task<ActionResult<string>> UserInactive([FromQuery] string ein)
        {
            var vq = new VistaQuery("TIU USER INACTIVE?");
            vq.addParameter(VistaQuery.LITERAL, ein);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region PCE / Encounter Findings (note-scoped)

        /// <summary>
        /// Save the encounter PCE delta for an authoring note. The body is the
        /// list of caret-pieced PCE entries (Category^Code^Narrative^...) — the
        /// same format consumed by ORWPCE PCE4NOTE on read.
        /// RPC: ORWPCE SAVE
        /// </summary>
        [HttpPost, Route("api/note/savepce")]
        public async Task<ActionResult<string>> SavePce(
            [FromQuery] long noteIen,
            [FromBody] List<string> entries)
        {
            var vq = new VistaQuery("ORWPCE SAVE");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            var dhl = new DictionaryHashList();
            if (entries != null)
            {
                for (var i = 0; i < entries.Count; i++)
                    dhl.Add((i + 1).ToString(), entries[i] ?? "");
            }
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save general findings (typically health factors) for the encounter
        /// associated with a note. Each entry is FindingType^FindingItem^Label.
        /// Mirrors cprs/rReminders.pas SaveGenFindingData: a single LIST whose
        /// subscript-0 header row carries DFN^Visit^NoteIen^User^EncProv and
        /// follow-on rows are caret-pieced finding entries.
        /// RPC: PXRMRPCG GENFUPD
        /// </summary>
        [HttpPost, Route("api/note/savehealthfactors")]
        public async Task<ActionResult<string>> SaveHealthFactors(
            [FromQuery] long noteIen,
            [FromBody] List<string> entries,
            [FromQuery] string? dfn = null,
            [FromQuery] string? visit = null,
            [FromQuery] string? user = null,
            [FromQuery] string? encProv = null)
        {
            var vq = new VistaQuery("PXRMRPCG GENFUPD");
            var dhl = new DictionaryHashList();
            // Header row at subscript 0: DFN^Visit^NoteIen^User^EncProv.
            dhl.Add("1", $"0^{dfn ?? ""}^{visit ?? ""}^{noteIen}^{user ?? ""}^{encProv ?? ""}");
            if (entries != null)
            {
                for (var i = 0; i < entries.Count; i++)
                    dhl.Add((i + 2).ToString(), entries[i] ?? "");
            }
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Simplified Aliases (used by Web.CPRS)

        /// <summary>
        /// Create an addendum (simplified alias).
        /// RPC: TIU CREATE ADDENDUM RECORD
        /// </summary>
        [HttpPost, Route("api/note/addendum")]
        public async Task<ActionResult<string>> Addendum(
            [FromQuery] string dfn,
            [FromQuery] long parentIen)
        {
            var vq = new VistaQuery("TIU CREATE ADDENDUM RECORD");
            vq.addParameter(VistaQuery.LITERAL, parentIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Change note title (simplified alias).
        /// RPC: TIU SET DOCUMENT TITLE
        /// </summary>
        [HttpPost, Route("api/note/changetitle")]
        public async Task<ActionResult<string>> ChangeTitle(
            [FromQuery] string ien,
            [FromQuery] string title)
        {
            var vq = new VistaQuery("TIU SET DOCUMENT TITLE");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, title);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Update note text (simplified alias).
        /// RPC: TIU UPDATE RECORD
        /// </summary>
        [HttpPost, Route("api/note/updatetext")]
        public async Task<ActionResult<string>> UpdateText(
            [FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU UPDATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get note properties / can-attach check (simplified alias).
        /// RPC: TIU ID CAN ATTACH
        /// </summary>
        [HttpGet, Route("api/note/properties")]
        public async Task<ActionResult<string>> Properties([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU ID CAN ATTACH");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save note properties (simplified alias).
        /// RPC: TIU SET DOCUMENT TITLE
        /// </summary>
        [HttpPost, Route("api/note/saveproperties")]
        public async Task<ActionResult<string>> SaveProperties([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU SET DOCUMENT TITLE");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Change the author of a note (field 1202).
        /// RPC: TIU UPDATE RECORD
        /// </summary>
        [HttpPost, Route("api/note/changeauthor")]
        public async Task<ActionResult<string>> ChangeAuthor(
            [FromQuery] string ien,
            [FromQuery] string authorDuz)
        {
            var vq = new VistaQuery("TIU UPDATE RECORD");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, $"1202/{authorDuz}");
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
