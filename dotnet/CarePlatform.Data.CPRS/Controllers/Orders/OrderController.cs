// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using CarePlatform.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Core order operations — retrieval, send, sign, actions (hold/DC/flag/verify/complete),
    /// locking, display groups, views, results, PKI, printing.
    /// Migrated from cprs/Orders/rOrders.pas — ORWORR, ORWDX, ORWDXA, ORWD1, ORWD2, ORWOR, ORWORDG RPCs.
    /// </summary>
    public class OrderController : BaseController
    {
        public OrderController() : base() { }

        #region Order Retrieval

        /// <summary>
        /// Load orders by filter and display groups.
        /// RPC: ORWORR GET
        /// </summary>
        [HttpGet, Route("api/order/list")]
        public async Task<ActionResult<List<Models.Orders.Order>>> List(
            [FromQuery] string dfn,
            [FromQuery] string filter,
            [FromQuery] string groups)
        {
            var vq = new VistaQuery("ORWORR GET");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, filter);
            vq.addParameter(VistaQuery.LITERAL, groups);
            var results = await this.Session.tQuery(vq);
            return Models.Orders.Order.ParseList(results);
        }

        /// <summary>
        /// Load abbreviated order list.
        /// RPC: ORWORR AGET
        /// </summary>
        [HttpGet, Route("api/order/listabbr")]
        public async Task<ActionResult<List<string>>> ListAbbr(
            [FromQuery] string dfn,
            [FromQuery] string filterTS,
            [FromQuery] string dGroup,
            [FromQuery] string timeFrom = "",
            [FromQuery] string timeThru = "",
            [FromQuery] string ptEvtId = "",
            [FromQuery] bool alertedUserOnly = false)
        {
            var vq = new VistaQuery("ORWORR AGET");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, filterTS);
            vq.addParameter(VistaQuery.LITERAL, dGroup);
            vq.addParameter(VistaQuery.LITERAL, timeFrom);
            vq.addParameter(VistaQuery.LITERAL, timeThru);
            vq.addParameter(VistaQuery.LITERAL, ptEvtId);
            vq.addParameter(VistaQuery.LITERAL, alertedUserOnly ? "1" : "0");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load abbreviated order list (DC/RL overload).
        /// RPC: ORWORR RGET
        /// </summary>
        [HttpGet, Route("api/order/listdcrl")]
        public async Task<ActionResult<List<string>>> ListDCRL(
            [FromQuery] string dfn,
            [FromQuery] string filterTS,
            [FromQuery] string dGroup,
            [FromQuery] string timeFrom = "",
            [FromQuery] string timeThru = "",
            [FromQuery] string ptEvtId = "")
        {
            var vq = new VistaQuery("ORWORR RGET");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, filterTS);
            vq.addParameter(VistaQuery.LITERAL, dGroup);
            vq.addParameter(VistaQuery.LITERAL, timeFrom);
            vq.addParameter(VistaQuery.LITERAL, timeThru);
            vq.addParameter(VistaQuery.LITERAL, ptEvtId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Retrieve full order fields for a list of order IDs.
        /// RPC: ORWORR GET4LST
        /// </summary>
        [HttpPost, Route("api/order/fields")]
        public async Task<ActionResult<List<string>>> Fields(
            [FromQuery] string textView,
            [FromQuery] string ctxtTime,
            [FromBody] List<string> orderIds)
        {
            var vq = new VistaQuery("ORWORR GET4LST");
            vq.addParameter(VistaQuery.LITERAL, textView);
            vq.addParameter(VistaQuery.LITERAL, ctxtTime);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderIds.Count; i++)
                dhl.Add(i.ToString(), orderIds[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get text for a single order.
        /// RPC: ORWORR GETTXT
        /// </summary>
        [HttpGet, Route("api/order/text")]
        public async Task<ActionResult<List<string>>> Text([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWORR GETTXT");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a single order by IFN.
        /// RPC: ORWORR GETBYIFN
        /// </summary>
        [HttpGet, Route("api/order/byifn")]
        public async Task<ActionResult<List<string>>> ByIFN([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWORR GETBYIFN");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get order result detail.
        /// RPC: ORWOR RESULT
        /// </summary>
        [HttpGet, Route("api/order/result")]
        public async Task<ActionResult<List<string>>> Result(
            [FromQuery] string dfn,
            [FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWOR RESULT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get order result history.
        /// RPC: ORWOR RESULT HISTORY
        /// </summary>
        [HttpGet, Route("api/order/resulthistory")]
        public async Task<ActionResult<List<string>>> ResultHistory(
            [FromQuery] string dfn,
            [FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWOR RESULT HISTORY");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load unsigned orders for current user.
        /// RPC: ORWOR UNSIGN
        /// </summary>
        [HttpGet, Route("api/order/unsigned")]
        public async Task<ActionResult<List<string>>> Unsigned(
            [FromQuery] string dfn,
            [FromQuery] string filter = "")
        {
            var vq = new VistaQuery("ORWOR UNSIGN");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            if (!string.IsNullOrEmpty(filter))
                vq.addParameter(VistaQuery.LITERAL, filter);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get order status by ID.
        /// RPC: OREVNTX1 GETSTS
        /// </summary>
        [HttpGet, Route("api/order/status")]
        public async Task<ActionResult<string>> Status([FromQuery] string orderId)
        {
            var vq = new VistaQuery("OREVNTX1 GETSTS");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if an order status matches.
        /// RPC: ORWDX1 ORDMATCH
        /// </summary>
        [HttpPost, Route("api/order/statusmatch")]
        public async Task<ActionResult<string>> StatusMatch(
            [FromQuery] string dfn,
            [FromBody] List<string> orders)
        {
            var vq = new VistaQuery("ORWDX1 ORDMATCH");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orders.Count; i++)
                dhl.Add(i.ToString(), orders[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Order Send / Sign

        /// <summary>
        /// Send (sign) orders with electronic signature.
        /// RPC: ORWDX SEND
        /// </summary>
        [HttpPost, Route("api/order/send")]
        public async Task<ActionResult<List<string>>> Send(
            [FromQuery] string dfn,
            [FromQuery] long provider,
            [FromQuery] long location,
            [FromQuery] string esCode,
            [FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWDX SEND");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, esCode);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Release orders (send with edit).
        /// RPC: ORWDX SENDED
        /// </summary>
        [HttpPost, Route("api/order/sendrelease")]
        public async Task<ActionResult<List<string>>> SendRelease(
            [FromBody] List<string> orderList,
            [FromQuery] string currTS = "",
            [FromQuery] long location = 0)
        {
            var vq = new VistaQuery("ORWDX SENDED");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, currTS);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Send and print orders.
        /// RPC: ORWDX SENDP
        /// </summary>
        [HttpPost, Route("api/order/sendandprint")]
        public async Task<ActionResult<List<string>>> SendAndPrint(
            [FromQuery] string dfn,
            [FromQuery] long provider,
            [FromQuery] long location,
            [FromQuery] string esCode,
            [FromQuery] string deviceInfo,
            [FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWDX SENDP");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, esCode);
            vq.addParameter(VistaQuery.LITERAL, deviceInfo);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if any orders require signature.
        /// RPC: ORWD1 SIG4ANY
        /// </summary>
        [HttpPost, Route("api/order/requiressignature")]
        public async Task<ActionResult<string>> RequiresSignature([FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWD1 SIG4ANY");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a single order requires signature.
        /// RPC: ORWD1 SIG4ONE
        /// </summary>
        [HttpGet, Route("api/order/requiressignatureone")]
        public async Task<ActionResult<string>> RequiresSignatureOne([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWD1 SIG4ONE");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if an order requires digital (PKI) signature.
        /// RPC: ORWOR1 CHKDIG
        /// </summary>
        [HttpGet, Route("api/order/requiresdigitalsig")]
        public async Task<ActionResult<string>> RequiresDigitalSig([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWOR1 CHKDIG");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Store a digital signature for an order.
        /// RPC: ORWOR1 SIG
        /// </summary>
        [HttpPost, Route("api/order/storedigitalsig")]
        public async Task<ActionResult<List<string>>> StoreDigitalSig(
            [FromQuery] string orderId,
            [FromQuery] string hash,
            [FromQuery] int length,
            [FromQuery] long provider,
            [FromQuery] string dfn,
            [FromQuery] string crlUrl,
            [FromBody] List<string> sigArray)
        {
            var vq = new VistaQuery("ORWOR1 SIG");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, hash);
            vq.addParameter(VistaQuery.LITERAL, length.ToString());
            vq.addParameter(VistaQuery.LITERAL, "100");
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < sigArray.Count; i++)
                dhl.Add(i.ToString(), sigArray[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, crlUrl);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get digital signature for an order.
        /// RPC: ORWORR GETDSIG
        /// </summary>
        [HttpGet, Route("api/order/digitalsig")]
        public async Task<ActionResult<List<string>>> DigitalSig([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWORR GETDSIG");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get DEA info for an order.
        /// RPC: ORWORR GETDEA
        /// </summary>
        [HttpGet, Route("api/order/dea")]
        public async Task<ActionResult<List<string>>> DEA([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWORR GETDEA");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Order Actions

        /// <summary>
        /// Validate an order action before performing it.
        /// RPC: ORWDXA VALID
        /// </summary>
        [HttpGet, Route("api/order/validateaction")]
        public async Task<ActionResult<string>> ValidateAction(
            [FromQuery] string orderId,
            [FromQuery] string action,
            [FromQuery] long provider)
        {
            var vq = new VistaQuery("ORWDXA VALID");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, action);
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Hold an order.
        /// RPC: ORWDXA HOLD
        /// </summary>
        [HttpPost, Route("api/order/hold")]
        public async Task<ActionResult<List<string>>> Hold(
            [FromQuery] string orderId,
            [FromQuery] long provider)
        {
            var vq = new VistaQuery("ORWDXA HOLD");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Release an order from hold.
        /// RPC: ORWDXA UNHOLD
        /// </summary>
        [HttpPost, Route("api/order/unhold")]
        public async Task<ActionResult<List<string>>> Unhold(
            [FromQuery] string orderId,
            [FromQuery] long provider)
        {
            var vq = new VistaQuery("ORWDXA UNHOLD");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Discontinue an order.
        /// RPC: ORWDXA DC
        /// </summary>
        [HttpPost, Route("api/order/discontinue")]
        public async Task<ActionResult<List<string>>> Discontinue(
            [FromQuery] string orderId,
            [FromQuery] long provider,
            [FromQuery] long location,
            [FromQuery] string reason = "",
            [FromQuery] bool dcOrigOrder = false,
            [FromQuery] string newOrder = "")
        {
            var vq = new VistaQuery("ORWDXA DC");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, reason);
            vq.addParameter(VistaQuery.LITERAL, dcOrigOrder ? "1" : "0");
            vq.addParameter(VistaQuery.LITERAL, newOrder);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get DC reason IEN.
        /// RPC: ORWDXA DCREQIEN
        /// </summary>
        [HttpGet, Route("api/order/dcreasonien")]
        public async Task<ActionResult<string>> DCReasonIEN()
        {
            var vq = new VistaQuery("ORWDXA DCREQIEN");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// List DC reasons.
        /// RPC: ORWDX2 DCREASON
        /// </summary>
        [HttpGet, Route("api/order/dcreasons")]
        public async Task<ActionResult<List<string>>> DCReasons()
        {
            var vq = new VistaQuery("ORWDX2 DCREASON");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Alert a recipient about an order.
        /// RPC: ORWDXA ALERT
        /// </summary>
        [HttpPost, Route("api/order/alert")]
        public async Task<ActionResult<List<string>>> Alert(
            [FromQuery] string orderId,
            [FromQuery] string alertRecip)
        {
            var vq = new VistaQuery("ORWDXA ALERT");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, alertRecip);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Flag an order for review.
        /// RPC: ORWDXA FLAG
        /// </summary>
        [HttpPost, Route("api/order/flag")]
        public async Task<ActionResult<List<string>>> Flag(
            [FromQuery] string orderId,
            [FromQuery] string reason,
            [FromQuery] string alertRecip)
        {
            var vq = new VistaQuery("ORWDXA FLAG");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, reason);
            vq.addParameter(VistaQuery.LITERAL, alertRecip);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load an order's flag reason text.
        /// RPC: ORWDXA FLAGTXT
        /// </summary>
        [HttpGet, Route("api/order/flagreason")]
        public async Task<ActionResult<List<string>>> FlagReason([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXA FLAGTXT");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Unflag an order.
        /// RPC: ORWDXA UNFLAG
        /// </summary>
        [HttpPost, Route("api/order/unflag")]
        public async Task<ActionResult<List<string>>> Unflag(
            [FromQuery] string orderId,
            [FromQuery] string comment = "")
        {
            var vq = new VistaQuery("ORWDXA UNFLAG");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, comment);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Complete an order.
        /// RPC: ORWDXA COMPLETE
        /// </summary>
        [HttpPost, Route("api/order/complete")]
        public async Task<ActionResult<List<string>>> Complete(
            [FromQuery] string orderId,
            [FromQuery] string esCode)
        {
            var vq = new VistaQuery("ORWDXA COMPLETE");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, esCode);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Verify an order.
        /// RPC: ORWDXA VERIFY
        /// </summary>
        [HttpPost, Route("api/order/verify")]
        public async Task<ActionResult<List<string>>> Verify(
            [FromQuery] string orderId,
            [FromQuery] string esCode,
            [FromQuery] bool chartReview = false)
        {
            var vq = new VistaQuery("ORWDXA VERIFY");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, esCode);
            if (chartReview)
                vq.addParameter(VistaQuery.LITERAL, "R");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load ward comments for an order.
        /// RPC: ORWDXA WCGET
        /// </summary>
        [HttpGet, Route("api/order/wardcomments")]
        public async Task<ActionResult<List<string>>> WardComments([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXA WCGET");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save ward comments for an order.
        /// RPC: ORWDXA WCPUT
        /// </summary>
        [HttpPost, Route("api/order/wardcomments")]
        public async Task<ActionResult<string>> SaveWardComments(
            [FromQuery] string orderId,
            [FromBody] List<string> comments)
        {
            var vq = new VistaQuery("ORWDXA WCPUT");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < comments.Count; i++)
                dhl.Add(i.ToString(), comments[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Validate a complex order action.
        /// RPC: ORWDXA OFCPLX
        /// </summary>
        [HttpGet, Route("api/order/validatecomplexaction")]
        public async Task<ActionResult<string>> ValidateComplexAction([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXA OFCPLX");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get latest action text for an order.
        /// RPC: ORWOR ACTION TEXT
        /// </summary>
        [HttpGet, Route("api/order/actiontext")]
        public async Task<ActionResult<List<string>>> ActionText([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWOR ACTION TEXT");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Locking

        /// <summary>
        /// Lock a patient for ordering.
        /// RPC: ORWDX LOCK
        /// </summary>
        [HttpPost, Route("api/order/lockpatient")]
        public async Task<ActionResult<string>> LockPatient([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWDX LOCK");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Unlock a patient.
        /// RPC: ORWDX UNLOCK
        /// </summary>
        [HttpPost, Route("api/order/unlockpatient")]
        public async Task<ActionResult<string>> UnlockPatient([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWDX UNLOCK");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Lock a single order.
        /// RPC: ORWDX LOCK ORDER
        /// </summary>
        [HttpPost, Route("api/order/lock")]
        public async Task<ActionResult<string>> LockOrder([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDX LOCK ORDER");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Unlock a single order.
        /// RPC: ORWDX UNLOCK ORDER
        /// </summary>
        [HttpPost, Route("api/order/unlock")]
        public async Task<ActionResult<string>> UnlockOrder([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDX UNLOCK ORDER");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Display Groups & Views

        /// <summary>
        /// Load display group map.
        /// RPC: ORWORDG MAPSEQ
        /// </summary>
        [HttpGet, Route("api/order/dgroupmap")]
        public async Task<ActionResult<List<string>>> DisplayGroupMap()
        {
            var vq = new VistaQuery("ORWORDG MAPSEQ");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get display group IEN by name ('ALL' or specific).
        /// RPC: ORWORDG IEN
        /// </summary>
        [HttpGet, Route("api/order/dgroupien")]
        public async Task<ActionResult<string>> DisplayGroupIEN([FromQuery] string name = "ALL")
        {
            var vq = new VistaQuery("ORWORDG IEN");
            vq.addParameter(VistaQuery.LITERAL, name);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// List all display groups in tree format.
        /// RPC: ORWORDG ALLTREE
        /// </summary>
        [HttpGet, Route("api/order/dgroupall")]
        public async Task<ActionResult<List<string>>> DisplayGroupAll()
        {
            var vq = new VistaQuery("ORWORDG ALLTREE");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List order filter statuses.
        /// RPC: ORWORDG REVSTS
        /// </summary>
        [HttpGet, Route("api/order/filterstatuses")]
        public async Task<ActionResult<List<string>>> FilterStatuses()
        {
            var vq = new VistaQuery("ORWORDG REVSTS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load order sheets.
        /// RPC: ORWOR SHEETS
        /// </summary>
        [HttpGet, Route("api/order/sheets")]
        public async Task<ActionResult<List<string>>> Sheets([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWOR SHEETS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load event-delayed order sheets — maps to LoadOrderSheetsED in rOrders.pas.
        /// RPC: OREVNTX PAT
        /// </summary>
        [HttpGet, Route("api/order/eventsheets")]
        public async Task<ActionResult<List<string>>> EventSheets([FromQuery] string dfn)
        {
            var vq = new VistaQuery("OREVNTX PAT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load default order view settings.
        /// RPC: ORWOR VWGET
        /// </summary>
        [HttpGet, Route("api/order/viewdefault")]
        public async Task<ActionResult<string>> ViewDefault()
        {
            var vq = new VistaQuery("ORWOR VWGET");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save default order view settings.
        /// RPC: ORWOR VWSET
        /// </summary>
        [HttpPost, Route("api/order/viewdefault")]
        public async Task<ActionResult<List<string>>> SaveViewDefault([FromQuery] string viewSettings)
        {
            var vq = new VistaQuery("ORWOR VWSET");
            vq.addParameter(VistaQuery.LITERAL, viewSettings);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List specialties for order filtering.
        /// RPC: ORWOR TSALL
        /// </summary>
        [HttpGet, Route("api/order/specialties")]
        public async Task<ActionResult<List<string>>> Specialties()
        {
            var vq = new VistaQuery("ORWOR TSALL");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get expired orders start date.
        /// RPC: ORWOR EXPIRED
        /// </summary>
        [HttpGet, Route("api/order/expiredstartdt")]
        public async Task<ActionResult<string>> ExpiredStartDT()
        {
            var vq = new VistaQuery("ORWOR EXPIRED");
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Renew / Complex

        /// <summary>
        /// Load renew fields for an order.
        /// RPC: ORWDXR RNWFLDS
        /// </summary>
        [HttpGet, Route("api/order/renewfields")]
        public async Task<ActionResult<List<string>>> RenewFields([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXR RNWFLDS");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if order is released.
        /// RPC: ORWDXR ISREL
        /// </summary>
        [HttpGet, Route("api/order/isreleased")]
        public async Task<ActionResult<string>> IsReleased([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXR ISREL");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get orderable item IEN from order ID.
        /// RPC: ORWDXR GTORITM
        /// </summary>
        [HttpGet, Route("api/order/orderableien")]
        public async Task<ActionResult<string>> OrderableIEN([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXR GTORITM");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get children of a complex order.
        /// RPC: ORWDXR ORCPLX
        /// </summary>
        [HttpGet, Route("api/order/complexchildren")]
        public async Task<ActionResult<List<string>>> ComplexChildren(
            [FromQuery] string parentId,
            [FromQuery] string currAction)
        {
            var vq = new VistaQuery("ORWDXR ORCPLX");
            vq.addParameter(VistaQuery.LITERAL, parentId);
            vq.addParameter(VistaQuery.LITERAL, currAction);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if a complex order is renewable.
        /// RPC: ORWDXR CANRN
        /// </summary>
        [HttpGet, Route("api/order/canrenewcomplex")]
        public async Task<ActionResult<string>> CanRenewComplex([FromQuery] string parentId)
        {
            var vq = new VistaQuery("ORWDXR CANRN");
            vq.addParameter(VistaQuery.LITERAL, parentId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if an order is a complex order.
        /// RPC: ORWDXR ISCPLX
        /// </summary>
        [HttpGet, Route("api/order/iscomplex")]
        public async Task<ActionResult<string>> IsComplex([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXR ISCPLX");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get package type for an order.
        /// RPC: ORWDXR GETPKG
        /// </summary>
        [HttpGet, Route("api/order/package")]
        public async Task<ActionResult<string>> Package([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXR GETPKG");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if renewed order can be changed.
        /// RPC: ORWDXR01 CANCHG
        /// </summary>
        [HttpGet, Route("api/order/canchangerenewed")]
        public async Task<ActionResult<string>> CanChangeRenewed(
            [FromQuery] string orderId,
            [FromQuery] bool isTxtOrder = false)
        {
            var vq = new VistaQuery("ORWDXR01 CANCHG");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, isTxtOrder ? "1" : "0");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save changes on a renewed order.
        /// RPC: ORWDXR01 SAVCHG
        /// </summary>
        [HttpPost, Route("api/order/saverenewchanges")]
        public async Task<ActionResult<string>> SaveRenewChanges(
            [FromQuery] string orderId,
            [FromQuery] string refills,
            [FromQuery] string pickup,
            [FromQuery] bool isTxtOrder = false)
        {
            var vq = new VistaQuery("ORWDXR01 SAVCHG");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, refills);
            vq.addParameter(VistaQuery.LITERAL, pickup);
            vq.addParameter(VistaQuery.LITERAL, isTxtOrder ? "1" : "0");
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Printing

        /// <summary>
        /// Print orders on review.
        /// RPC: ORWD1 RVPRINT
        /// </summary>
        [HttpPost, Route("api/order/printonreview")]
        public async Task<ActionResult<List<string>>> PrintOnReview(
            [FromQuery] long location,
            [FromQuery] string deviceInfo,
            [FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWD1 RVPRINT");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, deviceInfo);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Print service copies only.
        /// RPC: ORWD1 SVONLY
        /// </summary>
        [HttpPost, Route("api/order/printservicecopies")]
        public async Task<ActionResult<List<string>>> PrintServiceCopies(
            [FromQuery] long location,
            [FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWD1 SVONLY");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Execute order printing (GUI).
        /// RPC: ORWD1 PRINTGUI
        /// </summary>
        [HttpPost, Route("api/order/printgui")]
        public async Task<ActionResult<List<string>>> PrintGUI(
            [FromQuery] long location,
            [FromQuery] string deviceInfo,
            [FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWD1 PRINTGUI");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, deviceInfo);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get print device info for orders.
        /// RPC: ORWD2 DEVINFO
        /// </summary>
        [HttpPost, Route("api/order/printdeviceinfo")]
        public async Task<ActionResult<List<string>>> PrintDeviceInfo(
            [FromQuery] long location,
            [FromQuery] string nature,
            [FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWD2 DEVINFO");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, nature);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get common location for a set of orders.
        /// RPC: ORWD1 COMLOC
        /// </summary>
        [HttpPost, Route("api/order/commonlocation")]
        public async Task<ActionResult<string>> CommonLocation([FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWD1 COMLOC");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region PKI / Drug Info

        /// <summary>
        /// Check if PKI digital signing is enabled for site.
        /// RPC: ORWOR PKISITE
        /// </summary>
        [HttpGet, Route("api/order/pkisite")]
        public async Task<ActionResult<string>> PKISite()
        {
            var vq = new VistaQuery("ORWOR PKISITE");
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if PKI is in use.
        /// RPC: ORWOR PKIUSE
        /// </summary>
        [HttpGet, Route("api/order/pkiuse")]
        public async Task<ActionResult<string>> PKIUse()
        {
            var vq = new VistaQuery("ORWOR PKIUSE");
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get drug schedule for an order.
        /// RPC: ORWOR1 GETDSCH
        /// </summary>
        [HttpGet, Route("api/order/drugschedule")]
        public async Task<ActionResult<string>> DrugSchedule([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWOR1 GETDSCH");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get external text for an order.
        /// RPC: ORWOR1 GETDTEXT
        /// </summary>
        [HttpGet, Route("api/order/externaltext")]
        public async Task<ActionResult<List<string>>> ExternalText([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWOR1 GETDTEXT");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Set external text for an order.
        /// RPC: ORWOR1 SETDTEXT
        /// </summary>
        [HttpPost, Route("api/order/externaltext")]
        public async Task<ActionResult<List<string>>> SetExternalText(
            [FromQuery] string orderId,
            [FromQuery] string drugSchedule,
            [FromQuery] long userId)
        {
            var vq = new VistaQuery("ORWOR1 SETDTEXT");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, drugSchedule);
            vq.addParameter(VistaQuery.LITERAL, userId.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get data for order check.
        /// RPC: ORWDXR01 OXDATA
        /// </summary>
        [HttpGet, Route("api/order/checkdata")]
        public async Task<ActionResult<string>> CheckData([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXR01 OXDATA");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Order Change & DC/Renew

        /// <summary>
        /// Change orders (location change).
        /// RPC: ORWDX CHANGE
        /// </summary>
        [HttpPost, Route("api/order/change")]
        public async Task<ActionResult<List<string>>> Change(
            [FromQuery] string dfn,
            [FromQuery] bool imo,
            [FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWDX CHANGE");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, imo ? "1" : "0");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get patient ward for order print location.
        /// RPC: ORWDX1 PATWARD
        /// </summary>
        [HttpGet, Route("api/order/patientward")]
        public async Task<ActionResult<string>> PatientWard([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWDX1 PATWARD");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// DC/Renew an order.
        /// RPC: ORWDX1 DCREN
        /// </summary>
        [HttpGet, Route("api/order/dcreninfo")]
        public async Task<ActionResult<List<string>>> DCRenInfo([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDX1 DCREN");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Undo DC original.
        /// RPC: ORWDX1 UNDCORIG
        /// </summary>
        [HttpPost, Route("api/order/undcorig")]
        public async Task<ActionResult<List<string>>> UndoDiscontinueOriginal([FromBody] List<string> orderArr)
        {
            var vq = new VistaQuery("ORWDX1 UNDCORIG");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderArr.Count; i++)
                dhl.Add(i.ToString(), orderArr[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Web.CPRS Convenience Endpoints

        /// <summary>
        /// Get display groups (alias for dgroupmap).
        /// RPC: ORWORDG MAPSEQ
        /// </summary>
        [HttpGet, Route("api/order/displaygroups")]
        public async Task<ActionResult<List<string>>> DisplayGroups()
        {
            var vq = new VistaQuery("ORWORDG MAPSEQ");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get order detail by IEN — formatted multi-section text shown in the
        /// desktop CPRS "Order Details" dialog.
        /// RPC: ORQOR DETAIL  (rOrders.pas DetailText: CallV('ORQOR DETAIL', [ID, Patient.DFN]))
        /// </summary>
        [HttpGet, Route("api/order/detail")]
        public async Task<ActionResult<List<string>>> Detail(
            [FromQuery] string dfn,
            [FromQuery] string ien)
        {
            var vq = new VistaQuery("ORQOR DETAIL");
            vq.addParameter(VistaQuery.LITERAL, ien ?? "");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Place a new order.
        /// RPC: ORWDX SAVE
        /// </summary>
        [HttpPost, Route("api/order/place")]
        public async Task<ActionResult<string>> Place(
            [FromQuery] string dfn,
            [FromQuery] string type,
            [FromQuery] string item)
        {
            var vq = new VistaQuery("ORWDX SAVE");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            vq.addParameter(VistaQuery.LITERAL, type ?? "");
            vq.addParameter(VistaQuery.LITERAL, item ?? "");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Copy an existing order.
        /// RPC: ORWDX SAVE
        /// </summary>
        [HttpPost, Route("api/order/copy")]
        public async Task<ActionResult<string>> Copy(
            [FromQuery] string dfn,
            [FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDX SAVE");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            vq.addParameter(VistaQuery.LITERAL, orderId ?? "");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Sign orders.
        /// RPC: ORWDXR01 OXDATA
        /// </summary>
        [HttpPost, Route("api/order/sign")]
        public async Task<ActionResult<string>> Sign(
            [FromQuery] string dfn,
            [FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXR01 OXDATA");
            vq.addParameter(VistaQuery.LITERAL, orderId ?? "");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Renew an order.
        /// RPC: ORWDXA RENEW
        /// </summary>
        [HttpPost, Route("api/order/renew")]
        public async Task<ActionResult<string>> Renew([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXA RENEW");
            vq.addParameter(VistaQuery.LITERAL, orderId ?? "");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Discontinue order (short route alias).
        /// RPC: ORWDXA DC
        /// </summary>
        [HttpPost, Route("api/order/dc")]
        public async Task<ActionResult<string>> DC(
            [FromQuery] string orderId,
            [FromQuery] string reason)
        {
            var vq = new VistaQuery("ORWDXA DC");
            vq.addParameter(VistaQuery.LITERAL, orderId ?? "");
            vq.addParameter(VistaQuery.LITERAL, reason ?? "");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Run order checks.
        /// RPC: ORWDXC DISPLAY
        /// </summary>
        [HttpGet, Route("api/order/check")]
        public async Task<ActionResult<List<string>>> Check(
            [FromQuery] string dfn,
            [FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDXC DISPLAY");
            vq.addParameter(VistaQuery.LITERAL, dfn ?? "");
            vq.addParameter(VistaQuery.LITERAL, orderId ?? "");
            return await this.Session.tQuery(vq);
        }

        #endregion
    }
}
