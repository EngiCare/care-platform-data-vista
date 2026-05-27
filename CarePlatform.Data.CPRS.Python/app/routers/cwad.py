# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""CWAD router — mirrors CWADController.cs.

Crisis / Warnings / Allergies / Directives — problem detail, allergy detail, postings.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/cwad/problemdetail")
async def problem_detail(
    dfn: str = Query(...),
    encounterDateTime: str = Query(...),
    ien: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, f"{dfn}^{encounterDateTime}")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.t_query(vq)


@router.get("/api/cwad/allergydetail")
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


@router.get("/api/cwad/allergylistreport")
async def allergy_list_report(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQAL LIST REPORT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/cwad/allergies")
async def list_allergies(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQAL LIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/cwad/postingdetail")
async def posting_detail(
    dfn: str = Query(...),
    id: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    if id == "A":
        vq = VistaQuery("ORQQAL LIST REPORT")
        vq.add_parameter(VistaQuery.LITERAL, dfn)
        return await session.t_query(vq)
    elif id.startswith("WH^"):
        vq = VistaQuery("WVRPCOR POSTREP")
        vq.add_parameter(VistaQuery.LITERAL, dfn)
        vq.add_parameter(VistaQuery.LITERAL, id[3:4])
        return await session.t_query(vq)
    else:
        vq = VistaQuery("TIU GET RECORD TEXT")
        vq.add_parameter(VistaQuery.LITERAL, id)
        return await session.t_query(vq)


@router.get("/api/cwad/postings")
async def list_postings(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPP LIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/cwad/demographics")
async def demographics(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPT PTINQ")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)
