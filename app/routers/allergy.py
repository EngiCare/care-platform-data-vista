# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Allergy router — mirrors AllergyController.cs.

Search, add, remove, NKA.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/allergen/search")
async def search(search: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDAL32 ALLERGY MATCH")
    vq.add_parameter(VistaQuery.LITERAL, search or "")
    return await session.t_query(vq)


@router.post("/api/allergy/add")
async def add(
    dfn: str = Query(...),
    allergen: str = Query(""),
    type: str = Query(""),
    severity: str = Query(""),
    symptoms: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWDAL32 SAVE ALLERGY")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    vq.add_parameter(VistaQuery.LITERAL, allergen)
    vq.add_parameter(VistaQuery.LITERAL, type)
    vq.add_parameter(VistaQuery.LITERAL, severity)
    vq.add_parameter(VistaQuery.LITERAL, symptoms)
    return await session.s_query(vq)


@router.post("/api/allergy/remove")
async def remove(
    dfn: str = Query(...),
    ien: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWDAL32 SAVE ALLERGY")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    vq.add_parameter(VistaQuery.LITERAL, "EIE")
    return await session.s_query(vq)


@router.post("/api/allergy/nka")
async def mark_nka(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDAL32 SAVE ALLERGY")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    vq.add_parameter(VistaQuery.LITERAL, "NKA")
    return await session.s_query(vq)
