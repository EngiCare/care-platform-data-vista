using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// User preferences and options — notifications, order checks, surrogates,
    /// co-signers, document classes/titles, lab/appt/imaging/encounter ranges,
    /// reminders, clinic defaults, patient lists/teams, combos, report defaults,
    /// med ranges, copy-paste, and general "other" settings.
    /// Migrated from cprs/Options/rOptions.pas — ORWTPP *, ORWTPO *, ORWTPD *,
    /// ORWTPD1 *, ORWTPN *, ORWTIU *, ORWTPT *, TIU LONG LIST OF TITLES RPCs.
    /// </summary>
    public class OptionsController : BaseController
    {
        public OptionsController() : base() { }

        #region Notifications

        /// <summary>
        /// Retrieve notification settings for the current user.
        /// RPC: ORWTPP GETNOT
        /// </summary>
        [HttpGet, Route("api/options/notifications")]
        public async Task<ActionResult<List<string>>> GetNotifications()
        {
            var vq = new VistaQuery("ORWTPP GETNOT");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save notification settings for the current user.
        /// RPC: ORWTPP SAVENOT
        /// </summary>
        [HttpPost, Route("api/options/notifications")]
        public async Task<ActionResult<string>> SaveNotifications([FromBody] List<string> items)
        {
            var vq = new VistaQuery("ORWTPP SAVENOT");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < items.Count; i++)
                dhl.Add(i.ToString(), items[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get system-level notification defaults.
        /// RPC: ORWTPP GETNOTO
        /// </summary>
        [HttpGet, Route("api/options/notificationdefaults")]
        public async Task<ActionResult<string>> GetNotificationDefaults()
        {
            var vq = new VistaQuery("ORWTPP GETNOTO");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save notification default overrides (other stuff).
        /// RPC: ORWTPP SAVENOTO
        /// </summary>
        [HttpPost, Route("api/options/notificationdefaults")]
        public async Task<ActionResult<string>> SaveNotificationDefaults([FromQuery] string value)
        {
            var vq = new VistaQuery("ORWTPP SAVENOTO");
            vq.addParameter(VistaQuery.LITERAL, value);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Clear all notification flags for the current user.
        /// RPC: ORWTPP CLEARNOT
        /// </summary>
        [HttpPost, Route("api/options/notifications/clear")]
        public async Task<ActionResult<string>> ClearNotifications()
        {
            var vq = new VistaQuery("ORWTPP CLEARNOT");
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Order Checks

        /// <summary>
        /// Retrieve order-check settings for the current user.
        /// RPC: ORWTPP GETOC
        /// </summary>
        [HttpGet, Route("api/options/orderchecks")]
        public async Task<ActionResult<List<string>>> GetOrderChecks()
        {
            var vq = new VistaQuery("ORWTPP GETOC");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save order-check settings for the current user.
        /// RPC: ORWTPP SAVEOC
        /// </summary>
        [HttpPost, Route("api/options/orderchecks")]
        public async Task<ActionResult<string>> SaveOrderChecks([FromBody] List<string> items)
        {
            var vq = new VistaQuery("ORWTPP SAVEOC");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < items.Count; i++)
                dhl.Add(i.ToString(), items[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Surrogates

        /// <summary>
        /// Get the current user's surrogate information.
        /// RPC: ORWTPP GETSURR
        /// </summary>
        [HttpGet, Route("api/options/surrogate")]
        public async Task<ActionResult<string>> GetSurrogateInfo()
        {
            var vq = new VistaQuery("ORWTPP GETSURR");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check whether a given surrogate is valid.
        /// Returns "ok-flag^message".
        /// RPC: ORWTPP CHKSURR
        /// </summary>
        [HttpGet, Route("api/options/surrogate/check")]
        public async Task<ActionResult<string>> CheckSurrogate([FromQuery] long surrogate)
        {
            var vq = new VistaQuery("ORWTPP CHKSURR");
            vq.addParameter(VistaQuery.LITERAL, surrogate.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save surrogate information. Returns "ok-flag^message".
        /// RPC: ORWTPP SAVESURR
        /// </summary>
        [HttpPost, Route("api/options/surrogate")]
        public async Task<ActionResult<string>> SaveSurrogateInfo([FromQuery] string value)
        {
            var vq = new VistaQuery("ORWTPP SAVESURR");
            vq.addParameter(VistaQuery.LITERAL, value);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Co-signers

        /// <summary>
        /// Get a list of potential co-signers starting from a given name.
        /// RPC: ORWTPP GETCOS
        /// </summary>
        [HttpGet, Route("api/options/cosigners")]
        public async Task<ActionResult<List<string>>> GetCosigners(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWTPP GETCOS");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the default co-signer for the current user.
        /// RPC: ORWTPP GETDCOS
        /// </summary>
        [HttpGet, Route("api/options/cosigners/default")]
        public async Task<ActionResult<string>> GetDefaultCosigner()
        {
            var vq = new VistaQuery("ORWTPP GETDCOS");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Set the default co-signer for the current user.
        /// RPC: ORWTPP SETDCOS
        /// </summary>
        [HttpPost, Route("api/options/cosigners/default")]
        public async Task<ActionResult<string>> SetDefaultCosigner([FromQuery] long value)
        {
            var vq = new VistaQuery("ORWTPP SETDCOS");
            vq.addParameter(VistaQuery.LITERAL, value.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Subjects

        /// <summary>
        /// Get whether subjects are enabled for titles.
        /// RPC: ORWTPP GETSUB
        /// </summary>
        [HttpGet, Route("api/options/subject")]
        public async Task<ActionResult<string>> GetSubject()
        {
            var vq = new VistaQuery("ORWTPP GETSUB");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Set whether subjects are enabled for titles.
        /// RPC: ORWTPP SETSUB
        /// </summary>
        [HttpPost, Route("api/options/subject")]
        public async Task<ActionResult<string>> SetSubject([FromQuery] bool value)
        {
            var vq = new VistaQuery("ORWTPP SETSUB");
            vq.addParameter(VistaQuery.LITERAL, value ? "1" : "0");
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Document Classes & Titles

        /// <summary>
        /// Get the list of document classes.
        /// RPC: ORWTPN GETCLASS
        /// </summary>
        [HttpGet, Route("api/options/classes")]
        public async Task<ActionResult<List<string>>> GetClasses()
        {
            var vq = new VistaQuery("ORWTPN GETCLASS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get titles for a given document class (long list, supports scrolling).
        /// RPC: TIU LONG LIST OF TITLES
        /// </summary>
        [HttpGet, Route("api/options/titlesforclass")]
        public async Task<ActionResult<List<string>>> GetTitlesForClass(
            [FromQuery] int classValue,
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("TIU LONG LIST OF TITLES");
            vq.addParameter(VistaQuery.LITERAL, classValue.ToString());
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get titles assigned to a given user/class.
        /// RPC: ORWTPP GETTU
        /// </summary>
        [HttpGet, Route("api/options/titlesforuser")]
        public async Task<ActionResult<List<string>>> GetTitlesForUser([FromQuery] int classValue)
        {
            var vq = new VistaQuery("ORWTPP GETTU");
            vq.addParameter(VistaQuery.LITERAL, classValue.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the default title for a given class.
        /// RPC: ORWTPP GETTD
        /// </summary>
        [HttpGet, Route("api/options/titledefault")]
        public async Task<ActionResult<string>> GetTitleDefault([FromQuery] int classValue)
        {
            var vq = new VistaQuery("ORWTPP GETTD");
            vq.addParameter(VistaQuery.LITERAL, classValue.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save document default titles for a given class.
        /// RPC: ORWTPP SAVET
        /// </summary>
        [HttpPost, Route("api/options/documentdefaults")]
        public async Task<ActionResult<string>> SaveDocumentDefaults(
            [FromQuery] int classValue,
            [FromQuery] int titleDefault,
            [FromBody] List<string> items)
        {
            var vq = new VistaQuery("ORWTPP SAVET");
            vq.addParameter(VistaQuery.LITERAL, classValue.ToString());
            vq.addParameter(VistaQuery.LITERAL, titleDefault.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < items.Count; i++)
                dhl.Add(i.ToString(), items[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Lab Days

        /// <summary>
        /// Get system-level lab date-range defaults (inpatient/outpatient days).
        /// RPC: ORWTPO CSLABD
        /// </summary>
        [HttpGet, Route("api/options/labdays")]
        public async Task<ActionResult<string>> GetLabDays()
        {
            var vq = new VistaQuery("ORWTPO CSLABD");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get user-level lab date-range settings (inpatient/outpatient days).
        /// RPC: ORWTPP CSLAB
        /// </summary>
        [HttpGet, Route("api/options/labdays/user")]
        public async Task<ActionResult<string>> GetLabUserDays()
        {
            var vq = new VistaQuery("ORWTPP CSLAB");
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Appointment Days

        /// <summary>
        /// Get system-level appointment date-range defaults.
        /// RPC: ORWTPD1 GETCSDEF
        /// </summary>
        [HttpGet, Route("api/options/apptdays")]
        public async Task<ActionResult<string>> GetApptDays()
        {
            var vq = new VistaQuery("ORWTPD1 GETCSDEF");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get user-level appointment date-range settings.
        /// RPC: ORWTPD1 GETCSRNG
        /// </summary>
        [HttpGet, Route("api/options/apptdays/user")]
        public async Task<ActionResult<string>> GetApptUserDays()
        {
            var vq = new VistaQuery("ORWTPD1 GETCSRNG");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save lab and appointment day settings (inpatientDays^outpatientDays^startDays^stopDays).
        /// RPC: ORWTPD1 PUTCSRNG
        /// </summary>
        [HttpPost, Route("api/options/days")]
        public async Task<ActionResult<string>> SetDays([FromQuery] string values)
        {
            var vq = new VistaQuery("ORWTPD1 PUTCSRNG");
            vq.addParameter(VistaQuery.LITERAL, values);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Imaging Days

        /// <summary>
        /// Get system-level imaging date-range defaults.
        /// RPC: ORWTPO GETIMGD
        /// </summary>
        [HttpGet, Route("api/options/imagingdays")]
        public async Task<ActionResult<string>> GetImagingDays()
        {
            var vq = new VistaQuery("ORWTPO GETIMGD");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get user-level imaging date-range settings.
        /// RPC: ORWTPP GETIMG
        /// </summary>
        [HttpGet, Route("api/options/imagingdays/user")]
        public async Task<ActionResult<string>> GetImagingUserDays()
        {
            var vq = new VistaQuery("ORWTPP GETIMG");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save user-level imaging settings (maxNum, startDays, stopDays).
        /// RPC: ORWTPP SETIMG
        /// </summary>
        [HttpPost, Route("api/options/imagingdays")]
        public async Task<ActionResult<string>> SetImagingDays(
            [FromQuery] int maxNum,
            [FromQuery] int startDays,
            [FromQuery] int stopDays)
        {
            var vq = new VistaQuery("ORWTPP SETIMG");
            vq.addParameter(VistaQuery.LITERAL, maxNum.ToString());
            vq.addParameter(VistaQuery.LITERAL, startDays.ToString());
            vq.addParameter(VistaQuery.LITERAL, stopDays.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Reminders

        /// <summary>
        /// Get the user's reminder list preferences.
        /// RPC: ORWTPP GETREM
        /// </summary>
        [HttpGet, Route("api/options/reminders")]
        public async Task<ActionResult<List<string>>> GetReminders()
        {
            var vq = new VistaQuery("ORWTPP GETREM");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save the user's reminder list preferences.
        /// RPC: ORWTPP SETREM
        /// </summary>
        [HttpPost, Route("api/options/reminders")]
        public async Task<ActionResult<string>> SetReminders([FromBody] List<string> items)
        {
            var vq = new VistaQuery("ORWTPP SETREM");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < items.Count; i++)
                dhl.Add(i.ToString(), items[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Clinic Defaults

        /// <summary>
        /// Get user-level clinic date-range settings.
        /// RPC: ORWTPP CLRANGE
        /// </summary>
        [HttpGet, Route("api/options/clinicrange")]
        public async Task<ActionResult<string>> GetClinicUserDays()
        {
            var vq = new VistaQuery("ORWTPP CLRANGE");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get clinic day-of-week defaults (mon–sun flags).
        /// RPC: ORWTPP CLDAYS
        /// </summary>
        [HttpGet, Route("api/options/clinicdays")]
        public async Task<ActionResult<string>> GetClinicDefaults()
        {
            var vq = new VistaQuery("ORWTPP CLDAYS");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save clinic defaults (startDays^stopDays^mon^tue^wed^thu^fri^sat^sun).
        /// RPC: ORWTPP SAVECD
        /// </summary>
        [HttpPost, Route("api/options/clinicdefaults")]
        public async Task<ActionResult<string>> SaveClinicDefaults([FromQuery] string values)
        {
            var vq = new VistaQuery("ORWTPP SAVECD");
            vq.addParameter(VistaQuery.LITERAL, values);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get patient list sort order default.
        /// RPC: ORWTPP SORTDEF
        /// </summary>
        [HttpGet, Route("api/options/listorder")]
        public async Task<ActionResult<string>> GetListOrder()
        {
            var vq = new VistaQuery("ORWTPP SORTDEF");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get list source defaults (provider, treating, list, ward, pcmm).
        /// RPC: ORWTPP LSDEF
        /// </summary>
        [HttpGet, Route("api/options/listsourcedefaults")]
        public async Task<ActionResult<string>> GetListSourceDefaults()
        {
            var vq = new VistaQuery("ORWTPP LSDEF");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save patient-list defaults (plSource^sort^prov^spec^team^ward^pcmm).
        /// RPC: ORWTPP SAVEPLD
        /// </summary>
        [HttpPost, Route("api/options/ptlistdefaults")]
        public async Task<ActionResult<string>> SavePtListDefaults([FromQuery] string values)
        {
            var vq = new VistaQuery("ORWTPP SAVEPLD");
            vq.addParameter(VistaQuery.LITERAL, values);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Patient Lists & Teams

        /// <summary>
        /// Get the user's personal patient lists.
        /// RPC: ORWPT PERSONAL LISTS
        /// </summary>
        [HttpGet, Route("api/options/personallists")]
        public async Task<ActionResult<List<string>>> GetPersonalLists()
        {
            var vq = new VistaQuery("ORWPT PERSONAL LISTS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get all teams (for patient-list building).
        /// RPC: ORQPT TEAMS
        /// </summary>
        [HttpGet, Route("api/options/allteams")]
        public async Task<ActionResult<List<string>>> GetAllTeams()
        {
            var vq = new VistaQuery("ORQPT TEAMS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the user's assigned teams.
        /// RPC: ORWTPP TEAMS
        /// </summary>
        [HttpGet, Route("api/options/teams")]
        public async Task<ActionResult<List<string>>> GetTeams()
        {
            var vq = new VistaQuery("ORWTPP TEAMS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get auto-subscribe teams.
        /// RPC: ORWTPT ATEAMS
        /// </summary>
        [HttpGet, Route("api/options/ateams")]
        public async Task<ActionResult<List<string>>> GetATeams()
        {
            var vq = new VistaQuery("ORWTPT ATEAMS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get PCMM teams.
        /// RPC: ORWTPP PCMTEAMS
        /// </summary>
        [HttpGet, Route("api/options/pcmmteams")]
        public async Task<ActionResult<List<string>>> GetPcmmTeams()
        {
            var vq = new VistaQuery("ORWTPP PCMTEAMS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Delete a personal patient list.
        /// RPC: ORWTPP DELLIST
        /// </summary>
        [HttpPost, Route("api/options/lists/delete")]
        public async Task<ActionResult<string>> DeleteList([FromQuery] string listName)
        {
            var vq = new VistaQuery("ORWTPP DELLIST");
            vq.addParameter(VistaQuery.LITERAL, listName);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Create a new personal patient list. Returns the new IEN^name.
        /// RPC: ORWTPP NEWLIST
        /// </summary>
        [HttpPost, Route("api/options/lists/new")]
        public async Task<ActionResult<string>> NewList(
            [FromQuery] string listName,
            [FromQuery] int visibility)
        {
            var vq = new VistaQuery("ORWTPP NEWLIST");
            vq.addParameter(VistaQuery.LITERAL, listName);
            vq.addParameter(VistaQuery.LITERAL, visibility.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save changes to a patient list (add/remove patients).
        /// RPC: ORWTPP SAVELIST
        /// </summary>
        [HttpPost, Route("api/options/lists/save")]
        public async Task<ActionResult<string>> SaveListChanges(
            [FromQuery] int listIEN,
            [FromQuery] int listVisibility,
            [FromBody] List<string> items)
        {
            var vq = new VistaQuery("ORWTPP SAVELIST");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < items.Count; i++)
                dhl.Add(i.ToString(), items[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, listIEN.ToString());
            vq.addParameter(VistaQuery.LITERAL, listVisibility.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// List users belonging to a team.
        /// RPC: ORWTPT GETTEAM
        /// </summary>
        [HttpGet, Route("api/options/teams/users")]
        public async Task<ActionResult<List<string>>> ListUsersByTeam([FromQuery] int teamId)
        {
            var vq = new VistaQuery("ORWTPT GETTEAM");
            vq.addParameter(VistaQuery.LITERAL, teamId.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List users belonging to a PCMM team.
        /// RPC: ORWTPT GETPTEAM
        /// </summary>
        [HttpGet, Route("api/options/pcmmteams/users")]
        public async Task<ActionResult<List<string>>> ListUsersByPcmmTeam([FromQuery] int teamId)
        {
            var vq = new VistaQuery("ORWTPT GETPTEAM");
            vq.addParameter(VistaQuery.LITERAL, teamId.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Remove a list from the user's subscribed lists.
        /// RPC: ORWTPP REMLIST
        /// </summary>
        [HttpPost, Route("api/options/lists/remove")]
        public async Task<ActionResult<string>> RemoveList([FromQuery] int listIEN)
        {
            var vq = new VistaQuery("ORWTPP REMLIST");
            vq.addParameter(VistaQuery.LITERAL, listIEN.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Add a list to the user's subscribed lists.
        /// RPC: ORWTPP ADDLIST
        /// </summary>
        [HttpPost, Route("api/options/lists/add")]
        public async Task<ActionResult<string>> AddList([FromQuery] int listIEN)
        {
            var vq = new VistaQuery("ORWTPP ADDLIST");
            vq.addParameter(VistaQuery.LITERAL, listIEN.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Combos

        /// <summary>
        /// Get the user's combination-action settings.
        /// RPC: ORWTPP GETCOMBO
        /// </summary>
        [HttpGet, Route("api/options/combos")]
        public async Task<ActionResult<List<string>>> GetCombo()
        {
            var vq = new VistaQuery("ORWTPP GETCOMBO");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save the user's combination-action settings.
        /// RPC: ORWTPP SETCOMBO
        /// </summary>
        [HttpPost, Route("api/options/combos")]
        public async Task<ActionResult<string>> SetCombo([FromBody] List<string> items)
        {
            var vq = new VistaQuery("ORWTPP SETCOMBO");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < items.Count; i++)
                dhl.Add(i.ToString(), items[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Report Defaults

        /// <summary>
        /// Get the user's default report date-range settings.
        /// RPC: ORWTPD GETDFLT
        /// </summary>
        [HttpGet, Route("api/options/reportdefaults")]
        public async Task<ActionResult<string>> GetDefaultReportsSetting()
        {
            var vq = new VistaQuery("ORWTPD GETDFLT");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Delete the user-level report settings (revert to system default).
        /// RPC: ORWTPD DELDFLT
        /// </summary>
        [HttpPost, Route("api/options/reportdefaults/delete")]
        public async Task<ActionResult<string>> DeleteUserLevelReportsSetting()
        {
            var vq = new VistaQuery("ORWTPD DELDFLT");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Activate the default report setting.
        /// RPC: ORWTPD ACTDF
        /// </summary>
        [HttpPost, Route("api/options/reportdefaults/activate")]
        public async Task<ActionResult<string>> ActivateDefaultSetting()
        {
            var vq = new VistaQuery("ORWTPD ACTDF");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Set the default report date-range.
        /// RPC: ORWTPD SUDF
        /// </summary>
        [HttpPost, Route("api/options/reportdefaults")]
        public async Task<ActionResult<string>> SetDefaultReportsSetting([FromQuery] string value)
        {
            var vq = new VistaQuery("ORWTPD SUDF");
            vq.addParameter(VistaQuery.LITERAL, value);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Set an individual report's date-range setting.
        /// RPC: ORWTPD SUINDV
        /// </summary>
        [HttpPost, Route("api/options/reportdefaults/individual")]
        public async Task<ActionResult<string>> SetIndividualReportSetting(
            [FromQuery] string value1,
            [FromQuery] string value2)
        {
            var vq = new VistaQuery("ORWTPD SUINDV");
            vq.addParameter(VistaQuery.LITERAL, value1);
            vq.addParameter(VistaQuery.LITERAL, value2);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Retrieve the active default report setting. Returns semicolon-delimited
        /// offset values; empty means "NODEFAULT".
        /// RPC: ORWTPD RSDFLT
        /// </summary>
        [HttpGet, Route("api/options/reportdefaults/retrieve")]
        public async Task<ActionResult<string>> RetrieveDefaultSetting()
        {
            var vq = new VistaQuery("ORWTPD RSDFLT");
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Med Ranges

        /// <summary>
        /// Get outpatient med date-range (all).
        /// RPC: ORWTPD GETOCM
        /// </summary>
        [HttpGet, Route("api/options/medrange")]
        public async Task<ActionResult<string>> GetRangeForMeds()
        {
            var vq = new VistaQuery("ORWTPD GETOCM");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save outpatient med date-range (all).
        /// RPC: ORWTPD PUTOCM
        /// </summary>
        [HttpPost, Route("api/options/medrange")]
        public async Task<ActionResult<string>> PutRangeForMeds([FromQuery] string value)
        {
            var vq = new VistaQuery("ORWTPD PUTOCM");
            vq.addParameter(VistaQuery.LITERAL, value);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get inpatient med date-range.
        /// RPC: ORWTPD GETOCMIN
        /// </summary>
        [HttpGet, Route("api/options/medrange/inpatient")]
        public async Task<ActionResult<string>> GetRangeForMedsIn()
        {
            var vq = new VistaQuery("ORWTPD GETOCMIN");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save inpatient med date-range.
        /// RPC: ORWTPD PUTOCMIN
        /// </summary>
        [HttpPost, Route("api/options/medrange/inpatient")]
        public async Task<ActionResult<string>> PutRangeForMedsIn([FromQuery] string value)
        {
            var vq = new VistaQuery("ORWTPD PUTOCMIN");
            vq.addParameter(VistaQuery.LITERAL, value);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get outpatient (OP) med date-range.
        /// RPC: ORWTPD GETOCMOP
        /// </summary>
        [HttpGet, Route("api/options/medrange/outpatient")]
        public async Task<ActionResult<string>> GetRangeForMedsOp()
        {
            var vq = new VistaQuery("ORWTPD GETOCMOP");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save outpatient (OP) med date-range.
        /// RPC: ORWTPD PUTOCMOP
        /// </summary>
        [HttpPost, Route("api/options/medrange/outpatient")]
        public async Task<ActionResult<string>> PutRangeForMedsOp([FromQuery] string value)
        {
            var vq = new VistaQuery("ORWTPD PUTOCMOP");
            vq.addParameter(VistaQuery.LITERAL, value);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Encounter Ranges

        /// <summary>
        /// Get system-level encounter date-range defaults.
        /// RPC: ORWTPD1 GETEFDAT
        /// </summary>
        [HttpGet, Route("api/options/encounterrange/defaults")]
        public async Task<ActionResult<string>> GetRangeForEncsDefault()
        {
            var vq = new VistaQuery("ORWTPD1 GETEFDAT");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get user-level encounter date-range settings.
        /// RPC: ORWTPD1 GETEDATS
        /// </summary>
        [HttpGet, Route("api/options/encounterrange")]
        public async Task<ActionResult<string>> GetRangeForEncsUser()
        {
            var vq = new VistaQuery("ORWTPD1 GETEDATS");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save user-level encounter date-range (startDays^stopDays).
        /// RPC: ORWTPD1 PUTEDATS
        /// </summary>
        [HttpPost, Route("api/options/encounterrange")]
        public async Task<ActionResult<string>> PutRangeForEncs([FromQuery] string values)
        {
            var vq = new VistaQuery("ORWTPD1 PUTEDATS");
            vq.addParameter(VistaQuery.LITERAL, values);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the encounter-form future days limit.
        /// RPC: ORWTPD1 GETEAFL
        /// </summary>
        [HttpGet, Route("api/options/encounterrange/futuredays")]
        public async Task<ActionResult<string>> GetEncFutureDays()
        {
            var vq = new VistaQuery("ORWTPD1 GETEAFL");
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Other / Tabs / Copy-Paste

        /// <summary>
        /// Get the user's "other" options string.
        /// RPC: ORWTPP GETOTHER
        /// </summary>
        [HttpGet, Route("api/options/other")]
        public async Task<ActionResult<string>> GetOther()
        {
            var vq = new VistaQuery("ORWTPP GETOTHER");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Set the user's "other" options string.
        /// RPC: ORWTPP SETOTHER
        /// </summary>
        [HttpPost, Route("api/options/other")]
        public async Task<ActionResult<string>> SetOther([FromQuery] string info)
        {
            var vq = new VistaQuery("ORWTPP SETOTHER");
            vq.addParameter(VistaQuery.LITERAL, info);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get tab-display preferences.
        /// RPC: ORWTPO GETTABS
        /// </summary>
        [HttpGet, Route("api/options/tabs")]
        public async Task<ActionResult<List<string>>> GetOtherTabs()
        {
            var vq = new VistaQuery("ORWTPO GETTABS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load copy-paste identification settings.
        /// RPC: ORWTIU LDCPIDNT
        /// </summary>
        [HttpGet, Route("api/options/copypaste")]
        public async Task<ActionResult<string>> GetCopyPaste()
        {
            var vq = new VistaQuery("ORWTIU LDCPIDNT");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save copy-paste identification settings.
        /// RPC: ORWTIU SVCPIDNT
        /// </summary>
        [HttpPost, Route("api/options/copypaste")]
        public async Task<ActionResult<string>> SaveCopyPaste([FromQuery] string value)
        {
            var vq = new VistaQuery("ORWTIU SVCPIDNT");
            vq.addParameter(VistaQuery.LITERAL, value);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Web.CPRS Convenience Endpoints

        /// <summary>
        /// Create a new personal patient list (convenience alias).
        /// RPC: ORWPT SAVE LIST
        /// </summary>
        [HttpPost, Route("api/options/newlist")]
        public async Task<ActionResult<string>> NewListAlias([FromQuery] string name = "")
        {
            var vq = new VistaQuery("ORWPT SAVE LIST");
            vq.addParameter(VistaQuery.LITERAL, name);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Delete a personal patient list (convenience alias).
        /// RPC: ORWPT DELETE LIST
        /// </summary>
        [HttpPost, Route("api/options/deletelist")]
        public async Task<ActionResult<string>> DeleteListAlias([FromQuery] string ien = "")
        {
            var vq = new VistaQuery("ORWPT DELETE LIST");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
