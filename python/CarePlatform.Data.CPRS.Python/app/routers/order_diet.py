# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Order diet router — mirrors OrderDietController.cs.

Diet/nutrition order dialogs — ORWDFH RPCs.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/orderdiet/attributes")
async def diet_attributes(orderableItem: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDFH ATTR")
    vq.add_parameter(VistaQuery.LITERAL, str(orderableItem))
    return await session.s_query(vq)


@router.get("/api/orderdiet/params")
async def load_diet_params(dfn: str = Query(...), location: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDFH PARAM")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, location)
    return await session.t_query(vq)


@router.get("/api/orderdiet/currentdiettext")
async def current_diet_text(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDFH TXT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/orderdiet/tubefeeding")
async def tube_feeding_products(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDFH TFPROD")
    return await session.t_query(vq)


@router.get("/api/orderdiet/expandedqty")
async def expanded_quantity(product: int = Query(...), strength: int = Query(...), qty: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDFH QTY2CC")
    vq.add_parameter(VistaQuery.LITERAL, str(product))
    vq.add_parameter(VistaQuery.LITERAL, str(strength))
    vq.add_parameter(VistaQuery.LITERAL, qty)
    return await session.s_query(vq)


@router.get("/api/orderdiet/diets")
async def subset_of_diets(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDFH DIETS")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/orderdiet/opdiets")
async def outpatient_diets(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDFH OPDIETS")
    return await session.t_query(vq)


@router.post("/api/orderdiet/latetray")
async def order_late_tray(dfn: str = Query(...), provider: int = Query(...), location: int = Query(...), meal: str = Query(...), mealTime: str = Query(...), bagged: bool = Query(False), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDFH ADDLATE")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, meal)
    vq.add_parameter(VistaQuery.LITERAL, mealTime)
    vq.add_parameter(VistaQuery.LITERAL, "1" if bagged else "0")
    return await session.t_query(vq)


@router.get("/api/orderdiet/isolationid")
async def isolation_id(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDFH ISOIEN")
    return await session.s_query(vq)


@router.get("/api/orderdiet/currentisolation")
async def current_isolation(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDFH CURISO")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/orderdiet/isolations")
async def load_isolations(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDFH ISOLIST")
    return await session.t_query(vq)


@router.get("/api/orderdiet/dialogtype")
async def diet_dialog_type(groupIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDFH FINDTYP")
    vq.add_parameter(VistaQuery.LITERAL, str(groupIen))
    return await session.s_query(vq)


@router.get("/api/orderdiet/currentmeals")
async def current_recurring_meals(dfn: str = Query(...), mealType: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDFH CURRENT MEALS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, mealType)
    return await session.t_query(vq)


@router.get("/api/orderdiet/nfslocready")
async def outpatient_location_configured(location: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDFH NFSLOC READY")
    vq.add_parameter(VistaQuery.LITERAL, location)
    return await session.s_query(vq)
