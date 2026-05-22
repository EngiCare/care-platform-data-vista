# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Vitals router — mirrors VitalsController.cs.

Latest vitals, by-note, rate check, store, grid, detail, mark-error.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/vitals/latest")
async def latest(dfn: str = Query(...), encDate: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQVI VITALS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    if encDate:
        vq.add_parameter(VistaQuery.LITERAL, encDate)
    return await session.t_query(vq)


@router.get("/api/vitals/bynote")
async def by_note(dfn: str = Query(...), noteIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQVI NOTEVIT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.t_query(vq)


@router.get("/api/vitals/ratecheck")
async def rate_check(type: str = Query(...), rate: str = Query(...), unit: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQVI2 VITALS RATE CHECK")
    vq.add_parameter(VistaQuery.LITERAL, type)
    vq.add_parameter(VistaQuery.LITERAL, rate)
    vq.add_parameter(VistaQuery.LITERAL, unit)
    return await session.s_query(vq)


@router.post("/api/vitals/store")
async def store(vitalList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQVI2 VITALS VAL & STORE")
    dhl = DictionaryHashList()
    for i, item in enumerate(vitalList):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/vitals/grid")
async def grid(dfn: str = Query(...), date1: str = Query(...), date2: str = Query(...), restrictDates: int = Query(0), tests: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("GMV ORQQVI1 GRID")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    vq.add_parameter(VistaQuery.LITERAL, str(restrictDates))
    vq.add_parameter(VistaQuery.LITERAL, tests)
    return await session.t_query(vq)


@router.get("/api/vitals/detail")
async def detail(dfn: str = Query(...), date1: str = Query(...), date2: str = Query(...), tests: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("GMV ORQQVI1 DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    vq.add_parameter(VistaQuery.LITERAL, "0")
    vq.add_parameter(VistaQuery.LITERAL, tests)
    return await session.t_query(vq)


@router.get("/api/vitals/coverdetail")
async def cover_detail(dfn: str = Query(""), type: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("GMV ORQQVI1 DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    vq.add_parameter(VistaQuery.LITERAL, type)
    return await session.t_query(vq)


@router.post("/api/vitals/markerror")
async def mark_error(dfn: str = Query(""), ien: str = Query(""), reason: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("GMV MARK ERROR")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    vq.add_parameter(VistaQuery.LITERAL, reason or "")
    return await session.s_query(vq)


# ── Simple convenience endpoints ───────────────────────────────────

@router.get("/api/vitals/list")
async def all_data(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("GMV V/M ALLDATA")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.post("/api/vitals/add")
async def add_vital(dfn: str = Query(...), vitalType: str = Query(""), reading: str = Query(""), dateTime: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("GMV ADD VM")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, vitalType)
    vq.add_parameter(VistaQuery.LITERAL, reading)
    vq.add_parameter(VistaQuery.LITERAL, dateTime)
    return await session.s_query(vq)
