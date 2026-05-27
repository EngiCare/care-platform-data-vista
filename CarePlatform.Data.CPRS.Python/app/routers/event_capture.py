# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""EventCapture router — mirrors EventCaptureController.cs.

ECS — ESSO installation check, visit/division IDs, user path, reports, printing.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/eventcapture/isessoinstalled")
async def is_esso_installed(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORECS01 CHKESSO")
    return await session.s_query(vq)


@router.get("/api/eventcapture/visitid")
async def get_visit_id(visitStr: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORECS01 VSITID")
    vq.add_parameter(VistaQuery.LITERAL, visitStr)
    return await session.s_query(vq)


@router.get("/api/eventcapture/division")
async def get_division_id(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORECS01 GETDIV")
    return await session.s_query(vq)


@router.post("/api/eventcapture/path")
async def save_user_path(pathInfo: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORECS01 SAVPATH")
    vq.add_parameter(VistaQuery.LITERAL, pathInfo)
    return await session.s_query(vq)


@router.post("/api/eventcapture/report")
async def load_report(
    reportHandle: str = Query(...),
    reportType: str = Query(...),
    printDev: str = Query(...),
    dfn: str = Query(...),
    startDate: str = Query(...),
    endDate: str = Query(...),
    needReason: str = Query(...),
    userId: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORECS01 ECRPT")
    dhl = DictionaryHashList()
    dhl.add('"ECHNDL"', reportHandle or "")
    dhl.add('"ECPTYP"', reportType or "")
    dhl.add('"ECDEV"', printDev or "")
    dhl.add('"ECDFN"', dfn or "")
    dhl.add('"ECSD"', startDate or "")
    dhl.add('"ECED"', endDate or "")
    dhl.add('"ECRY"', needReason or "")
    dhl.add('"ECDUZ"', userId or "")
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/eventcapture/print")
async def print_report(
    reportHandle: str = Query(...),
    reportType: str = Query(...),
    printDev: str = Query(...),
    dfn: str = Query(...),
    startDate: str = Query(...),
    endDate: str = Query(...),
    needReason: str = Query(...),
    userId: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORECS01 ECPRINT")
    dhl = DictionaryHashList()
    dhl.add('"ECHNDL"', reportHandle or "")
    dhl.add('"ECPTYP"', reportType or "")
    dhl.add('"ECDEV"', printDev or "")
    dhl.add('"ECDFN"', dfn or "")
    dhl.add('"ECSD"', startDate or "")
    dhl.add('"ECED"', endDate or "")
    dhl.add('"ECRY"', needReason or "")
    dhl.add('"ECDUZ"', userId or "")
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)
