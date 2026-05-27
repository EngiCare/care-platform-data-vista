# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Order router — mirrors OrderController.cs.

Core order operations — retrieval, send, sign, actions, locking,
display groups, views, results, PKI, printing, renew/complex.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.models.order import Order
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Order Retrieval ────────────────────────────────────────────────

@router.get("/api/order/list")
async def order_list(dfn: str = Query(...), filter: str = Query(...), groups: str = Query(...), session: ISession = Depends(get_current_session)) -> list[Order]:
    vq = VistaQuery("ORWORR GET")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, filter)
    vq.add_parameter(VistaQuery.LITERAL, groups)
    results = await session.t_query(vq)
    return Order.parse_list(results)


@router.get("/api/order/listabbr")
async def list_abbr(dfn: str = Query(...), filterTS: str = Query(...), dGroup: str = Query(...), timeFrom: str = Query(""), timeThru: str = Query(""), ptEvtId: str = Query(""), alertedUserOnly: bool = Query(False), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORR AGET")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, filterTS)
    vq.add_parameter(VistaQuery.LITERAL, dGroup)
    vq.add_parameter(VistaQuery.LITERAL, timeFrom)
    vq.add_parameter(VistaQuery.LITERAL, timeThru)
    vq.add_parameter(VistaQuery.LITERAL, ptEvtId)
    vq.add_parameter(VistaQuery.LITERAL, "1" if alertedUserOnly else "0")
    return await session.t_query(vq)


@router.get("/api/order/listdcrl")
async def list_dcrl(dfn: str = Query(...), filterTS: str = Query(...), dGroup: str = Query(...), timeFrom: str = Query(""), timeThru: str = Query(""), ptEvtId: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORR RGET")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, filterTS)
    vq.add_parameter(VistaQuery.LITERAL, dGroup)
    vq.add_parameter(VistaQuery.LITERAL, timeFrom)
    vq.add_parameter(VistaQuery.LITERAL, timeThru)
    vq.add_parameter(VistaQuery.LITERAL, ptEvtId)
    return await session.t_query(vq)


@router.post("/api/order/fields")
async def fields(textView: str = Query(...), ctxtTime: str = Query(...), orderIds: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORR GET4LST")
    vq.add_parameter(VistaQuery.LITERAL, textView)
    vq.add_parameter(VistaQuery.LITERAL, ctxtTime)
    dhl = DictionaryHashList()
    for i, oid in enumerate(orderIds):
        dhl.add(str(i), oid)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/order/text")
async def order_text(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORR GETTXT")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


@router.get("/api/order/byifn")
async def by_ifn(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORR GETBYIFN")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


@router.get("/api/order/result")
async def result(dfn: str = Query(...), orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWOR RESULT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


@router.get("/api/order/resulthistory")
async def result_history(dfn: str = Query(...), orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWOR RESULT HISTORY")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


@router.get("/api/order/unsigned")
async def unsigned(dfn: str = Query(...), filter: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWOR UNSIGN")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    if filter:
        vq.add_parameter(VistaQuery.LITERAL, filter)
    return await session.t_query(vq)


@router.get("/api/order/status")
async def status(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("OREVNTX1 GETSTS")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.post("/api/order/statusmatch")
async def status_match(dfn: str = Query(...), orders: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX1 ORDMATCH")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    dhl = DictionaryHashList()
    for i, o in enumerate(orders):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── Order Send / Sign ─────────────────────────────────────────────

@router.post("/api/order/send")
async def send(dfn: str = Query(...), provider: int = Query(...), location: int = Query(...), esCode: str = Query(...), orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX SEND")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, esCode)
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/order/sendrelease")
async def send_release(orderList: list[str] = Body(...), currTS: str = Query(""), location: int = Query(0), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX SENDED")
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, currTS)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.post("/api/order/sendandprint")
async def send_and_print(dfn: str = Query(...), provider: int = Query(...), location: int = Query(...), esCode: str = Query(...), deviceInfo: str = Query(...), orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX SENDP")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, esCode)
    vq.add_parameter(VistaQuery.LITERAL, deviceInfo)
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/order/requiressignature")
async def requires_signature(orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWD1 SIG4ANY")
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.get("/api/order/requiressignatureone")
async def requires_signature_one(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWD1 SIG4ONE")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/order/requiresdigitalsig")
async def requires_digital_sig(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWOR1 CHKDIG")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.post("/api/order/storedigitalsig")
async def store_digital_sig(orderId: str = Query(...), hash: str = Query(...), length: int = Query(...), provider: int = Query(...), dfn: str = Query(...), crlUrl: str = Query(...), sigArray: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWOR1 SIG")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, hash)
    vq.add_parameter(VistaQuery.LITERAL, str(length))
    vq.add_parameter(VistaQuery.LITERAL, "100")
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    dhl = DictionaryHashList()
    for i, s in enumerate(sigArray):
        dhl.add(str(i), s)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, crlUrl)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/order/digitalsig")
async def digital_sig(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORR GETDSIG")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


@router.get("/api/order/dea")
async def dea(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORR GETDEA")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


# ── Order Actions ─────────────────────────────────────────────────

@router.get("/api/order/validateaction")
async def validate_action(orderId: str = Query(...), action: str = Query(...), provider: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXA VALID")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, action)
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    return await session.s_query(vq)


@router.post("/api/order/hold")
async def hold(orderId: str = Query(...), provider: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXA HOLD")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    return await session.t_query(vq)


@router.post("/api/order/unhold")
async def unhold(orderId: str = Query(...), provider: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXA UNHOLD")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    return await session.t_query(vq)


@router.post("/api/order/discontinue")
async def discontinue(orderId: str = Query(...), provider: int = Query(...), location: int = Query(...), reason: str = Query(""), dcOrigOrder: bool = Query(False), newOrder: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXA DC")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, reason)
    vq.add_parameter(VistaQuery.LITERAL, "1" if dcOrigOrder else "0")
    vq.add_parameter(VistaQuery.LITERAL, newOrder)
    return await session.t_query(vq)


@router.get("/api/order/dcreasonien")
async def dc_reason_ien(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXA DCREQIEN")
    return await session.s_query(vq)


@router.get("/api/order/dcreasons")
async def dc_reasons(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX2 DCREASON")
    return await session.t_query(vq)


@router.post("/api/order/alert")
async def alert_order(orderId: str = Query(...), alertRecip: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXA ALERT")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, alertRecip)
    return await session.t_query(vq)


@router.post("/api/order/flag")
async def flag(orderId: str = Query(...), reason: str = Query(...), alertRecip: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXA FLAG")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, reason)
    vq.add_parameter(VistaQuery.LITERAL, alertRecip)
    return await session.t_query(vq)


@router.get("/api/order/flagreason")
async def flag_reason(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXA FLAGTXT")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


@router.post("/api/order/unflag")
async def unflag(orderId: str = Query(...), comment: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXA UNFLAG")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, comment)
    return await session.t_query(vq)


@router.post("/api/order/complete")
async def complete(orderId: str = Query(...), esCode: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXA COMPLETE")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, esCode)
    return await session.t_query(vq)


@router.post("/api/order/verify")
async def verify(orderId: str = Query(...), esCode: str = Query(...), chartReview: bool = Query(False), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXA VERIFY")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, esCode)
    if chartReview:
        vq.add_parameter(VistaQuery.LITERAL, "R")
    return await session.t_query(vq)


@router.get("/api/order/wardcomments")
async def get_ward_comments(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXA WCGET")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


@router.post("/api/order/wardcomments")
async def save_ward_comments(orderId: str = Query(...), comments: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXA WCPUT")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    dhl = DictionaryHashList()
    for i, c in enumerate(comments):
        dhl.add(str(i), c)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.get("/api/order/validatecomplexaction")
async def validate_complex_action(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXA OFCPLX")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/order/actiontext")
async def action_text(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWOR ACTION TEXT")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


# ── Locking ───────────────────────────────────────────────────────

@router.post("/api/order/lockpatient")
async def lock_patient(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX LOCK")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.post("/api/order/unlockpatient")
async def unlock_patient(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX UNLOCK")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.post("/api/order/lock")
async def lock_order(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX LOCK ORDER")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.post("/api/order/unlock")
async def unlock_order(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX UNLOCK ORDER")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


# ── Display Groups & Views ────────────────────────────────────────

@router.get("/api/order/dgroupmap")
async def display_group_map(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORDG MAPSEQ")
    return await session.t_query(vq)


@router.get("/api/order/dgroupien")
async def display_group_ien(name: str = Query("ALL"), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWORDG IEN")
    vq.add_parameter(VistaQuery.LITERAL, name)
    return await session.s_query(vq)


@router.get("/api/order/dgroupall")
async def display_group_all(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORDG ALLTREE")
    return await session.t_query(vq)


@router.get("/api/order/filterstatuses")
async def filter_statuses(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORDG REVSTS")
    return await session.t_query(vq)


@router.get("/api/order/sheets")
async def sheets(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWOR SHEETS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/order/eventsheets")
async def event_sheets(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("OREVNTX PAT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/order/viewdefault")
async def view_default(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWOR VWGET")
    return await session.s_query(vq)


@router.post("/api/order/viewdefault")
async def save_view_default(viewSettings: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWOR VWSET")
    vq.add_parameter(VistaQuery.LITERAL, viewSettings)
    return await session.t_query(vq)


@router.get("/api/order/specialties")
async def specialties(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWOR TSALL")
    return await session.t_query(vq)


@router.get("/api/order/expiredstartdt")
async def expired_start_dt(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWOR EXPIRED")
    return await session.s_query(vq)


# ── Renew / Complex ──────────────────────────────────────────────

@router.get("/api/order/renewfields")
async def renew_fields(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXR RNWFLDS")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


@router.get("/api/order/isreleased")
async def is_released(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXR ISREL")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/order/orderableien")
async def orderable_ien(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXR GTORITM")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/order/complexchildren")
async def complex_children(parentId: str = Query(...), currAction: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXR ORCPLX")
    vq.add_parameter(VistaQuery.LITERAL, parentId)
    vq.add_parameter(VistaQuery.LITERAL, currAction)
    return await session.t_query(vq)


@router.get("/api/order/canrenewcomplex")
async def can_renew_complex(parentId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXR CANRN")
    vq.add_parameter(VistaQuery.LITERAL, parentId)
    return await session.s_query(vq)


@router.get("/api/order/iscomplex")
async def is_complex(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXR ISCPLX")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/order/package")
async def package(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXR GETPKG")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/order/canchangerenewed")
async def can_change_renewed(orderId: str = Query(...), isTxtOrder: bool = Query(False), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXR01 CANCHG")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, "1" if isTxtOrder else "0")
    return await session.s_query(vq)


@router.post("/api/order/saverenewchanges")
async def save_renew_changes(orderId: str = Query(...), refills: str = Query(...), pickup: str = Query(...), isTxtOrder: bool = Query(False), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXR01 SAVCHG")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, refills)
    vq.add_parameter(VistaQuery.LITERAL, pickup)
    vq.add_parameter(VistaQuery.LITERAL, "1" if isTxtOrder else "0")
    return await session.s_query(vq)


# ── Printing ──────────────────────────────────────────────────────

@router.post("/api/order/printonreview")
async def print_on_review(location: int = Query(...), deviceInfo: str = Query(...), orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWD1 RVPRINT")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, deviceInfo)
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/order/printservicecopies")
async def print_service_copies(location: int = Query(...), orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWD1 SVONLY")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/order/printgui")
async def print_gui(location: int = Query(...), deviceInfo: str = Query(...), orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWD1 PRINTGUI")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, deviceInfo)
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/order/printdeviceinfo")
async def print_device_info(location: int = Query(...), nature: str = Query(...), orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWD2 DEVINFO")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, nature)
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/order/commonlocation")
async def common_location(orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWD1 COMLOC")
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── PKI / Drug Info ───────────────────────────────────────────────

@router.get("/api/order/pkisite")
async def pki_site(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWOR PKISITE")
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.s_query(vq)


@router.get("/api/order/pkiuse")
async def pki_use(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWOR PKIUSE")
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.s_query(vq)


@router.get("/api/order/drugschedule")
async def drug_schedule(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWOR1 GETDSCH")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/order/externaltext")
async def get_external_text(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWOR1 GETDTEXT")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


@router.post("/api/order/externaltext")
async def set_external_text(orderId: str = Query(...), drugSchedule: str = Query(...), userId: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWOR1 SETDTEXT")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, drugSchedule)
    vq.add_parameter(VistaQuery.LITERAL, str(userId))
    return await session.t_query(vq)


@router.get("/api/order/checkdata")
async def check_data(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXR01 OXDATA")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


# ── Order Change & DC/Renew ──────────────────────────────────────

@router.post("/api/order/change")
async def change(dfn: str = Query(...), imo: bool = Query(...), orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX CHANGE")
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, "1" if imo else "0")
    return await session.t_query(vq)


@router.get("/api/order/patientward")
async def patient_ward(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX1 PATWARD")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/order/dcreninfo")
async def dc_ren_info(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX1 DCREN")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


@router.post("/api/order/undcorig")
async def undo_discontinue_original(orderArr: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX1 UNDCORIG")
    dhl = DictionaryHashList()
    for i, o in enumerate(orderArr):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


# ── Web.CPRS Convenience ─────────────────────────────────────────

@router.get("/api/order/displaygroups")
async def display_groups(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWORDG MAPSEQ")
    return await session.t_query(vq)


@router.get("/api/order/detail")
async def detail(dfn: str = Query(...), ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQOR DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    return await session.t_query(vq)


@router.post("/api/order/place")
async def place(dfn: str = Query(...), type: str = Query(...), item: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX SAVE")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    vq.add_parameter(VistaQuery.LITERAL, type or "")
    vq.add_parameter(VistaQuery.LITERAL, item or "")
    return await session.s_query(vq)


@router.post("/api/order/copy")
async def copy(dfn: str = Query(...), orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX SAVE")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    vq.add_parameter(VistaQuery.LITERAL, orderId or "")
    return await session.s_query(vq)


@router.post("/api/order/sign")
async def sign(dfn: str = Query(...), orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXR01 OXDATA")
    vq.add_parameter(VistaQuery.LITERAL, orderId or "")
    return await session.s_query(vq)


@router.post("/api/order/renew")
async def renew(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXA RENEW")
    vq.add_parameter(VistaQuery.LITERAL, orderId or "")
    return await session.s_query(vq)


@router.post("/api/order/dc")
async def dc(orderId: str = Query(...), reason: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXA DC")
    vq.add_parameter(VistaQuery.LITERAL, orderId or "")
    vq.add_parameter(VistaQuery.LITERAL, reason or "")
    return await session.s_query(vq)


@router.get("/api/order/check")
async def check(dfn: str = Query(...), orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXC DISPLAY")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    vq.add_parameter(VistaQuery.LITERAL, orderId or "")
    return await session.t_query(vq)
