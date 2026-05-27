// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Medications — active lists, detail, admin history, refill, status changes.
    /// Migrated from cprs/rMeds.pas — ORWPS *, ORWPS1 *, ORWDXR, ORWDX1 RPCs.
    /// </summary>
    public class MedicationController : BaseController
    {
        public MedicationController() : base() { }

        /// <summary>
        /// Get active medications (inpatient + outpatient + non-VA).
        /// RPC: ORWPS ACTIVE
        /// Returns: first line = view^dateRange^dateRangeIp^dateRangeOp, then ~-prefixed medication blocks.
        /// </summary>
        [HttpGet, Route("api/medication/active")]
        public async Task<ActionResult<List<string>>> Active(
            [FromQuery] string dfn,
            [FromQuery] long duz,
            [FromQuery] int view = 0)
        {
            var vq = new VistaQuery("ORWPS ACTIVE");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, duz.ToString());
            vq.addParameter(VistaQuery.LITERAL, view.ToString());
            vq.addParameter(VistaQuery.LITERAL, "1"); // include instructions
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get detail for a single medication.
        /// RPC: ORWPS DETAIL
        /// </summary>
        [HttpGet, Route("api/medication/detail")]
        public async Task<ActionResult<List<string>>> Detail(
            [FromQuery] string dfn,
            [FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("id parameter is required.");
            var vq = new VistaQuery("ORWPS DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, id.ToUpper());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get medication administration history.
        /// RPC: ORWPS MEDHIST
        /// </summary>
        [HttpGet, Route("api/medication/adminhistory")]
        public async Task<ActionResult<List<string>>> AdminHistory(
            [FromQuery] string dfn,
            [FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWPS MEDHIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the new-medication dialog identifier based on patient inpatient status.
        /// RPC: ORWPS1 NEWDLG
        /// Returns: dialog IEN string
        /// </summary>
        [HttpGet, Route("api/medication/newdialog")]
        public async Task<ActionResult<string>> NewDialog([FromQuery] bool inpatient)
        {
            var vq = new VistaQuery("ORWPS1 NEWDLG");
            vq.addParameter(VistaQuery.LITERAL, inpatient ? "1" : "0");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get default pickup location for outpatient refills (C=clinic, W=window, M=mail).
        /// RPC: ORWPS1 PICKUP
        /// </summary>
        [HttpGet, Route("api/medication/pickupdefault")]
        public async Task<ActionResult<string>> PickupDefault()
        {
            var vq = new VistaQuery("ORWPS1 PICKUP");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Request a medication refill.
        /// RPC: ORWPS1 REFILL (matches legacy rMeds.pas → Refill)
        /// </summary>
        [HttpPost, Route("api/medication/refill")]
        public async Task<ActionResult<string>> Refill(
            [FromQuery] string orderId,
            [FromQuery] string pickUpAt,
            [FromQuery] string dfn,
            [FromQuery] long provider,
            [FromQuery] long location)
        {
            var vq = new VistaQuery("ORWPS1 REFILL");
            vq.addParameter(VistaQuery.LITERAL, orderId ?? "");
            vq.addParameter(VistaQuery.LITERAL, pickUpAt ?? "W");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if an order is a first-dose-now order.
        /// RPC: ORWDXR ISNOW
        /// Returns: '1' if first dose now, '0' otherwise
        /// </summary>
        [HttpGet, Route("api/medication/isfirstdosenow")]
        public async Task<ActionResult<string>> IsFirstDoseNow([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXR ISNOW");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check for medication status changes.
        /// RPC: ORWDX1 STCHANGE
        /// Returns: '1' if status changed
        /// </summary>
        [HttpPost, Route("api/medication/statuschangecheck")]
        public async Task<ActionResult<string>> StatusChangeCheck(
            [FromQuery] string dfn,
            [FromBody] List<string> medIds)
        {
            var vq = new VistaQuery("ORWDX1 STCHANGE");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < medIds.Count; i++)
                dhl.Add(i.ToString(), medIds[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get cover-sheet medication detail.
        /// RPC: ORWPS COVER
        /// </summary>
        [HttpGet, Route("api/medication/coverdetail")]
        public async Task<ActionResult<List<string>>> CoverDetail(
            [FromQuery] string dfn,
            [FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWPS COVER");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Search the drug file for ordering.
        /// RPC: ORWDX WRLST
        /// </summary>
        [HttpGet, Route("api/medication/drugsearch")]
        public async Task<ActionResult<List<string>>> DrugSearch([FromQuery] string search)
        {
            var vq = new VistaQuery("ORWDX WRLST");
            vq.addParameter(VistaQuery.LITERAL, search ?? "");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Place a new medication order.
        /// RPC: ORWDX SAVE
        /// </summary>
        [HttpPost, Route("api/medication/order")]
        public async Task<ActionResult<string>> Order(
            [FromQuery] string dfn,
            [FromQuery] string drug,
            [FromQuery] string sig,
            [FromQuery] string type)
        {
            var vq = new VistaQuery("ORWDX SAVE");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            vq.addParameter(VistaQuery.LITERAL, drug ?? "");
            vq.addParameter(VistaQuery.LITERAL, sig ?? "");
            vq.addParameter(VistaQuery.LITERAL, type ?? "O");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Discontinue a medication order.
        /// RPC: ORWDXA DC (matches legacy rOrders.pas → DCOrder)
        /// </summary>
        [HttpPost, Route("api/medication/discontinue")]
        public async Task<ActionResult<string>> Discontinue(
            [FromQuery] string orderId,
            [FromQuery] long provider,
            [FromQuery] long location,
            [FromQuery] string reason,
            [FromQuery] string dcOrigOrder,
            [FromQuery] bool newOrder)
        {
            var vq = new VistaQuery("ORWDXA DC");
            vq.addParameter(VistaQuery.LITERAL, orderId ?? "");
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, reason ?? "");
            vq.addParameter(VistaQuery.LITERAL, dcOrigOrder ?? "0");
            vq.addParameter(VistaQuery.LITERAL, newOrder ? "1" : "0");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Place medication on hold.
        /// RPC: ORWDXA HOLD (matches legacy rOrders.pas → HoldOrder)
        /// </summary>
        [HttpPost, Route("api/medication/hold")]
        public async Task<ActionResult<string>> Hold(
            [FromQuery] string orderId,
            [FromQuery] long provider)
        {
            var vq = new VistaQuery("ORWDXA HOLD");
            vq.addParameter(VistaQuery.LITERAL, orderId ?? "");
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Release a held medication.
        /// RPC: ORWDXA UNHOLD (matches legacy rOrders.pas → ReleaseOrderHold)
        /// </summary>
        [HttpPost, Route("api/medication/unhold")]
        public async Task<ActionResult<string>> Unhold(
            [FromQuery] string orderId,
            [FromQuery] long provider)
        {
            var vq = new VistaQuery("ORWDXA UNHOLD");
            vq.addParameter(VistaQuery.LITERAL, orderId ?? "");
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Transfer medication between inpatient/outpatient.
        /// RPC: ORWDXA TRANSFER
        /// </summary>
        [HttpPost, Route("api/medication/transfer")]
        public async Task<ActionResult<string>> Transfer([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXA TRANSFER");
            vq.addParameter(VistaQuery.LITERAL, orderId ?? "");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Renew a medication order.
        /// RPC: ORWDXA RENEW
        /// </summary>
        [HttpPost, Route("api/medication/renew")]
        public async Task<ActionResult<string>> Renew([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXA RENEW");
            vq.addParameter(VistaQuery.LITERAL, orderId ?? "");
            return await this.Session.sQuery(vq);
        }
    }
}
