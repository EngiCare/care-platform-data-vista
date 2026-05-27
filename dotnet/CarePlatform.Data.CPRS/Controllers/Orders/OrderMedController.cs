// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Pharmacy order dialogs — DEA checks, formulations, dosing, schedules, routes,
    /// copay, supplies. Migrated from cprs/Orders/rODMeds.pas + rODBase.pas — ORWDPS* RPCs.
    /// </summary>
    public class OrderMedController : BaseController
    {
        public OrderMedController() : base() { }

        #region DEA / Authorization

        [HttpGet, Route("api/ordermed/deacheck")]
        public async Task<ActionResult<string>> DEACheck(
            [FromQuery] int orderableItem, [FromQuery] long provider, [FromQuery] string ptType)
        {
            var vq = new VistaQuery("ORWDPS1 FAILDEA");
            vq.addParameter(VistaQuery.LITERAL, orderableItem.ToString());
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            vq.addParameter(VistaQuery.LITERAL, ptType);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/ivdeacheck")]
        public async Task<ActionResult<string>> IVDEACheck(
            [FromQuery] int orderableItem, [FromQuery] string oiType, [FromQuery] long provider)
        {
            var vq = new VistaQuery("ORWDPS1 IVDEA");
            vq.addParameter(VistaQuery.LITERAL, orderableItem.ToString());
            vq.addParameter(VistaQuery.LITERAL, oiType);
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/authcheck")]
        public async Task<ActionResult<string>> AuthCheck(
            [FromQuery] long provider, [FromQuery] string dlgId)
        {
            var vq = new VistaQuery("ORWDPS32 AUTH");
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            vq.addParameter(VistaQuery.LITERAL, dlgId);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/authnva")]
        public async Task<ActionResult<string>> AuthNVA([FromQuery] long provider)
        {
            var vq = new VistaQuery("ORWDPS32 AUTHNVA");
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Order Dialog Data

        [HttpGet, Route("api/ordermed/odformedsin")]
        public async Task<ActionResult<List<string>>> ODForMedsIn(
            [FromQuery] string dfn, [FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDPS1 ODSLCT");
            vq.addParameter(VistaQuery.LITERAL, "U"); // PST_UNIT_DOSE
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/odformedsout")]
        public async Task<ActionResult<List<string>>> ODForMedsOut(
            [FromQuery] string dfn, [FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDPS1 ODSLCT");
            vq.addParameter(VistaQuery.LITERAL, "O"); // PST_OUTPATIENT
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/odfordialog")]
        public async Task<ActionResult<List<string>>> ODForDialog(
            [FromQuery] string psType, [FromQuery] string dfn, [FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDPS32 DLGSLCT");
            vq.addParameter(VistaQuery.LITERAL, psType);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/oiformed")]
        public async Task<ActionResult<List<string>>> OIForMed(
            [FromQuery] int ien, [FromQuery] string ptType, [FromQuery] string dfn,
            [FromQuery] bool needPI = false, [FromQuery] bool isPKI = false)
        {
            var vq = new VistaQuery("ORWDPS2 OISLCT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, ptType);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, needPI ? "Y" : "N");
            vq.addParameter(VistaQuery.LITERAL, isPKI ? "Y" : "N");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/oifordialog")]
        public async Task<ActionResult<List<string>>> OIForDialog(
            [FromQuery] int ien, [FromQuery] string psType, [FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWDPS32 OISLCT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, psType);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Formulary / Alternates

        [HttpGet, Route("api/ordermed/formularyalt")]
        public async Task<ActionResult<List<string>>> FormularyAlt(
            [FromQuery] int ien, [FromQuery] string ptType)
        {
            var vq = new VistaQuery("ORWDPS1 FORMALT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, ptType);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/formularyaltdose")]
        public async Task<ActionResult<List<string>>> FormularyAltDose(
            [FromQuery] string dispenseDrug, [FromQuery] int oi, [FromQuery] string ptType)
        {
            var vq = new VistaQuery("ORWDPS1 DOSEALT");
            vq.addParameter(VistaQuery.LITERAL, dispenseDrug);
            vq.addParameter(VistaQuery.LITERAL, oi.ToString());
            vq.addParameter(VistaQuery.LITERAL, ptType);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/formularyalt32")]
        public async Task<ActionResult<List<string>>> FormularyAlt32(
            [FromQuery] int ien, [FromQuery] string psType)
        {
            var vq = new VistaQuery("ORWDPS32 FORMALT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, psType);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Schedules / Routes / Dosing

        [HttpGet, Route("api/ordermed/schedules")]
        public async Task<ActionResult<List<string>>> Schedules(
            [FromQuery] string dfn, [FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDPS1 SCHALL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/dowschedules")]
        public async Task<ActionResult<List<string>>> DOWSchedules(
            [FromQuery] string dfn, [FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDPS1 DOWSCH");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/allroutes")]
        public async Task<ActionResult<List<string>>> AllRoutes()
        {
            var vq = new VistaQuery("ORWDPS32 ALLROUTE");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/allivroutes")]
        public async Task<ActionResult<List<string>>> AllIVRoutes()
        {
            var vq = new VistaQuery("ORWDPS32 ALLIVRTE");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/validateroute")]
        public async Task<ActionResult<string>> ValidateRoute([FromQuery] string name)
        {
            var vq = new VistaQuery("ORWDPS32 VALROUTE");
            vq.addParameter(VistaQuery.LITERAL, name);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/validateschedule")]
        public async Task<ActionResult<string>> ValidateSchedule(
            [FromQuery] string schedule, [FromQuery] string psType)
        {
            var vq = new VistaQuery("ORWDPS32 VALSCH");
            vq.addParameter(VistaQuery.LITERAL, schedule);
            vq.addParameter(VistaQuery.LITERAL, psType);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/validaterate")]
        public async Task<ActionResult<string>> ValidateRate([FromQuery] string rate)
        {
            var vq = new VistaQuery("ORWDPS32 VALRATE");
            vq.addParameter(VistaQuery.LITERAL, rate);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/validatequantity")]
        public async Task<ActionResult<string>> ValidateQuantity([FromQuery] string qty)
        {
            var vq = new VistaQuery("ORWDPS32 VALQTY");
            vq.addParameter(VistaQuery.LITERAL, qty.Trim());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Quantities / Days / Refills

        [HttpGet, Route("api/ordermed/qtytodays")]
        public async Task<ActionResult<string>> QtyToDays(
            [FromQuery] string quantity, [FromQuery] string unitsPerDose,
            [FromQuery] string schedule, [FromQuery] string duration,
            [FromQuery] string dfn, [FromQuery] string drug)
        {
            var vq = new VistaQuery("ORWDPS2 QTY2DAY");
            vq.addParameter(VistaQuery.LITERAL, quantity);
            vq.addParameter(VistaQuery.LITERAL, unitsPerDose);
            vq.addParameter(VistaQuery.LITERAL, schedule);
            vq.addParameter(VistaQuery.LITERAL, duration);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, drug);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/daystoqty")]
        public async Task<ActionResult<string>> DaysToQty(
            [FromQuery] string daysSupply, [FromQuery] string unitsPerDose,
            [FromQuery] string schedule, [FromQuery] string duration,
            [FromQuery] string dfn, [FromQuery] string drug)
        {
            var vq = new VistaQuery("ORWDPS2 DAY2QTY");
            vq.addParameter(VistaQuery.LITERAL, daysSupply);
            vq.addParameter(VistaQuery.LITERAL, unitsPerDose);
            vq.addParameter(VistaQuery.LITERAL, schedule);
            vq.addParameter(VistaQuery.LITERAL, duration);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, drug);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/defaultdays")]
        public async Task<ActionResult<string>> DefaultDays(
            [FromQuery] string unitStr, [FromQuery] string schedStr,
            [FromQuery] string dfn, [FromQuery] string drug, [FromQuery] int oi)
        {
            var vq = new VistaQuery("ORWDPS1 DFLTSPLY");
            vq.addParameter(VistaQuery.LITERAL, unitStr);
            vq.addParameter(VistaQuery.LITERAL, schedStr);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, drug);
            vq.addParameter(VistaQuery.LITERAL, oi.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/maxrefills")]
        public async Task<ActionResult<string>> MaxRefills(
            [FromQuery] string dfn, [FromQuery] string drug, [FromQuery] int days,
            [FromQuery] int ordItem, [FromQuery] bool discharge = false)
        {
            var vq = new VistaQuery("ORWDPS2 MAXREF");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, drug);
            vq.addParameter(VistaQuery.LITERAL, days.ToString());
            vq.addParameter(VistaQuery.LITERAL, ordItem.ToString());
            vq.addParameter(VistaQuery.LITERAL, discharge ? "1" : "0");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/maxdayssupply")]
        public async Task<ActionResult<string>> MaxDaysSupply(
            [FromQuery] int orderableIen, [FromQuery] int drugIen)
        {
            var vq = new VistaQuery("ORWDPS1 MAXDS");
            vq.addParameter(VistaQuery.LITERAL, orderableIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, drugIen.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/schedulerequired")]
        public async Task<ActionResult<string>> ScheduleRequired(
            [FromQuery] int ordItem, [FromQuery] string route, [FromQuery] string drug)
        {
            var vq = new VistaQuery("ORWDPS2 SCHREQ");
            vq.addParameter(VistaQuery.LITERAL, ordItem.ToString());
            vq.addParameter(VistaQuery.LITERAL, route);
            vq.addParameter(VistaQuery.LITERAL, drug);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Admin / Pickup

        [HttpGet, Route("api/ordermed/admininfo")]
        public async Task<ActionResult<string>> AdminInfo(
            [FromQuery] string dfn, [FromQuery] string schedule,
            [FromQuery] int ordItem, [FromQuery] long location, [FromQuery] string admin = "")
        {
            var vq = new VistaQuery("ORWDPS2 ADMIN");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, schedule);
            vq.addParameter(VistaQuery.LITERAL, ordItem.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, admin);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/admintime")]
        public async Task<ActionResult<string>> AdminTime(
            [FromQuery] string dfn, [FromQuery] string schedule,
            [FromQuery] int ordItem, [FromQuery] long location, [FromQuery] string startText = "")
        {
            var vq = new VistaQuery("ORWDPS2 REQST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, schedule);
            vq.addParameter(VistaQuery.LITERAL, ordItem.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, startText);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/pickupforlocation")]
        public async Task<ActionResult<string>> PickupForLocation([FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDPS1 LOCPICK");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region IV

        [HttpGet, Route("api/ordermed/ivamounts")]
        public async Task<ActionResult<string>> IVAmounts(
            [FromQuery] int ien, [FromQuery] string fluidType)
        {
            var vq = new VistaQuery("ORWDPS32 IVAMT");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, fluidType);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/ivdosageformroutes")]
        public async Task<ActionResult<List<string>>> IVDosageFormRoutes(
            [FromQuery] string orderIds)
        {
            var vq = new VistaQuery("ORWDPS33 IVDOSFRM");
            vq.addParameter(VistaQuery.LITERAL, orderIds);
            vq.addParameter(VistaQuery.LITERAL, "0"); // False
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/defaultaddfreq")]
        public async Task<ActionResult<string>> DefaultAddFreq([FromQuery] string oid)
        {
            var vq = new VistaQuery("ORWDPS33 GETADDFR");
            vq.addParameter(VistaQuery.LITERAL, oid);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Drug Properties

        [HttpGet, Route("api/ordermed/issupply")]
        public async Task<ActionResult<string>> IsSupply([FromQuery] int ien)
        {
            var vq = new VistaQuery("ORWDPS32 ISSPLY");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/isiv")]
        public async Task<ActionResult<string>> IsIV([FromQuery] int ien)
        {
            var vq = new VistaQuery("ORWDPS32 MEDISIV");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/dispensemessage")]
        public async Task<ActionResult<string>> DispenseMessage([FromQuery] int ien)
        {
            var vq = new VistaQuery("ORWDPS32 DRUGMSG");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/copay")]
        public async Task<ActionResult<string>> CopayRequired(
            [FromQuery] string dfn, [FromQuery] string dispenseDrug)
        {
            var vq = new VistaQuery("ORWDPS32 SCSTS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, dispenseDrug);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/isactivateoi")]
        public async Task<ActionResult<string>> IsActivateOI([FromQuery] string oi)
        {
            var vq = new VistaQuery("ORWDXA ISACTOI");
            vq.addParameter(VistaQuery.LITERAL, oi);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/hasroutedefined")]
        public async Task<ActionResult<string>> HasRouteDefined([FromQuery] int qoId)
        {
            var vq = new VistaQuery("ORWDPS1 HASROUTE");
            vq.addParameter(VistaQuery.LITERAL, qoId.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/checkexistingpi")]
        public async Task<ActionResult<string>> CheckExistingPI([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDPS2 CHKPI");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/usenewmeddialogs")]
        public async Task<ActionResult<string>> UseNewMedDialogs()
        {
            var vq = new VistaQuery("ORWDPS1 CHK94");
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/checkordergroup")]
        public async Task<ActionResult<string>> CheckOrderGroup([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDPS2 CHKGRP");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/differentlocations")]
        public async Task<ActionResult<string>> DifferentOrderLocations(
            [FromQuery] string orderId, [FromQuery] long location)
        {
            var vq = new VistaQuery("ORWDPS33 COMPLOC");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Copay

        [HttpPost, Route("api/ordermed/copaylist")]
        public async Task<ActionResult<List<string>>> CopayList([FromBody] List<string> orderList)
        {
            var vq = new VistaQuery("ORWDPS4 CPLST");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < orderList.Count; i++)
                dhl.Add(i.ToString(), orderList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/ordermed/savecopy")]
        public async Task<ActionResult<List<string>>> SaveCopayStatus([FromBody] List<string> items)
        {
            var vq = new VistaQuery("ORWDPS4 CPINFO");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < items.Count; i++)
                dhl.Add(i.ToString(), items[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/verbaltelpolicy")]
        public async Task<ActionResult<string>> VerbalTelPolicy([FromQuery] string orderId)
        {
            var vq = new VistaQuery("ORWDPS5 ISVTP");
            vq.addParameter(VistaQuery.LITERAL, orderId);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/ordermed/lesvalidation")]
        public async Task<ActionResult<List<string>>> LESValidation([FromQuery] string orderInfo)
        {
            var vq = new VistaQuery("ORWDPS5 LESAPI");
            vq.addParameter(VistaQuery.LITERAL, orderInfo);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/ordermed/lesdispgroup")]
        public async Task<ActionResult<string>> LESDispGroup()
        {
            var vq = new VistaQuery("ORWDPS5 LESGRP");
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
