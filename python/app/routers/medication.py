# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Medication router — mirrors MedicationController.cs.

Active lists, detail, admin history, refill, status changes, ordering,
discontinue, hold/unhold, transfer, renew.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query
from starlette.responses import JSONResponse

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/medication/active")
async def active(
    dfn: str = Query(...),
    duz: int = Query(...),
    view: int = Query(0),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWPS ACTIVE")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(duz))
    vq.add_parameter(VistaQuery.LITERAL, str(view))
    vq.add_parameter(VistaQuery.LITERAL, "1")  # include instructions
    return await session.t_query(vq)


@router.get("/api/medication/detail")
async def detail(
    dfn: str = Query(...),
    id: str = Query(...),
    session: ISession = Depends(get_current_session),
):
    if not id:
        return JSONResponse(status_code=400, content="id parameter is required.")
    vq = VistaQuery("ORWPS DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, id.upper())
    return await session.t_query(vq)


@router.get("/api/medication/adminhistory")
async def admin_history(
    dfn: str = Query(...),
    orderId: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWPS MEDHIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.t_query(vq)


@router.get("/api/medication/newdialog")
async def new_dialog(
    inpatient: bool = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWPS1 NEWDLG")
    vq.add_parameter(VistaQuery.LITERAL, "1" if inpatient else "0")
    return await session.s_query(vq)


@router.get("/api/medication/pickupdefault")
async def pickup_default(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPS1 PICKUP")
    return await session.s_query(vq)


@router.post("/api/medication/refill")
async def refill(
    orderId: str = Query(...),
    pickUpAt: str = Query("W"),
    dfn: str = Query(...),
    provider: int = Query(...),
    location: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWPS1 REFILL")
    vq.add_parameter(VistaQuery.LITERAL, orderId or "")
    vq.add_parameter(VistaQuery.LITERAL, pickUpAt or "W")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


@router.get("/api/medication/isfirstdosenow")
async def is_first_dose_now(
    orderId: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWDXR ISNOW")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.post("/api/medication/statuschangecheck")
async def status_change_check(
    dfn: str = Query(...),
    medIds: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWDX1 STCHANGE")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    dhl = DictionaryHashList()
    for i, mid in enumerate(medIds):
        dhl.add(str(i), mid)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.get("/api/medication/coverdetail")
async def cover_detail(
    dfn: str = Query(""),
    orderId: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWPS COVER")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    return await session.t_query(vq)


@router.get("/api/medication/drugsearch")
async def drug_search(
    search: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWDX WRLST")
    vq.add_parameter(VistaQuery.LITERAL, search or "")
    return await session.t_query(vq)


@router.post("/api/medication/order")
async def order(
    dfn: str = Query(""),
    drug: str = Query(""),
    sig: str = Query(""),
    type: str = Query("O"),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWDX SAVE")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    vq.add_parameter(VistaQuery.LITERAL, drug or "")
    vq.add_parameter(VistaQuery.LITERAL, sig or "")
    vq.add_parameter(VistaQuery.LITERAL, type or "O")
    return await session.s_query(vq)


@router.post("/api/medication/discontinue")
async def discontinue(
    orderId: str = Query(""),
    provider: int = Query(...),
    location: int = Query(...),
    reason: str = Query(""),
    dcOrigOrder: str = Query("0"),
    newOrder: bool = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWDXA DC")
    vq.add_parameter(VistaQuery.LITERAL, orderId or "")
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, reason or "")
    vq.add_parameter(VistaQuery.LITERAL, dcOrigOrder or "0")
    vq.add_parameter(VistaQuery.LITERAL, "1" if newOrder else "0")
    return await session.s_query(vq)


@router.post("/api/medication/hold")
async def hold(
    orderId: str = Query(""),
    provider: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWDXA HOLD")
    vq.add_parameter(VistaQuery.LITERAL, orderId or "")
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    return await session.s_query(vq)


@router.post("/api/medication/unhold")
async def unhold(
    orderId: str = Query(""),
    provider: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWDXA UNHOLD")
    vq.add_parameter(VistaQuery.LITERAL, orderId or "")
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    return await session.s_query(vq)


@router.post("/api/medication/transfer")
async def transfer(
    orderId: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWDXA TRANSFER")
    vq.add_parameter(VistaQuery.LITERAL, orderId or "")
    return await session.s_query(vq)


@router.post("/api/medication/renew")
async def renew(
    orderId: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWDXA RENEW")
    vq.add_parameter(VistaQuery.LITERAL, orderId or "")
    return await session.s_query(vq)
