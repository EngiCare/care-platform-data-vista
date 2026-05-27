# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Graph router — mirrors GraphController.cs.

Graphing — test groups, public/report settings, data, details, fast data, items,
types, class, taxonomy, dates, user prefs, views, sizing, test spec/testing/lookup.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Test Groups & Settings ──────────────────────────────────────────

@router.get("/api/graph/testgroup")
async def test_group(testNum: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWGRPC TESTGROUP")
    vq.add_parameter(VistaQuery.LITERAL, str(testNum))
    return await session.t_query(vq)


@router.get("/api/graph/public")
async def public_setting(section: str = Query(""), ext: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWGRPC PUBLIC")
    vq.add_parameter(VistaQuery.LITERAL, section)
    vq.add_parameter(VistaQuery.LITERAL, ext)
    return await session.t_query(vq)


@router.get("/api/graph/reportsetting")
async def report_setting(ext: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWGRPC RPTPNL")
    vq.add_parameter(VistaQuery.LITERAL, ext)
    return await session.t_query(vq)


# ── Data Retrieval ──────────────────────────────────────────────────

@router.get("/api/graph/data")
async def data(
    dfn: str = Query(...),
    item: str = Query(...),
    dateRange: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC DATA")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, item)
    vq.add_parameter(VistaQuery.LITERAL, dateRange)
    return await session.t_query(vq)


@router.post("/api/graph/details")
async def details(
    dfn: str = Query(...),
    itemIds: list[str] = Body(...),
    dateRange: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC DETAILS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    dhl = DictionaryHashList()
    for i, item in enumerate(itemIds):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, dateRange)
    return await session.t_query(vq)


# ── Fast Endpoints ──────────────────────────────────────────────────

@router.get("/api/graph/fastdata")
async def fast_data(
    dfn: str = Query(...),
    item: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC FASTDATA")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, item)
    return await session.t_query(vq)


@router.get("/api/graph/fastitem")
async def fast_item(
    item: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC FASTITEM")
    vq.add_parameter(VistaQuery.LITERAL, item)
    return await session.t_query(vq)


@router.get("/api/graph/fastlabs")
async def fast_labs(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC FASTLABS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/graph/fasttask")
async def fast_task(
    dfn: str = Query(...),
    item: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC FASTTASK")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, item)
    return await session.t_query(vq)


# ── Items / Types / Classes ─────────────────────────────────────────

@router.get("/api/graph/items")
async def items(
    typeNum: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC ITEMS")
    vq.add_parameter(VistaQuery.LITERAL, str(typeNum))
    return await session.t_query(vq)


@router.get("/api/graph/types")
async def types(
    classNum: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC TYPES")
    vq.add_parameter(VistaQuery.LITERAL, str(classNum))
    return await session.t_query(vq)


@router.get("/api/graph/class")
async def graph_class(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWGRPC CLASS")
    return await session.t_query(vq)


@router.post("/api/graph/taxonomy")
async def taxonomy(
    itemIds: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC TAX")
    dhl = DictionaryHashList()
    for i, item in enumerate(itemIds):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


# ── Dates ───────────────────────────────────────────────────────────

@router.get("/api/graph/dateitem")
async def date_item(
    dfn: str = Query(...),
    item: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWGRPC DATEITEM")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, item)
    return await session.s_query(vq)


@router.get("/api/graph/dates")
async def dates(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWGRPC DATES")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


# ── User Prefs ──────────────────────────────────────────────────────

@router.get("/api/graph/userprefs")
async def user_prefs(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWGRPC GETPREF")
    return await session.s_query(vq)


@router.post("/api/graph/userprefs")
async def set_user_prefs(
    value: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWGRPC SETPREF")
    vq.add_parameter(VistaQuery.LITERAL, value)
    return await session.s_query(vq)


# ── Views ───────────────────────────────────────────────────────────

@router.get("/api/graph/getviews")
async def get_views(
    paramName: str = Query(""),
    permission: str = Query(""),
    ext: str = Query(""),
    userx: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC GETVIEWS")
    vq.add_parameter(VistaQuery.LITERAL, paramName)
    vq.add_parameter(VistaQuery.LITERAL, permission)
    vq.add_parameter(VistaQuery.LITERAL, ext)
    vq.add_parameter(VistaQuery.LITERAL, userx)
    return await session.t_query(vq)


@router.post("/api/graph/setviews")
async def set_views(
    paramName: str = Query(...),
    permission: str = Query(...),
    paramValues: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWGRPC SETVIEWS")
    vq.add_parameter(VistaQuery.LITERAL, paramName)
    vq.add_parameter(VistaQuery.LITERAL, permission)
    dhl = DictionaryHashList()
    for i, val in enumerate(paramValues):
        dhl.add(str(i + 1), val)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.post("/api/graph/delviews")
async def del_views(
    viewName: str = Query(...),
    permission: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWGRPC DELVIEWS")
    vq.add_parameter(VistaQuery.LITERAL, viewName)
    vq.add_parameter(VistaQuery.LITERAL, permission)
    return await session.s_query(vq)


@router.get("/api/graph/allviews")
async def all_views(
    vtype: str = Query(""),
    user: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC ALLVIEWS")
    vq.add_parameter(VistaQuery.LITERAL, vtype)
    vq.add_parameter(VistaQuery.LITERAL, user)
    return await session.t_query(vq)


# ── Sizing ──────────────────────────────────────────────────────────

@router.get("/api/graph/getsize")
async def get_size(
    paramName: str = Query(""),
    userx: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC GETSIZE")
    vq.add_parameter(VistaQuery.LITERAL, paramName)
    vq.add_parameter(VistaQuery.LITERAL, userx)
    return await session.t_query(vq)


@router.post("/api/graph/setsize")
async def set_size(
    paramName: str = Query(...),
    paramValues: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWGRPC SETSIZE")
    vq.add_parameter(VistaQuery.LITERAL, paramName)
    dhl = DictionaryHashList()
    for i, val in enumerate(paramValues):
        dhl.add(str(i + 1), val)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── Test Specification & Lookup ─────────────────────────────────────

@router.get("/api/graph/testspec")
async def test_spec(
    item: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC TESTSPEC")
    vq.add_parameter(VistaQuery.LITERAL, item)
    return await session.t_query(vq)


@router.get("/api/graph/testing")
async def testing(
    dfn: str = Query(...),
    item: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC TESTING")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, item)
    return await session.t_query(vq)


@router.get("/api/graph/lookup")
async def lookup(
    file: str = Query(...),
    startFrom: str = Query(...),
    direction: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWGRPC LOOKUP")
    vq.add_parameter(VistaQuery.LITERAL, file)
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)
