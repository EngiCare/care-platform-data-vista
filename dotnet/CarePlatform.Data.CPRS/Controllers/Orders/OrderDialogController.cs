// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Order dialogs, menus, order sets, quick orders, prompts, order checks,
    /// order items, write-order lists.
    /// Migrated from cprs/Orders/rODBase.pas + rOrders.pas —
    /// ORWDX, ORWDXM, ORWDXQ, ORWDXC, ORWDOR RPCs.
    /// </summary>
    public class OrderDialogController : BaseController
    {
        public OrderDialogController() : base() { }

        #region Dialog Definitions

        /// <summary>
        /// Load a dialog definition.
        /// RPC: ORWDX DLGDEF
        /// </summary>
        [HttpGet, Route("api/orderdialog/definition")]
        public async Task<ActionResult<List<string>>> Definition([FromQuery] string dialogName)
        {
            var vq = new VistaQuery("ORWDX DLGDEF");
            vq.addParameter(VistaQuery.LITERAL, dialogName);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load responses for an existing order.
        /// RPC: ORWDX LOADRSP
        /// </summary>
        [HttpGet, Route("api/orderdialog/loadresponses")]
        public async Task<ActionResult<List<string>>> LoadResponses(
            [FromQuery] string orderId,
            [FromQuery] bool transfer = false)
        {
            var vq = new VistaQuery("ORWDX LOADRSP");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, transfer ? "1" : "0");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Build responses for a quick order or order set item.
        /// RPC: ORWDXM1 BLDQRSP
        /// </summary>
        [HttpGet, Route("api/orderdialog/buildresponses")]
        public async Task<ActionResult<List<string>>> BuildResponses(
            [FromQuery] string inputId,
            [FromQuery] string extra = "",
            [FromQuery] bool forIMO = false,
            [FromQuery] long location = 0)
        {
            var vq = new VistaQuery("ORWDXM1 BLDQRSP");
            vq.addParameter(VistaQuery.LITERAL, inputId);
            vq.addParameter(VistaQuery.LITERAL, extra);
            vq.addParameter(VistaQuery.LITERAL, forIMO ? "1" : "0");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the dialog ID (IEN) for an order.
        /// RPC: ORWDX DLGID
        /// </summary>
        [HttpGet, Route("api/orderdialog/dialogfororder")]
        public async Task<ActionResult<string>> DialogForOrder([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDX DLGID");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the form ID for an order.
        /// RPC: ORWDX FORMID
        /// </summary>
        [HttpGet, Route("api/orderdialog/formfororder")]
        public async Task<ActionResult<string>> FormForOrder([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDX FORMID");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the form ID for a dialog.
        /// RPC: ORWDXM FORMID
        /// </summary>
        [HttpGet, Route("api/orderdialog/formfordialog")]
        public async Task<ActionResult<string>> FormForDialog([FromQuery] int dialogIen)
        {
            var vq = new VistaQuery("ORWDXM FORMID");
            vq.addParameter(VistaQuery.LITERAL, dialogIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Identify a dialog name.
        /// RPC: ORWDXM DLGNAME
        /// </summary>
        [HttpGet, Route("api/orderdialog/identify")]
        public async Task<ActionResult<string>> Identify([FromQuery] string dialog)
        {
            var vq = new VistaQuery("ORWDXM DLGNAME");
            vq.addParameter(VistaQuery.LITERAL, dialog);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get disabled message for a dialog.
        /// RPC: ORWDX DISMSG
        /// </summary>
        [HttpGet, Route("api/orderdialog/disabledmessage")]
        public async Task<ActionResult<string>> DisabledMessage([FromQuery] int dlgIen)
        {
            var vq = new VistaQuery("ORWDX DISMSG");
            vq.addParameter(VistaQuery.LITERAL, dlgIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the display group name for a dialog.
        /// RPC: ORWDX DGNM
        /// </summary>
        [HttpGet, Route("api/orderdialog/dgroupname")]
        public async Task<ActionResult<string>> DisplayGroupByName([FromQuery] string name)
        {
            var vq = new VistaQuery("ORWDX DGNM");
            vq.addParameter(VistaQuery.LITERAL, name);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the display group for a dialog.
        /// RPC: ORWDX DGRP
        /// </summary>
        [HttpGet, Route("api/orderdialog/dgroupfordialog")]
        public async Task<ActionResult<string>> DisplayGroupForDialog([FromQuery] string dialogName)
        {
            var vq = new VistaQuery("ORWDX DGRP");
            vq.addParameter(VistaQuery.LITERAL, dialogName);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if another order should be asked for.
        /// RPC: ORWDX AGAIN
        /// </summary>
        [HttpGet, Route("api/orderdialog/askanother")]
        public async Task<ActionResult<string>> AskAnother([FromQuery] string dialog)
        {
            var vq = new VistaQuery("ORWDX AGAIN");
            vq.addParameter(VistaQuery.LITERAL, dialog);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get OI message for an orderable item.
        /// RPC: ORWDX MSG
        /// </summary>
        [HttpGet, Route("api/orderdialog/oimessage")]
        public async Task<ActionResult<List<string>>> OIMessage([FromQuery] int ien)
        {
            var vq = new VistaQuery("ORWDX MSG");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a subset of orderable items.
        /// RPC: ORWDX ORDITM
        /// </summary>
        [HttpGet, Route("api/orderdialog/orderitems")]
        public async Task<ActionResult<List<string>>> OrderItems(
            [FromQuery] string startFrom,
            [FromQuery] int direction,
            [FromQuery] string xref = "",
            [FromQuery] int quickOrderDlgIen = 0)
        {
            var vq = new VistaQuery("ORWDX ORDITM");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            vq.addParameter(VistaQuery.LITERAL, xref);
            vq.addParameter(VistaQuery.LITERAL, quickOrderDlgIen.ToString());
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Menus & Order Sets

        /// <summary>
        /// Load an order menu.
        /// RPC: ORWDXM MENU
        /// </summary>
        [HttpGet, Route("api/orderdialog/menu")]
        public async Task<ActionResult<List<string>>> Menu([FromQuery] int menuIen)
        {
            var vq = new VistaQuery("ORWDXM MENU");
            vq.addParameter(VistaQuery.LITERAL, menuIen.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load an order set.
        /// RPC: ORWDXM LOADSET
        /// </summary>
        [HttpGet, Route("api/orderdialog/orderset")]
        public async Task<ActionResult<List<string>>> OrderSet([FromQuery] int setIen)
        {
            var vq = new VistaQuery("ORWDXM LOADSET");
            vq.addParameter(VistaQuery.LITERAL, setIen.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load write-order list for a location.
        /// RPC: ORWDX WRLST
        /// </summary>
        [HttpGet, Route("api/orderdialog/writeorders")]
        public async Task<ActionResult<List<string>>> WriteOrders([FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDX WRLST");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the menu display style.
        /// RPC: ORWDXM MSTYLE
        /// </summary>
        [HttpGet, Route("api/orderdialog/menustyle")]
        public async Task<ActionResult<string>> MenuStyle()
        {
            var vq = new VistaQuery("ORWDXM MSTYLE");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Resolve a screen reference.
        /// RPC: ORWDXM RSCRN
        /// </summary>
        [HttpGet, Route("api/orderdialog/resolvescreen")]
        public async Task<ActionResult<string>> ResolveScreen([FromQuery] string reference)
        {
            var vq = new VistaQuery("ORWDXM RSCRN");
            vq.addParameter(VistaQuery.LITERAL, reference);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Prompting

        /// <summary>
        /// Load order prompting for a dialog.
        /// RPC: ORWDXM PROMPTS
        /// </summary>
        [HttpGet, Route("api/orderdialog/prompts")]
        public async Task<ActionResult<List<string>>> Prompts([FromQuery] string dialog)
        {
            var vq = new VistaQuery("ORWDXM PROMPTS");
            vq.addParameter(VistaQuery.LITERAL, dialog);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Auto-accept a quick order (no prompts).
        /// RPC: ORWDXM AUTOACK
        /// </summary>
        [HttpPost, Route("api/orderdialog/autoaccept")]
        public async Task<ActionResult<List<string>>> AutoAccept(
            [FromQuery] string dfn,
            [FromQuery] long provider,
            [FromQuery] long location,
            [FromQuery] string dialog)
        {
            var vq = new VistaQuery("ORWDXM AUTOACK");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, dialog);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Quick Orders

        /// <summary>
        /// Get quick order list for a display group.
        /// RPC: ORWDXQ GETQLST
        /// </summary>
        [HttpGet, Route("api/orderdialog/quicklist")]
        public async Task<ActionResult<List<string>>> QuickList(
            [FromQuery] string dGroup,
            [FromQuery] string listType = "Q")
        {
            var vq = new VistaQuery("ORWDXQ GETQLST");
            vq.addParameter(VistaQuery.LITERAL, dGroup);
            vq.addParameter(VistaQuery.LITERAL, listType);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a quick order name by CRC.
        /// RPC: ORWDXQ GETQNAM
        /// </summary>
        [HttpGet, Route("api/orderdialog/quickname")]
        public async Task<ActionResult<string>> QuickName([FromQuery] string crc)
        {
            var vq = new VistaQuery("ORWDXQ GETQNAM");
            vq.addParameter(VistaQuery.LITERAL, crc);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save a quick order list.
        /// RPC: ORWDXQ PUTQLST
        /// </summary>
        [HttpPost, Route("api/orderdialog/quicklist")]
        public async Task<ActionResult<List<string>>> SaveQuickList(
            [FromQuery] string dGroup,
            [FromBody] List<string> items)
        {
            var vq = new VistaQuery("ORWDXQ PUTQLST");
            vq.addParameter(VistaQuery.LITERAL, dGroup);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < items.Count; i++)
                dhl.Add(i.ToString(), items[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Order Checks

        /// <summary>
        /// Get filler ID for a dialog (used for order checking).
        /// RPC: ORWDXC FILLID
        /// </summary>
        [HttpGet, Route("api/orderdialog/fillerid")]
        public async Task<ActionResult<string>> FillerID([FromQuery] int dialogIen)
        {
            var vq = new VistaQuery("ORWDXC FILLID");
            vq.addParameter(VistaQuery.LITERAL, dialogIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if order checks are enabled.
        /// RPC: ORWDXC ON
        /// </summary>
        [HttpGet, Route("api/orderdialog/checksenabled")]
        public async Task<ActionResult<string>> ChecksEnabled()
        {
            var vq = new VistaQuery("ORWDXC ON");
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get order checks on display (before ordering).
        /// RPC: ORWDXC DISPLAY
        /// </summary>
        [HttpGet, Route("api/orderdialog/checksondisplay")]
        public async Task<ActionResult<List<string>>> ChecksOnDisplay(
            [FromQuery] string dfn,
            [FromQuery] string fillerId)
        {
            var vq = new VistaQuery("ORWDXC DISPLAY");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, fillerId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get order checks on accept.
        /// RPC: ORWDXC ACCEPT
        /// </summary>
        [HttpPost, Route("api/orderdialog/checksonaccept")]
        public async Task<ActionResult<List<string>>> ChecksOnAccept(
            [FromQuery] string dfn,
            [FromQuery] string fillerId,
            [FromQuery] string startDtTm,
            [FromQuery] long location,
            [FromQuery] string dupORIFN = "",
            [FromQuery] bool renewal = false,
            [FromBody] List<string> oiList = null)
        {
            var vq = new VistaQuery("ORWDXC ACCEPT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, fillerId);
            vq.addParameter(VistaQuery.LITERAL, startDtTm);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            var dhl = new DictionaryHashList();
            if (oiList != null)
                for (int i = 0; i < oiList.Count; i++)
                    dhl.Add(i.ToString(), oiList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, dupORIFN);
            vq.addParameter(VistaQuery.LITERAL, renewal ? "1" : "0");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get order checks for delayed/event orders.
        /// RPC: ORWDXC DELAY
        /// </summary>
        [HttpPost, Route("api/orderdialog/checksondelay")]
        public async Task<ActionResult<List<string>>> ChecksOnDelay(
            [FromQuery] string dfn,
            [FromQuery] string fillerId,
            [FromQuery] string startDtTm,
            [FromQuery] long location,
            [FromBody] List<string> oiList = null)
        {
            var vq = new VistaQuery("ORWDXC DELAY");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, fillerId);
            vq.addParameter(VistaQuery.LITERAL, startDtTm);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            var dhl = new DictionaryHashList();
            if (oiList != null)
                for (int i = 0; i < oiList.Count; i++)
                    dhl.Add(i.ToString(), oiList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get order checks for the whole session.
        /// RPC: ORWDXC SESSION
        /// </summary>
        [HttpPost, Route("api/orderdialog/sessionchecks")]
        public async Task<ActionResult<List<string>>> SessionChecks(
            [FromQuery] string dfn,
            [FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWDXC SESSION");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Delete a checked order.
        /// RPC: ORWDXC DELORD
        /// </summary>
        [HttpPost, Route("api/orderdialog/deletechecked")]
        public async Task<ActionResult<string>> DeleteChecked([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXC DELORD");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Lookups

        /// <summary>
        /// Subset of entries by global reference + screen.
        /// RPC: ORWDOR LKSCRN
        /// </summary>
        [HttpGet, Route("api/orderdialog/entries")]
        public async Task<ActionResult<List<string>>> Entries(
            [FromQuery] string startFrom,
            [FromQuery] int direction,
            [FromQuery] string xref = "",
            [FromQuery] string gblRef = "",
            [FromQuery] string screenRef = "")
        {
            var vq = new VistaQuery("ORWDOR LKSCRN");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            vq.addParameter(VistaQuery.LITERAL, xref);
            vq.addParameter(VistaQuery.LITERAL, gblRef);
            vq.addParameter(VistaQuery.LITERAL, screenRef);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Validate a numeric string for an order prompt.
        /// RPC: ORWDOR VALNUM
        /// </summary>
        [HttpGet, Route("api/orderdialog/validatenum")]
        public async Task<ActionResult<string>> ValidateNum(
            [FromQuery] string value,
            [FromQuery] string domain)
        {
            var vq = new VistaQuery("ORWDOR VALNUM");
            vq.addParameter(VistaQuery.LITERAL, value);
            vq.addParameter(VistaQuery.LITERAL, domain);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Clear order recall list.
        /// RPC: ORWDXM2 CLRRCL
        /// </summary>
        [HttpPost, Route("api/orderdialog/clearrecall")]
        public async Task<ActionResult<List<string>>> ClearRecall()
        {
            var vq = new VistaQuery("ORWDXM2 CLRRCL");
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if a QO is an inpatient quick order.
        /// RPC: ORWDXM3 ISUDQO
        /// </summary>
        [HttpGet, Route("api/orderdialog/isinpatientqo")]
        public async Task<ActionResult<string>> IsInpatientQO([FromQuery] string dlgId)
        {
            var vq = new VistaQuery("ORWDXM3 ISUDQO");
            vq.addParameter(VistaQuery.LITERAL, dlgId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a dialog is a PSO supply dialog.
        /// RPC: ORWDXR01 ISSPLY
        /// </summary>
        [HttpGet, Route("api/orderdialog/issupply")]
        public async Task<ActionResult<string>> IsSupply(
            [FromQuery] string dlgId,
            [FromQuery] string qoDlg)
        {
            var vq = new VistaQuery("ORWDXR01 ISSPLY");
            vq.addParameter(VistaQuery.LITERAL, dlgId);
            vq.addParameter(VistaQuery.LITERAL, qoDlg);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get DLG IEN from dialog name.
        /// RPC: OREVNTX1 DLGIEN
        /// </summary>
        [HttpGet, Route("api/orderdialog/dlgien")]
        public async Task<ActionResult<string>> DlgIEN([FromQuery] string dlgName)
        {
            var vq = new VistaQuery("OREVNTX1 DLGIEN");
            vq.addParameter(VistaQuery.LITERAL, dlgName);
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
