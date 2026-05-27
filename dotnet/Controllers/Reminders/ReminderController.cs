using CarePlatform.Data.VistA;
using CarePlatform.Models.Reminders;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Reminders — unevaluated reminders, categories, evaluation, education,
    /// reminder dialogs, prompts, cover-sheet, folders, MH testing,
    /// women's health, GEC, MST, general findings.
    /// Migrated from cprs/rReminders.pas — ORQQPXRM *, ORQQPX *, PXRM *, PXRMRPCG *, PXRMRPCC * RPCs.
    /// </summary>
    public class ReminderController : BaseController
    {
        public ReminderController() : base() { }

        #region Reminder Lists & Categories

        /// <summary>
        /// Get unevaluated reminders for a patient.
        /// RPC: ORQQPXRM REMINDERS UNEVALUATED
        /// </summary>
        [HttpGet, Route("api/reminder/unevaluated")]
        public async Task<ActionResult<List<string>>> Unevaluated([FromQuery] string dfn, [FromQuery] string location = "0")
        {
            var vq = new VistaQuery("ORQQPXRM REMINDERS UNEVALUATED");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get applicable reminders for a patient (pre-evaluated by VistA).
        /// CPRS cover sheet uses this single RPC instead of the 2-step UNEVALUATED→EVALUATION.
        /// RPC: ORQQPXRM REMINDERS APPLICABLE
        /// </summary>
        [HttpGet, Route("api/reminder/applicable")]
        public async Task<ActionResult<List<string>>> Applicable([FromQuery] string dfn, [FromQuery] string location = "0")
        {
            var vq = new VistaQuery("ORQQPXRM REMINDERS APPLICABLE");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get reminder categories.
        /// RPC: ORQQPXRM REMINDER CATEGORIES
        /// </summary>
        [HttpGet, Route("api/reminder/categories")]
        public async Task<ActionResult<List<string>>> Categories([FromQuery] string dfn, [FromQuery] string location = "0")
        {
            var vq = new VistaQuery("ORQQPXRM REMINDER CATEGORIES");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a specific reminder category's sub-items.
        /// RPC: PXRM REMINDER CATEGORY
        /// </summary>
        [HttpGet, Route("api/reminder/category")]
        public async Task<ActionResult<List<string>>> Category([FromQuery] string ien)
        {
            var vq = new VistaQuery("PXRM REMINDER CATEGORY");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get combined reminders and categories.
        /// RPC: PXRM REMINDERS AND CATEGORIES
        /// </summary>
        [HttpGet, Route("api/reminder/remindersandcategories")]
        public async Task<ActionResult<List<string>>> RemindersAndCategories(
            [FromQuery] string dfn,
            [FromQuery] string location)
        {
            var vq = new VistaQuery("PXRM REMINDERS AND CATEGORIES");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get link sequence for reminders.
        /// RPC: ORQQPXRM REMINDER LINK SEQ
        /// </summary>
        [HttpGet, Route("api/reminder/linkseq")]
        public async Task<ActionResult<string>> ReminderLinkSeq(
            [FromQuery] string ien,
            [FromQuery] string parentIen)
        {
            var vq = new VistaQuery("ORQQPXRM REMINDER LINK SEQ");
            vq.addParameter(VistaQuery.LITERAL, ien);
            vq.addParameter(VistaQuery.LITERAL, parentIen);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Reminder Evaluation & Detail

        /// <summary>
        /// Evaluate a single reminder for a patient.
        /// RPC: ORQQPXRM REMINDER EVALUATION
        /// </summary>
        [HttpGet, Route("api/reminder/evaluate")]
        public async Task<ActionResult<List<string>>> Evaluate(
            [FromQuery] string dfn,
            [FromQuery] string reminderIen)
        {
            var vq = new VistaQuery("ORQQPXRM REMINDER EVALUATION");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, reminderIen);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Evaluate a batch of reminders for a patient.
        /// Mirrors Delphi EvaluateReminders: ORQQPXRM REMINDER EVALUATION with LIST param.
        /// See rReminders.pas — sends DFN + neworNetMult subscripted IEN list.
        /// </summary>
        [HttpPost, Route("api/reminder/evaluatelist")]
        public async Task<ActionResult<List<string>>> EvaluateList(
            [FromQuery] string dfn,
            [FromBody] List<string> reminderIens)
        {
            var vq = new VistaQuery("ORQQPXRM REMINDER EVALUATION");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < reminderIens.Count; i++)
                dhl.Add((i + 1).ToString(), reminderIens[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get reminder detail.
        /// RPC: ORQQPXRM REMINDER DETAIL
        /// </summary>
        [HttpGet, Route("api/reminder/detail")]
        public async Task<ActionResult<List<string>>> Detail(
            [FromQuery] string dfn,
            [FromQuery] string reminderIen)
        {
            var vq = new VistaQuery("ORQQPXRM REMINDER DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, reminderIen);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get reminder detail (alternate/legacy RPC).
        /// RPC: ORQQPX REMINDER DETAIL
        /// </summary>
        [HttpGet, Route("api/reminder/detailalt")]
        public async Task<ActionResult<List<string>>> DetailAlt(
            [FromQuery] string dfn,
            [FromQuery] string reminderIen)
        {
            var vq = new VistaQuery("ORQQPX REMINDER DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, reminderIen);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get reminder inquiry (clinical info).
        /// RPC: ORQQPXRM REMINDER INQUIRY
        /// </summary>
        [HttpGet, Route("api/reminder/inquiry")]
        public async Task<ActionResult<List<string>>> Inquiry([FromQuery] string reminderIen)
        {
            var vq = new VistaQuery("ORQQPXRM REMINDER INQUIRY");
            vq.addParameter(VistaQuery.LITERAL, reminderIen);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get reminder web URL.
        /// RPC: ORQQPXRM REMINDER WEB
        /// </summary>
        [HttpGet, Route("api/reminder/web")]
        public async Task<ActionResult<string>> ReminderWeb([FromQuery] string reminderIen)
        {
            var vq = new VistaQuery("ORQQPXRM REMINDER WEB");
            vq.addParameter(VistaQuery.LITERAL, reminderIen);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Education

        /// <summary>
        /// Get education summary for a patient.
        /// RPC: ORQQPXRM EDUCATION SUMMARY
        /// </summary>
        [HttpGet, Route("api/reminder/educationsummary")]
        public async Task<ActionResult<List<string>>> EducationSummary([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQPXRM EDUCATION SUMMARY");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get education subtopics.
        /// RPC: ORQQPXRM EDUCATION SUBTOPICS
        /// </summary>
        [HttpGet, Route("api/reminder/educationsubtopics")]
        public async Task<ActionResult<List<string>>> EducationSubtopics([FromQuery] string topicIen)
        {
            var vq = new VistaQuery("ORQQPXRM EDUCATION SUBTOPICS");
            vq.addParameter(VistaQuery.LITERAL, topicIen);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get education topic details.
        /// RPC: ORQQPXRM EDUCATION TOPIC
        /// </summary>
        [HttpGet, Route("api/reminder/educationtopic")]
        public async Task<ActionResult<List<string>>> EducationTopic([FromQuery] string topicIen)
        {
            var vq = new VistaQuery("ORQQPXRM EDUCATION TOPIC");
            vq.addParameter(VistaQuery.LITERAL, topicIen);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Reminder Dialogs & Prompts

        /// <summary>
        /// Get reminder dialog definition.
        /// RPC: ORQQPXRM REMINDER DIALOG
        /// </summary>
        [HttpGet, Route("api/reminder/dialog")]
        public async Task<ActionResult<List<string>>> Dialog(
            [FromQuery] string reminderIen,
            [FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQPXRM REMINDER DIALOG");
            vq.addParameter(VistaQuery.LITERAL, reminderIen);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get reminder dialog (TIU version).
        /// RPC: PXRM REMINDER DIALOG (TIU)
        /// </summary>
        [HttpGet, Route("api/reminder/dialogtiu")]
        public async Task<ActionResult<List<string>>> DialogTiu(
            [FromQuery] string reminderIen,
            [FromQuery] string dfn)
        {
            var vq = new VistaQuery("PXRM REMINDER DIALOG (TIU)");
            vq.addParameter(VistaQuery.LITERAL, reminderIen);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get dialog prompts.
        /// RPC: ORQQPXRM DIALOG PROMPTS
        /// </summary>
        [HttpGet, Route("api/reminder/dialogprompts")]
        public async Task<ActionResult<List<string>>> DialogPrompts(
            [FromQuery] string dialogIen,
            [FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQPXRM DIALOG PROMPTS");
            vq.addParameter(VistaQuery.LITERAL, dialogIen);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if a reminder dialog is active.
        /// RPC: ORQQPXRM DIALOG ACTIVE
        /// </summary>
        [HttpGet, Route("api/reminder/dialogactive")]
        public async Task<ActionResult<string>> DialogActive([FromQuery] string dialogIen)
        {
            var vq = new VistaQuery("ORQQPXRM DIALOG ACTIVE");
            vq.addParameter(VistaQuery.LITERAL, dialogIen);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if new reminders are active for a patient.
        /// RPC: ORQQPX NEW REMINDERS ACTIVE
        /// </summary>
        [HttpGet, Route("api/reminder/newactive")]
        public async Task<ActionResult<string>> NewRemindersActive()
        {
            var vq = new VistaQuery("ORQQPX NEW REMINDERS ACTIVE");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get progress note header for a reminder dialog.
        /// RPC: ORQQPXRM PROGRESS NOTE HEADER
        /// </summary>
        [HttpGet, Route("api/reminder/noteheader")]
        public async Task<ActionResult<string>> ProgressNoteHeader([FromQuery] string reminderIen)
        {
            var vq = new VistaQuery("ORQQPXRM PROGRESS NOTE HEADER");
            vq.addParameter(VistaQuery.LITERAL, reminderIen);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Cover Sheet & Folders

        /// <summary>
        /// Get reminder folders for a patient.
        /// RPC: ORQQPX GET FOLDERS
        /// </summary>
        [HttpGet, Route("api/reminder/folders")]
        public async Task<ActionResult<List<string>>> GetFolders([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQPX GET FOLDERS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Set reminder folders for a patient.
        /// RPC: ORQQPX SET FOLDERS
        /// </summary>
        [HttpPost, Route("api/reminder/setfolders")]
        public async Task<ActionResult<string>> SetFolders(
            [FromQuery] string dfn,
            [FromBody] List<string> folders)
        {
            var vq = new VistaQuery("ORQQPX SET FOLDERS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < folders.Count; i++)
                dhl.Add((i + 1).ToString(), folders[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get default locations for reminders.
        /// RPC: ORQQPX GET DEF LOCATIONS
        /// </summary>
        [HttpGet, Route("api/reminder/deflocations")]
        public async Task<ActionResult<List<string>>> GetDefLocations()
        {
            var vq = new VistaQuery("ORQQPX GET DEF LOCATIONS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Insert reminder text at cursor position in note.
        /// RPC: ORQQPX REM INSERT AT CURSOR
        /// </summary>
        [HttpGet, Route("api/reminder/insertcursor")]
        public async Task<ActionResult<string>> RemInsertAtCursor()
        {
            var vq = new VistaQuery("ORQQPX REM INSERT AT CURSOR");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if new cover-sheet reminders are active.
        /// RPC: ORQQPX NEW COVER SHEET ACTIVE
        /// </summary>
        [HttpGet, Route("api/reminder/newcoversheetactive")]
        public async Task<ActionResult<string>> NewCoverSheetActive()
        {
            var vq = new VistaQuery("ORQQPX NEW COVER SHEET ACTIVE");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get cover-sheet level reminder list.
        /// RPC: ORQQPX LVREMLST
        /// </summary>
        [HttpGet, Route("api/reminder/lvremlst")]
        public async Task<ActionResult<List<string>>> CoverSheetLevelList(
            [FromQuery] string loc,
            [FromQuery] string cls)
        {
            var vq = new VistaQuery("ORQQPX LVREMLST");
            vq.addParameter(VistaQuery.LITERAL, loc);
            vq.addParameter(VistaQuery.LITERAL, cls);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save cover-sheet level reminder settings.
        /// RPC: ORQQPX SAVELVL
        /// </summary>
        [HttpPost, Route("api/reminder/savelvl")]
        public async Task<ActionResult<string>> SaveLevel([FromBody] List<string> items)
        {
            var vq = new VistaQuery("ORQQPX SAVELVL");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < items.Count; i++)
                dhl.Add((i + 1).ToString(), items[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Women's Health & MST & GEC

        /// <summary>
        /// Save women's health data.
        /// RPC: ORQQPXRM WOMEN HEALTH SAVE
        /// </summary>
        [HttpPost, Route("api/reminder/womenhealthsave")]
        public async Task<ActionResult<string>> WomenHealthSave([FromBody] List<string> data)
        {
            var vq = new VistaQuery("ORQQPXRM WOMEN HEALTH SAVE");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < data.Count; i++)
                dhl.Add((i + 1).ToString(), data[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// MST (Military Sexual Trauma) update.
        /// RPC: ORQQPXRM MST UPDATE
        /// </summary>
        [HttpPost, Route("api/reminder/mstupdate")]
        public async Task<ActionResult<string>> MstUpdate([FromBody] List<string> data)
        {
            var vq = new VistaQuery("ORQQPXRM MST UPDATE");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < data.Count; i++)
                dhl.Add((i + 1).ToString(), data[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Open a GEC (Geriatrics/Extended Care) dialog.
        /// RPC: ORQQPXRM GEC DIALOG
        /// </summary>
        [HttpGet, Route("api/reminder/gecdialog")]
        public async Task<ActionResult<List<string>>> GecDialog(
            [FromQuery] string dfn,
            [FromQuery] string reminderIen)
        {
            var vq = new VistaQuery("ORQQPXRM GEC DIALOG");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, reminderIen);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if GEC processing is finished.
        /// RPC: ORQQPXRM GEC FINISHED?
        /// </summary>
        [HttpGet, Route("api/reminder/gecfinished")]
        public async Task<ActionResult<string>> GecFinished(
            [FromQuery] string dfn,
            [FromQuery] string reminderIen)
        {
            var vq = new VistaQuery("ORQQPXRM GEC FINISHED?");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, reminderIen);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Mental Health Integration

        /// <summary>
        /// Get MHV (My HealtheVet) data.
        /// RPC: ORQQPXRM MHV
        /// </summary>
        [HttpGet, Route("api/reminder/mhv")]
        public async Task<ActionResult<List<string>>> Mhv([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQPXRM MHV");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get MH DLL/DMS data.
        /// RPC: ORQQPXRM MHDLLDMS
        /// </summary>
        [HttpGet, Route("api/reminder/mhdlldms")]
        public async Task<ActionResult<string>> MhDllDms([FromQuery] string testName)
        {
            var vq = new VistaQuery("ORQQPXRM MHDLLDMS");
            vq.addParameter(VistaQuery.LITERAL, testName);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get MH DLL data.
        /// RPC: ORQQPXRM MHDLL
        /// </summary>
        [HttpGet, Route("api/reminder/mhdll")]
        public async Task<ActionResult<List<string>>> MhDll([FromQuery] string testName)
        {
            var vq = new VistaQuery("ORQQPXRM MHDLL");
            vq.addParameter(VistaQuery.LITERAL, testName);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region General Findings (PXRMRPCG / PXRMRPCC)

        /// <summary>
        /// Cancel a general finding update.
        /// RPC: PXRMRPCG CANCEL
        /// </summary>
        [HttpPost, Route("api/reminder/gencancel")]
        public async Task<ActionResult<string>> GenCancel([FromBody] List<string> data)
        {
            var vq = new VistaQuery("PXRMRPCG CANCEL");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < data.Count; i++)
                dhl.Add((i + 1).ToString(), data[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// General finding update.
        /// RPC: PXRMRPCG GENFUPD
        /// Mirrors cprs/rReminders.pas SaveGenFindingData — the LIST starts with
        /// a header row ("0^DFN^Visit^NoteIen^User^EncProv") followed by
        /// caret-pieced finding entries.
        /// </summary>
        [HttpPost, Route("api/reminder/genfupd")]
        public async Task<ActionResult<List<string>>> GenFUpdate(
            [FromBody] List<string> data,
            [FromQuery] string? noteIen = null,
            [FromQuery] string? dfn = null,
            [FromQuery] string? visit = null,
            [FromQuery] string? user = null,
            [FromQuery] string? encProv = null)
        {
            var vq = new VistaQuery("PXRMRPCG GENFUPD");
            var dhl = new DictionaryHashList();
            var idx = 1;
            // Prepend canonical header row when noteIen is supplied.
            if (!string.IsNullOrEmpty(noteIen))
            {
                dhl.Add(idx.ToString(), $"0^{dfn ?? ""}^{visit ?? ""}^{noteIen}^{user ?? ""}^{encProv ?? ""}");
                idx++;
            }
            for (int i = 0; i < data.Count; i++, idx++)
                dhl.Add(idx.ToString(), data[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// General finding validation.
        /// RPC: PXRMRPCG GENFVALD
        /// </summary>
        [HttpPost, Route("api/reminder/genfvald")]
        public async Task<ActionResult<List<string>>> GenFValidate([FromBody] List<string> data)
        {
            var vq = new VistaQuery("PXRMRPCG GENFVALD");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < data.Count; i++)
                dhl.Add((i + 1).ToString(), data[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// View general finding data.
        /// RPC: PXRMRPCG VIEW
        /// </summary>
        [HttpGet, Route("api/reminder/genview")]
        public async Task<ActionResult<List<string>>> GenView([FromQuery] string ien)
        {
            var vq = new VistaQuery("PXRMRPCG VIEW");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Prompt value list for a general finding.
        /// RPC: PXRMRPCC PROMPTVL
        /// </summary>
        [HttpGet, Route("api/reminder/promptvl")]
        public async Task<ActionResult<List<string>>> PromptValueList([FromQuery] string ien)
        {
            var vq = new VistaQuery("PXRMRPCC PROMPTVL");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Structured Reminder Dialog Runtime (web extensions)

        /// <summary>
        /// Get the structured reminder-dialog definition (JSON) for rendering
        /// every component type — Radio/Check/FreeText/Date/Numeric/Combo.
        /// Calls canonical <c>PXRM REMINDER DIALOG (TIU)</c> (cprs/rReminders.pas L152)
        /// and parses the caret-delimited wire payload server-side into the
        /// strongly-typed <see cref="ReminderDialogDefinition"/>.
        /// </summary>
        [HttpGet, Route("api/reminder/dialogdef")]
        public async Task<ActionResult<string>> DialogDefinition(
            [FromQuery] string ien,
            [FromQuery] string? dfn = null)
        {
            var vq = new VistaQuery("PXRM REMINDER DIALOG (TIU)");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            var lines = await this.Session.tQuery(vq);
            if (lines == null || lines.Count == 0)
                return "";
            var def = ReminderDialogWireParser.Parse(lines);
            return JsonSerializer.Serialize(def);
        }

        #endregion
    }
}
