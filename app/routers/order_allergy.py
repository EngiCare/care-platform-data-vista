# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Order allergy router — mirrors OrderAllergyController.cs.

Allergy-related order checks — ORWDAL32 RPCs.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/orderallergy/symptoms")
async def symptoms(causativeAgent: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDAL32 SYMPTOMS")
    vq.add_parameter(VistaQuery.LITERAL, causativeAgent)
    return await session.t_query(vq)


@router.get("/api/orderallergy/allinprogress")
async def all_in_progress(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDAL32 ALLERGY MATCH")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/orderallergy/cliniccausative")
async def clinic_causative(dfn: str = Query(...), causativeAgent: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDAL32 CLINUSER")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, causativeAgent)
    return await session.t_query(vq)


@router.get("/api/orderallergy/lookup")
async def lookup(searchText: str = Query(...), direction: int = Query(1), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDAL32 DEF")
    vq.add_parameter(VistaQuery.LITERAL, searchText)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/orderallergy/detail")
async def detail(allergyIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDAL32 LOAD FOR EDIT")
    vq.add_parameter(VistaQuery.LITERAL, allergyIen)
    return await session.t_query(vq)


@router.get("/api/orderallergy/sitedefaults")
async def site_defaults(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDAL32 SITE PARAMS")
    return await session.t_query(vq)


@router.get("/api/orderallergy/topten")
async def top_ten(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDAL32 MOST COMMON")
    return await session.t_query(vq)


@router.get("/api/orderallergy/diagtermrpt")
async def diag_term_report(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDAL32 DTRPT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)
