# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""CoverSheet router — mirrors CoverSheetController.cs.

Allergies, postings, demographics, problem/allergy detail, dynamic panels.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, HTTPException, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()

# Per-session cache of allowed RPC names for paneldata/paneldetail
_allowed_rpcs: dict[int, set[str]] = {}


@router.get("/api/coversheet/allergies")
async def allergies(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQAL LIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/coversheet/allergyreport")
async def allergy_report(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQAL LIST REPORT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/coversheet/allergydetail")
async def allergy_detail(
    dfn: str = Query(...),
    ien: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQAL DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.t_query(vq)


@router.get("/api/coversheet/postings")
async def postings(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPP LIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/coversheet/postingdetail")
async def posting_detail(
    dfn: str = Query(...),
    flag: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("WVRPCOR POSTREP")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, flag)
    return await session.t_query(vq)


@router.get("/api/coversheet/recordtext")
async def record_text(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU GET RECORD TEXT")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.get("/api/coversheet/demographics")
async def demographics(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPT PTINQ")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/coversheet/problemdetail")
async def problem_detail(
    dfn: str = Query(...),
    ien: int = Query(...),
    dateTime: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, f"{dfn}^{dateTime}")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.t_query(vq)


@router.get("/api/coversheet/immunizations")
async def immunizations(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("PXVIMM ADMIN HX")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/coversheet/config")
async def coversheet_config(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWCV1 COVERSHEET LIST")
    return await session.t_query(vq)


@router.get("/api/coversheet/paneldata")
async def panel_data(
    rpc: str = Query(...),
    dfn: str = Query(...),
    param1: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    if not rpc or not rpc.strip():
        raise HTTPException(status_code=400, detail="rpc parameter is required.")
    allowed = await _get_allowed_rpcs(session)
    if rpc not in allowed:
        raise HTTPException(status_code=400, detail=f"RPC '{rpc}' is not in the cover sheet configuration.")
    vq = VistaQuery(rpc)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    if param1:
        vq.add_parameter(VistaQuery.LITERAL, param1)
    return await session.t_query(vq)


@router.get("/api/coversheet/paneldetail")
async def panel_detail(
    rpc: str = Query(...),
    dfn: str = Query(...),
    ien: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    if not rpc or not rpc.strip():
        raise HTTPException(status_code=400, detail="rpc parameter is required.")
    allowed = await _get_allowed_rpcs(session)
    if rpc not in allowed:
        raise HTTPException(status_code=400, detail=f"RPC '{rpc}' is not in the cover sheet configuration.")
    vq = VistaQuery(rpc)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


async def _get_allowed_rpcs(session: ISession) -> set[str]:
    sid = id(session)
    if sid in _allowed_rpcs:
        return _allowed_rpcs[sid]
    vq = VistaQuery("ORWCV1 COVERSHEET LIST")
    config_lines = await session.t_query(vq)
    rpcs: set[str] = set()
    for line in config_lines:
        if not line or not line.strip():
            continue
        pieces = line.split("^")
        if len(pieces) > 5 and pieces[5].strip():
            rpcs.add(pieces[5])
        if len(pieces) > 15 and pieces[15].strip():
            rpcs.add(pieces[15])
    _allowed_rpcs[sid] = rpcs
    return rpcs
