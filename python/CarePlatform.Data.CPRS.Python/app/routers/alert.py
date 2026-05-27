# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Alert router — mirrors AlertController.cs.

Notification management — load, delete, forward, renew, sort, follow-up,
kill alerts, notification aliases, smart-alert helpers.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Load & Query Alerts ────────────────────────────────────────────

@router.get("/api/alert/list")
async def alert_list(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORB FASTUSER")
    return await session.t_query(vq)


@router.get("/api/alert/longtext")
async def long_text(alertId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORB GETLTXT")
    vq.add_parameter(VistaQuery.LITERAL, alertId)
    return await session.t_query(vq)


@router.get("/api/alert/data")
async def data(xqaid: str = Query(...), flag: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORB GETDATA")
    vq.add_parameter(VistaQuery.LITERAL, xqaid)
    vq.add_parameter(VistaQuery.LITERAL, flag)
    return await session.s_query(vq)


@router.get("/api/alert/followuptext")
async def follow_up_text(patientDfn: str = Query(...), notification: int = Query(...), xqadata: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORB TEXT FOLLOWUP")
    vq.add_parameter(VistaQuery.LITERAL, patientDfn)
    vq.add_parameter(VistaQuery.LITERAL, str(notification))
    vq.add_parameter(VistaQuery.LITERAL, xqadata)
    return await session.t_query(vq)


@router.get("/api/alert/tiualertinfo")
async def tiu_alert_info(xqaid: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU GET ALERT INFO")
    vq.add_parameter(VistaQuery.LITERAL, xqaid)
    return await session.s_query(vq)


@router.get("/api/alert/issmartalert")
async def is_smart_alert(notIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORBSMART ISSMNOT")
    vq.add_parameter(VistaQuery.LITERAL, str(notIen))
    return await session.s_query(vq)


@router.get("/api/alert/unsignedordersfollowup")
async def unsigned_orders_follow_up(xqaid: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORB UNSIG ORDERS FOLLOWUP")
    vq.add_parameter(VistaQuery.LITERAL, xqaid)
    return await session.s_query(vq)


# ── Alert Actions ──────────────────────────────────────────────────

@router.post("/api/alert/delete")
async def delete_alert(xqaid: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORB DELETE ALERT")
    vq.add_parameter(VistaQuery.LITERAL, xqaid)
    return await session.s_query(vq)


@router.post("/api/alert/deleteforuser")
async def delete_for_user(xqaid: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORB DELETE ALERT")
    vq.add_parameter(VistaQuery.LITERAL, xqaid)
    vq.add_parameter(VistaQuery.LITERAL, "1")
    return await session.s_query(vq)


@router.post("/api/alert/forward")
async def forward(xqaid: str = Query(...), recipient: str = Query(...), forwardType: str = Query(...), comment: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORB FORWARD ALERT")
    vq.add_parameter(VistaQuery.LITERAL, xqaid)
    vq.add_parameter(VistaQuery.LITERAL, recipient)
    vq.add_parameter(VistaQuery.LITERAL, forwardType)
    vq.add_parameter(VistaQuery.LITERAL, comment)
    return await session.s_query(vq)


@router.post("/api/alert/renew")
async def renew(xqaid: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORB RENEW ALERT")
    vq.add_parameter(VistaQuery.LITERAL, xqaid)
    return await session.s_query(vq)


# ── Alert Sorting ──────────────────────────────────────────────────

@router.get("/api/alert/sortmethod")
async def get_sort_method(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORB GETSORT")
    return await session.s_query(vq)


@router.post("/api/alert/setsortmethod")
async def set_sort_method(sort: str = Query(...), direction: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORB SETSORT")
    vq.add_parameter(VistaQuery.LITERAL, sort)
    vq.add_parameter(VistaQuery.LITERAL, direction)
    return await session.s_query(vq)


# ── Alert Cleanup (Kill Alerts) ────────────────────────────────────

@router.post("/api/alert/killunsignedorders")
async def kill_unsigned_orders_alert(patientDfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORB KILL UNSIG ORDERS ALERT")
    vq.add_parameter(VistaQuery.LITERAL, patientDfn)
    return await session.s_query(vq)


@router.post("/api/alert/killexpiringmeds")
async def kill_expiring_meds_alert(patientDfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORB KILL EXPIR MED ALERT")
    vq.add_parameter(VistaQuery.LITERAL, patientDfn)
    return await session.s_query(vq)


@router.post("/api/alert/killexpiringoi")
async def kill_expiring_oi_alert(patientDfn: str = Query(...), followUp: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORB KILL EXPIR OI ALERT")
    vq.add_parameter(VistaQuery.LITERAL, patientDfn)
    vq.add_parameter(VistaQuery.LITERAL, str(followUp))
    return await session.s_query(vq)


@router.post("/api/alert/killunverifiedmeds")
async def kill_unverified_meds_alert(patientDfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORB KILL UNVER MEDS ALERT")
    vq.add_parameter(VistaQuery.LITERAL, patientDfn)
    return await session.s_query(vq)


@router.post("/api/alert/killunverifiedorders")
async def kill_unverified_orders_alert(patientDfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORB KILL UNVER ORDERS ALERT")
    vq.add_parameter(VistaQuery.LITERAL, patientDfn)
    return await session.s_query(vq)


@router.post("/api/alert/autounflagorders")
async def auto_unflag_orders(patientDfn: str = Query(...), xqaid: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORB AUTOUNFLAG ORDERS")
    vq.add_parameter(VistaQuery.LITERAL, patientDfn)
    vq.add_parameter(VistaQuery.LITERAL, xqaid)
    return await session.s_query(vq)


# ── Notification Alias Endpoints ───────────────────────────────────

@router.get("/api/notification/list")
async def notification_list(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORB FASTUSER")
    return await session.t_query(vq)


@router.get("/api/notification/count")
async def notification_count(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORB UNSORTED")
    return await session.s_query(vq)


@router.post("/api/notification/delete")
async def notification_delete(alertId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORB DELETE ALERT")
    vq.add_parameter(VistaQuery.LITERAL, alertId)
    return await session.s_query(vq)


@router.post("/api/notification/forward")
async def notification_forward(alertId: str = Query(...), toUser: str = Query(""), comment: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORB FORWARD ALERT")
    vq.add_parameter(VistaQuery.LITERAL, alertId)
    vq.add_parameter(VistaQuery.LITERAL, toUser)
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, comment)
    return await session.s_query(vq)


@router.post("/api/notification/defer")
async def notification_defer(alertId: str = Query(...), deferUntil: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORB DEFER ALERT")
    vq.add_parameter(VistaQuery.LITERAL, alertId)
    vq.add_parameter(VistaQuery.LITERAL, deferUntil)
    return await session.s_query(vq)


# ── ORB3UTL — Smart Notification Helpers ───────────────────────────

@router.get("/api/alert/existingnotes")
async def get_existing_notes(noteTitleIen: int = Query(...), dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORB3UTL GET EXISTING NOTES")
    vq.add_parameter(VistaQuery.LITERAL, str(noteTitleIen))
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/alert/description")
async def get_description(alertId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORB3UTL GET DESCRIPTION")
    vq.add_parameter(VistaQuery.LITERAL, alertId)
    return await session.t_query(vq)
