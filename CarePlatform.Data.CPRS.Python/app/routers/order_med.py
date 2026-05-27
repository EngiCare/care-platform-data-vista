# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Order med router — mirrors OrderMedController.cs.

Pharmacy order dialogs — DEA, formulations, dosing, schedules, routes,
copay, supplies — ORWDPS* RPCs.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── DEA / Authorization ──────────────────────────────────────────

@router.get("/api/ordermed/deacheck")
async def dea_check(orderableItem: int = Query(...), provider: int = Query(...), ptType: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS1 FAILDEA")
    vq.add_parameter(VistaQuery.LITERAL, str(orderableItem))
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    vq.add_parameter(VistaQuery.LITERAL, ptType)
    return await session.s_query(vq)


@router.get("/api/ordermed/ivdeacheck")
async def iv_dea_check(orderableItem: int = Query(...), oiType: str = Query(...), provider: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS1 IVDEA")
    vq.add_parameter(VistaQuery.LITERAL, str(orderableItem))
    vq.add_parameter(VistaQuery.LITERAL, oiType)
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    return await session.s_query(vq)


@router.get("/api/ordermed/authcheck")
async def auth_check(provider: int = Query(...), dlgId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS32 AUTH")
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    vq.add_parameter(VistaQuery.LITERAL, dlgId)
    return await session.s_query(vq)


@router.get("/api/ordermed/authnva")
async def auth_nva(provider: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS32 AUTHNVA")
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    return await session.s_query(vq)


# ── Order Dialog Data ────────────────────────────────────────────

@router.get("/api/ordermed/odformedsin")
async def od_for_meds_in(dfn: str = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS1 ODSLCT")
    vq.add_parameter(VistaQuery.LITERAL, "U")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.get("/api/ordermed/odformedsout")
async def od_for_meds_out(dfn: str = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS1 ODSLCT")
    vq.add_parameter(VistaQuery.LITERAL, "O")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.get("/api/ordermed/odfordialog")
async def od_for_dialog(psType: str = Query(...), dfn: str = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS32 DLGSLCT")
    vq.add_parameter(VistaQuery.LITERAL, psType)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.get("/api/ordermed/oiformed")
async def oi_for_med(ien: int = Query(...), ptType: str = Query(...), dfn: str = Query(...), needPI: bool = Query(False), isPKI: bool = Query(False), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS2 OISLCT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, ptType)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, "Y" if needPI else "N")
    vq.add_parameter(VistaQuery.LITERAL, "Y" if isPKI else "N")
    return await session.t_query(vq)


@router.get("/api/ordermed/oifordialog")
async def oi_for_dialog(ien: int = Query(...), psType: str = Query(...), dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS32 OISLCT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, psType)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


# ── Formulary / Alternates ───────────────────────────────────────

@router.get("/api/ordermed/formularyalt")
async def formulary_alt(ien: int = Query(...), ptType: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS1 FORMALT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, ptType)
    return await session.t_query(vq)


@router.get("/api/ordermed/formularyaltdose")
async def formulary_alt_dose(dispenseDrug: str = Query(...), oi: int = Query(...), ptType: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS1 DOSEALT")
    vq.add_parameter(VistaQuery.LITERAL, dispenseDrug)
    vq.add_parameter(VistaQuery.LITERAL, str(oi))
    vq.add_parameter(VistaQuery.LITERAL, ptType)
    return await session.t_query(vq)


@router.get("/api/ordermed/formularyalt32")
async def formulary_alt_32(ien: int = Query(...), psType: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS32 FORMALT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, psType)
    return await session.t_query(vq)


# ── Schedules / Routes / Dosing ──────────────────────────────────

@router.get("/api/ordermed/schedules")
async def schedules(dfn: str = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS1 SCHALL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.get("/api/ordermed/dowschedules")
async def dow_schedules(dfn: str = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS1 DOWSCH")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.get("/api/ordermed/allroutes")
async def all_routes(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS32 ALLROUTE")
    return await session.t_query(vq)


@router.get("/api/ordermed/allivroutes")
async def all_iv_routes(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS32 ALLIVRTE")
    return await session.t_query(vq)


@router.get("/api/ordermed/validateroute")
async def validate_route(name: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS32 VALROUTE")
    vq.add_parameter(VistaQuery.LITERAL, name)
    return await session.s_query(vq)


@router.get("/api/ordermed/validateschedule")
async def validate_schedule(schedule: str = Query(...), psType: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS32 VALSCH")
    vq.add_parameter(VistaQuery.LITERAL, schedule)
    vq.add_parameter(VistaQuery.LITERAL, psType)
    return await session.s_query(vq)


@router.get("/api/ordermed/validaterate")
async def validate_rate(rate: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS32 VALRATE")
    vq.add_parameter(VistaQuery.LITERAL, rate)
    return await session.s_query(vq)


@router.get("/api/ordermed/validatequantity")
async def validate_quantity(qty: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS32 VALQTY")
    vq.add_parameter(VistaQuery.LITERAL, qty.strip())
    return await session.s_query(vq)


# ── Quantities / Days / Refills ──────────────────────────────────

@router.get("/api/ordermed/qtytodays")
async def qty_to_days(quantity: str = Query(...), unitsPerDose: str = Query(...), schedule: str = Query(...), duration: str = Query(...), dfn: str = Query(...), drug: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS2 QTY2DAY")
    vq.add_parameter(VistaQuery.LITERAL, quantity)
    vq.add_parameter(VistaQuery.LITERAL, unitsPerDose)
    vq.add_parameter(VistaQuery.LITERAL, schedule)
    vq.add_parameter(VistaQuery.LITERAL, duration)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, drug)
    return await session.s_query(vq)


@router.get("/api/ordermed/daystoqty")
async def days_to_qty(daysSupply: str = Query(...), unitsPerDose: str = Query(...), schedule: str = Query(...), duration: str = Query(...), dfn: str = Query(...), drug: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS2 DAY2QTY")
    vq.add_parameter(VistaQuery.LITERAL, daysSupply)
    vq.add_parameter(VistaQuery.LITERAL, unitsPerDose)
    vq.add_parameter(VistaQuery.LITERAL, schedule)
    vq.add_parameter(VistaQuery.LITERAL, duration)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, drug)
    return await session.s_query(vq)


@router.get("/api/ordermed/defaultdays")
async def default_days(unitStr: str = Query(...), schedStr: str = Query(...), dfn: str = Query(...), drug: str = Query(...), oi: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS1 DFLTSPLY")
    vq.add_parameter(VistaQuery.LITERAL, unitStr)
    vq.add_parameter(VistaQuery.LITERAL, schedStr)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, drug)
    vq.add_parameter(VistaQuery.LITERAL, str(oi))
    return await session.s_query(vq)


@router.get("/api/ordermed/maxrefills")
async def max_refills(dfn: str = Query(...), drug: str = Query(...), days: int = Query(...), ordItem: int = Query(...), discharge: bool = Query(False), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS2 MAXREF")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, drug)
    vq.add_parameter(VistaQuery.LITERAL, str(days))
    vq.add_parameter(VistaQuery.LITERAL, str(ordItem))
    vq.add_parameter(VistaQuery.LITERAL, "1" if discharge else "0")
    return await session.s_query(vq)


@router.get("/api/ordermed/maxdayssupply")
async def max_days_supply(orderableIen: int = Query(...), drugIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS1 MAXDS")
    vq.add_parameter(VistaQuery.LITERAL, str(orderableIen))
    vq.add_parameter(VistaQuery.LITERAL, str(drugIen))
    return await session.s_query(vq)


@router.get("/api/ordermed/schedulerequired")
async def schedule_required(ordItem: int = Query(...), route: str = Query(...), drug: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS2 SCHREQ")
    vq.add_parameter(VistaQuery.LITERAL, str(ordItem))
    vq.add_parameter(VistaQuery.LITERAL, route)
    vq.add_parameter(VistaQuery.LITERAL, drug)
    return await session.s_query(vq)


# ── Admin / Pickup ───────────────────────────────────────────────

@router.get("/api/ordermed/admininfo")
async def admin_info(dfn: str = Query(...), schedule: str = Query(...), ordItem: int = Query(...), location: int = Query(...), admin: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS2 ADMIN")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, schedule)
    vq.add_parameter(VistaQuery.LITERAL, str(ordItem))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, admin)
    return await session.s_query(vq)


@router.get("/api/ordermed/admintime")
async def admin_time(dfn: str = Query(...), schedule: str = Query(...), ordItem: int = Query(...), location: int = Query(...), startText: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS2 REQST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, schedule)
    vq.add_parameter(VistaQuery.LITERAL, str(ordItem))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, startText)
    return await session.s_query(vq)


@router.get("/api/ordermed/pickupforlocation")
async def pickup_for_location(location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS1 LOCPICK")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


# ── IV ────────────────────────────────────────────────────────────

@router.get("/api/ordermed/ivamounts")
async def iv_amounts(ien: int = Query(...), fluidType: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS32 IVAMT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, fluidType)
    return await session.s_query(vq)


@router.get("/api/ordermed/ivdosageformroutes")
async def iv_dosage_form_routes(orderIds: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS33 IVDOSFRM")
    vq.add_parameter(VistaQuery.LITERAL, orderIds)
    vq.add_parameter(VistaQuery.LITERAL, "0")
    return await session.t_query(vq)


@router.get("/api/ordermed/defaultaddfreq")
async def default_add_freq(oid: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS33 GETADDFR")
    vq.add_parameter(VistaQuery.LITERAL, oid)
    return await session.s_query(vq)


# ── Drug Properties ──────────────────────────────────────────────

@router.get("/api/ordermed/issupply")
async def is_supply(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS32 ISSPLY")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.get("/api/ordermed/isiv")
async def is_iv(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS32 MEDISIV")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.get("/api/ordermed/dispensemessage")
async def dispense_message(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS32 DRUGMSG")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.get("/api/ordermed/copay")
async def copay_required(dfn: str = Query(...), dispenseDrug: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS32 SCSTS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, dispenseDrug)
    return await session.s_query(vq)


@router.get("/api/ordermed/isactivateoi")
async def is_activate_oi(oi: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXA ISACTOI")
    vq.add_parameter(VistaQuery.LITERAL, oi)
    return await session.s_query(vq)


@router.get("/api/ordermed/hasroutedefined")
async def has_route_defined(qoId: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS1 HASROUTE")
    vq.add_parameter(VistaQuery.LITERAL, str(qoId))
    return await session.s_query(vq)


@router.get("/api/ordermed/checkexistingpi")
async def check_existing_pi(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS2 CHKPI")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/ordermed/usenewmeddialogs")
async def use_new_med_dialogs(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS1 CHK94")
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.s_query(vq)


@router.get("/api/ordermed/checkordergroup")
async def check_order_group(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS2 CHKGRP")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/ordermed/differentlocations")
async def different_order_locations(orderId: str = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS33 COMPLOC")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


# ── Copay ─────────────────────────────────────────────────────────

@router.post("/api/ordermed/copaylist")
async def copay_list(orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS4 CPLST")
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/ordermed/savecopy")
async def save_copay_status(items: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS4 CPINFO")
    dhl = DictionaryHashList()
    for i, item in enumerate(items):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/ordermed/verbaltelpolicy")
async def verbal_tel_policy(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS5 ISVTP")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/ordermed/lesvalidation")
async def les_validation(orderInfo: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDPS5 LESAPI")
    vq.add_parameter(VistaQuery.LITERAL, orderInfo)
    return await session.t_query(vq)


@router.get("/api/ordermed/lesdispgroup")
async def les_disp_group(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDPS5 LESGRP")
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.s_query(vq)
