// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Alert/notification management — load, delete, forward, renew, sort, follow-up.
    /// Migrated from cprs/rCore.pas — ORWORB *, ORB *, ORBSMART *, TIU GET ALERT INFO RPCs.
    /// </summary>
    public class AlertController : BaseController
    {
        public AlertController() : base() { }

        #region Load & Query Alerts

        /// <summary>
        /// Load all notifications/alerts for the current user.
        /// RPC: ORWORB FASTUSER
        /// Returns: list of alert strings
        /// </summary>
        [HttpGet, Route("api/alert/list")]
        public async Task<ActionResult<List<string>>> List()
        {
            var vq = new VistaQuery("ORWORB FASTUSER");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get long text for a notification.
        /// RPC: ORWORB GETLTXT
        /// Returns: list of text lines
        /// </summary>
        [HttpGet, Route("api/alert/longtext")]
        public async Task<ActionResult<List<string>>> LongText([FromQuery] string alertId)
        {
            var vq = new VistaQuery("ORWORB GETLTXT");
            vq.addParameter(VistaQuery.LITERAL, alertId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get data associated with an alert (XQAID).
        /// RPC: ORWORB GETDATA
        /// </summary>
        [HttpGet, Route("api/alert/data")]
        public async Task<ActionResult<string>> Data(
            [FromQuery] string xqaid,
            [FromQuery] string flag = "")
        {
            var vq = new VistaQuery("ORWORB GETDATA");
            vq.addParameter(VistaQuery.LITERAL, xqaid);
            vq.addParameter(VistaQuery.LITERAL, flag);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get follow-up text for a notification.
        /// RPC: ORWORB TEXT FOLLOWUP
        /// Returns: list of follow-up text lines
        /// </summary>
        [HttpGet, Route("api/alert/followuptext")]
        public async Task<ActionResult<List<string>>> FollowUpText(
            [FromQuery] string patientDfn,
            [FromQuery] int notification,
            [FromQuery] string xqadata)
        {
            var vq = new VistaQuery("ORWORB TEXT FOLLOWUP");
            vq.addParameter(VistaQuery.LITERAL, patientDfn);
            vq.addParameter(VistaQuery.LITERAL, notification.ToString());
            vq.addParameter(VistaQuery.LITERAL, xqadata);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get TIU alert info (DFN and document type).
        /// RPC: TIU GET ALERT INFO
        /// </summary>
        [HttpGet, Route("api/alert/tiualertinfo")]
        public async Task<ActionResult<string>> TiuAlertInfo([FromQuery] string xqaid)
        {
            var vq = new VistaQuery("TIU GET ALERT INFO");
            vq.addParameter(VistaQuery.LITERAL, xqaid);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a notification is a smart alert.
        /// RPC: ORBSMART ISSMNOT
        /// Returns: '1' if smart alert
        /// </summary>
        [HttpGet, Route("api/alert/issmartalert")]
        public async Task<ActionResult<string>> IsSmartAlert([FromQuery] int notIen)
        {
            var vq = new VistaQuery("ORBSMART ISSMNOT");
            vq.addParameter(VistaQuery.LITERAL, notIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get unsigned order alert follow-up data.
        /// RPC: ORWORB UNSIG ORDERS FOLLOWUP
        /// </summary>
        [HttpGet, Route("api/alert/unsignedordersfollowup")]
        public async Task<ActionResult<string>> UnsignedOrdersFollowUp([FromQuery] string xqaid)
        {
            var vq = new VistaQuery("ORWORB UNSIG ORDERS FOLLOWUP");
            vq.addParameter(VistaQuery.LITERAL, xqaid);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Alert Actions

        /// <summary>
        /// Delete an alert.
        /// RPC: ORB DELETE ALERT
        /// </summary>
        [HttpPost, Route("api/alert/delete")]
        public async Task<ActionResult<string>> Delete([FromQuery] string xqaid)
        {
            var vq = new VistaQuery("ORB DELETE ALERT");
            vq.addParameter(VistaQuery.LITERAL, xqaid);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Delete an alert for a specific user.
        /// RPC: ORB DELETE ALERT (with forUser=true)
        /// </summary>
        [HttpPost, Route("api/alert/deleteforuser")]
        public async Task<ActionResult<string>> DeleteForUser([FromQuery] string xqaid)
        {
            var vq = new VistaQuery("ORB DELETE ALERT");
            vq.addParameter(VistaQuery.LITERAL, xqaid);
            vq.addParameter(VistaQuery.LITERAL, "1");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Forward an alert to a recipient with a comment.
        /// RPC: ORB FORWARD ALERT
        /// </summary>
        [HttpPost, Route("api/alert/forward")]
        public async Task<ActionResult<string>> Forward(
            [FromQuery] string xqaid,
            [FromQuery] string recipient,
            [FromQuery] string forwardType,
            [FromQuery] string comment)
        {
            var vq = new VistaQuery("ORB FORWARD ALERT");
            vq.addParameter(VistaQuery.LITERAL, xqaid);
            vq.addParameter(VistaQuery.LITERAL, recipient);
            vq.addParameter(VistaQuery.LITERAL, forwardType);
            vq.addParameter(VistaQuery.LITERAL, comment);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Renew/restore an alert.
        /// RPC: ORB RENEW ALERT
        /// </summary>
        [HttpPost, Route("api/alert/renew")]
        public async Task<ActionResult<string>> Renew([FromQuery] string xqaid)
        {
            var vq = new VistaQuery("ORB RENEW ALERT");
            vq.addParameter(VistaQuery.LITERAL, xqaid);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Alert Sorting

        /// <summary>
        /// Get the current alert sort method.
        /// RPC: ORWORB GETSORT
        /// </summary>
        [HttpGet, Route("api/alert/sortmethod")]
        public async Task<ActionResult<string>> GetSortMethod()
        {
            var vq = new VistaQuery("ORWORB GETSORT");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Set the alert sort method for the current user.
        /// RPC: ORWORB SETSORT
        /// </summary>
        [HttpPost, Route("api/alert/setsortmethod")]
        public async Task<ActionResult<string>> SetSortMethod(
            [FromQuery] string sort,
            [FromQuery] string direction)
        {
            var vq = new VistaQuery("ORWORB SETSORT");
            vq.addParameter(VistaQuery.LITERAL, sort);
            vq.addParameter(VistaQuery.LITERAL, direction);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Alert Cleanup (Kill Alerts)

        /// <summary>
        /// Clear unsigned orders alert for a patient.
        /// RPC: ORWORB KILL UNSIG ORDERS ALERT
        /// </summary>
        [HttpPost, Route("api/alert/killunsignedorders")]
        public async Task<ActionResult<string>> KillUnsignedOrdersAlert([FromQuery] string patientDfn)
        {
            var vq = new VistaQuery("ORWORB KILL UNSIG ORDERS ALERT");
            vq.addParameter(VistaQuery.LITERAL, patientDfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Clear expiring medication alert for a patient.
        /// RPC: ORWORB KILL EXPIR MED ALERT
        /// </summary>
        [HttpPost, Route("api/alert/killexpiringmeds")]
        public async Task<ActionResult<string>> KillExpiringMedsAlert([FromQuery] string patientDfn)
        {
            var vq = new VistaQuery("ORWORB KILL EXPIR MED ALERT");
            vq.addParameter(VistaQuery.LITERAL, patientDfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Clear expiring flagged orderable item alert for a patient.
        /// RPC: ORWORB KILL EXPIR OI ALERT
        /// </summary>
        [HttpPost, Route("api/alert/killexpiringoi")]
        public async Task<ActionResult<string>> KillExpiringOiAlert(
            [FromQuery] string patientDfn,
            [FromQuery] int followUp)
        {
            var vq = new VistaQuery("ORWORB KILL EXPIR OI ALERT");
            vq.addParameter(VistaQuery.LITERAL, patientDfn);
            vq.addParameter(VistaQuery.LITERAL, followUp.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Clear unverified medications alert for a patient.
        /// RPC: ORWORB KILL UNVER MEDS ALERT
        /// </summary>
        [HttpPost, Route("api/alert/killunverifiedmeds")]
        public async Task<ActionResult<string>> KillUnverifiedMedsAlert([FromQuery] string patientDfn)
        {
            var vq = new VistaQuery("ORWORB KILL UNVER MEDS ALERT");
            vq.addParameter(VistaQuery.LITERAL, patientDfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Clear unverified orders alert for a patient.
        /// RPC: ORWORB KILL UNVER ORDERS ALERT
        /// </summary>
        [HttpPost, Route("api/alert/killunverifiedorders")]
        public async Task<ActionResult<string>> KillUnverifiedOrdersAlert([FromQuery] string patientDfn)
        {
            var vq = new VistaQuery("ORWORB KILL UNVER ORDERS ALERT");
            vq.addParameter(VistaQuery.LITERAL, patientDfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Auto-unflag alerted orders for a patient.
        /// RPC: ORWORB AUTOUNFLAG ORDERS
        /// </summary>
        [HttpPost, Route("api/alert/autounflagorders")]
        public async Task<ActionResult<string>> AutoUnflagOrders(
            [FromQuery] string patientDfn,
            [FromQuery] string xqaid)
        {
            var vq = new VistaQuery("ORWORB AUTOUNFLAG ORDERS");
            vq.addParameter(VistaQuery.LITERAL, patientDfn);
            vq.addParameter(VistaQuery.LITERAL, xqaid);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Notification Alias Endpoints

        /// <summary>
        /// Alias for alert/list — used by Web.CPRS NotificationsController.
        /// RPC: ORWORB FASTUSER
        /// </summary>
        [HttpGet, Route("api/notification/list")]
        public async Task<ActionResult<List<string>>> NotificationList()
        {
            var vq = new VistaQuery("ORWORB FASTUSER");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get unsorted notification count.
        /// RPC: ORWORB UNSORTED
        /// </summary>
        [HttpGet, Route("api/notification/count")]
        public async Task<ActionResult<string>> NotificationCount()
        {
            var vq = new VistaQuery("ORWORB UNSORTED");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Delete a notification by alertId.
        /// RPC: ORB DELETE ALERT
        /// </summary>
        [HttpPost, Route("api/notification/delete")]
        public async Task<ActionResult<string>> NotificationDelete([FromQuery] string alertId)
        {
            var vq = new VistaQuery("ORB DELETE ALERT");
            vq.addParameter(VistaQuery.LITERAL, alertId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Forward a notification to another user.
        /// RPC: ORB FORWARD ALERT
        /// </summary>
        [HttpPost, Route("api/notification/forward")]
        public async Task<ActionResult<string>> NotificationForward(
            [FromQuery] string alertId,
            [FromQuery] string toUser = "",
            [FromQuery] string comment = "")
        {
            var vq = new VistaQuery("ORB FORWARD ALERT");
            vq.addParameter(VistaQuery.LITERAL, alertId);
            vq.addParameter(VistaQuery.LITERAL, toUser);
            vq.addParameter(VistaQuery.LITERAL, "");
            vq.addParameter(VistaQuery.LITERAL, comment);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Defer a notification until a later time.
        /// RPC: ORB DEFER ALERT
        /// </summary>
        [HttpPost, Route("api/notification/defer")]
        public async Task<ActionResult<string>> NotificationDefer(
            [FromQuery] string alertId,
            [FromQuery] string deferUntil = "")
        {
            var vq = new VistaQuery("ORB DEFER ALERT");
            vq.addParameter(VistaQuery.LITERAL, alertId);
            vq.addParameter(VistaQuery.LITERAL, deferUntil);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region ORB3UTL — Smart Notification Helpers

        /// <summary>
        /// Get existing notes for a given note title and patient (for alert-to-note linking).
        /// RPC: ORB3UTL GET EXISTING NOTES
        /// Returns: list of IEN^Title^Date per line
        /// Maps to: fNotificationProcessor.pas → CallVistA('ORB3UTL GET EXISTING NOTES', [fNoteTitleIEN, fDFN], fNoteList)
        /// </summary>
        [HttpGet, Route("api/alert/existingnotes")]
        public async Task<ActionResult<List<string>>> GetExistingNotes(
            [FromQuery] int noteTitleIen,
            [FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORB3UTL GET EXISTING NOTES");
            vq.addParameter(VistaQuery.LITERAL, noteTitleIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get notification description text.
        /// RPC: ORB3UTL GET DESCRIPTION
        /// </summary>
        [HttpGet, Route("api/alert/description")]
        public async Task<ActionResult<List<string>>> GetDescription([FromQuery] string alertId)
        {
            var vq = new VistaQuery("ORB3UTL GET DESCRIPTION");
            vq.addParameter(VistaQuery.LITERAL, alertId);
            return await this.Session.tQuery(vq);
        }

        #endregion
    }
}
