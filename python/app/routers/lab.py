# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Lab results router — mirrors LabResultController.cs.

Atomics, specimens, tests, test groups, cumulative/interim/micro results,
worksheet, chart, reports, printing, remote lab, convenience endpoints.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Lookups (Tests, Specimens, Users) ───────────────────────────────

@router.get("/api/lab/atomics")
async def atomics(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLRR ATOMICS")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/lab/specimens")
async def specimens(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLRR SPEC")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/lab/alltests")
async def all_tests(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLRR ALLTESTS")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/lab/chemtests")
async def chem_tests(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLRR CHEMTEST")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/lab/users")
async def users(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLRR USERS")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/lab/specimendefaults")
async def specimen_defaults(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWLRR PARAM")
    return await session.s_query(vq)


@router.get("/api/lab/testinfo")
async def test_info(test: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLRR INFO")
    vq.add_parameter(VistaQuery.LITERAL, test)
    return await session.t_query(vq)


# ── Test Groups ─────────────────────────────────────────────────────

@router.get("/api/lab/testgroups")
async def test_groups(user: int = Query(0), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLRR TG")
    vq.add_parameter(VistaQuery.LITERAL, str(user))
    return await session.t_query(vq)


@router.get("/api/lab/atest")
async def a_test(test: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLRR ATESTS")
    vq.add_parameter(VistaQuery.LITERAL, str(test))
    return await session.t_query(vq)


@router.get("/api/lab/atestgroup")
async def a_test_group(testGroup: int = Query(...), user: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLRR ATG")
    vq.add_parameter(VistaQuery.LITERAL, str(testGroup))
    vq.add_parameter(VistaQuery.LITERAL, str(user))
    return await session.t_query(vq)


@router.post("/api/lab/testgroup/add")
async def test_group_add(tests: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWLRR UTGA")
    dhl = DictionaryHashList()
    for i, t in enumerate(tests):
        dhl.add(str(i), t)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.post("/api/lab/testgroup/replace")
async def test_group_replace(tests: list[str] = Body(...), testGroup: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWLRR UTGR")
    dhl = DictionaryHashList()
    for i, t in enumerate(tests):
        dhl.add(str(i), t)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, str(testGroup))
    return await session.s_query(vq)


@router.post("/api/lab/testgroup/delete")
async def test_group_delete(testGroup: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWLRR UTGD")
    vq.add_parameter(VistaQuery.LITERAL, str(testGroup))
    return await session.s_query(vq)


# ── Results (Cumulative, Interim, Micro) ────────────────────────────

@router.get("/api/lab/cumulative")
async def cumulative(
    dfn: str = Query(...),
    daysBack: int = Query(365),
    date1: str = Query(""),
    date2: str = Query(""),
    rpc: str = Query("ORWLR CUMULATIVE REPORT"),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery(rpc)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(daysBack))
    vq.add_parameter(VistaQuery.LITERAL, date1)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    return await session.t_query(vq)


@router.get("/api/lab/interim")
async def interim(
    dfn: str = Query(...),
    date1: str = Query(""),
    date2: str = Query(""),
    rpc: str = Query("ORWLR INTERIM REPORT"),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery(rpc)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    return await session.t_query(vq)


@router.post("/api/lab/interimselect")
async def interim_select(
    dfn: str = Query(...),
    date1: str = Query(...),
    date2: str = Query(...),
    tests: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWLRR INTERIMS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    dhl = DictionaryHashList()
    for i, t in enumerate(tests):
        dhl.add(str(i), t)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/lab/interimgrid")
async def interim_grid(
    dfn: str = Query(...),
    date1: str = Query(...),
    direction: int = Query(...),
    format: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWLRR INTERIMG")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    vq.add_parameter(VistaQuery.LITERAL, str(format))
    return await session.t_query(vq)


@router.get("/api/lab/micro")
async def micro(
    dfn: str = Query(...),
    date1: str = Query(""),
    date2: str = Query(""),
    rpc: str = Query("ORWLR MICRO REPORT"),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery(rpc)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    return await session.t_query(vq)


# ── Worksheet & Chart ───────────────────────────────────────────────

@router.post("/api/lab/worksheet")
async def worksheet(
    dfn: str = Query(...),
    date1: str = Query(...),
    date2: str = Query(...),
    spec: str = Query(...),
    tests: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWLRR GRID")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    vq.add_parameter(VistaQuery.LITERAL, spec)
    dhl = DictionaryHashList()
    for i, t in enumerate(tests):
        dhl.add(str(i), t)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/lab/chart")
async def chart(
    dfn: str = Query(...),
    date1: str = Query(...),
    date2: str = Query(...),
    spec: str = Query(...),
    test: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWLRR CHART")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    vq.add_parameter(VistaQuery.LITERAL, spec)
    vq.add_parameter(VistaQuery.LITERAL, test)
    return await session.t_query(vq)


@router.get("/api/lab/daterange")
async def date_range(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWLRR NEWOLD")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


# ── Reports & Printing ─────────────────────────────────────────────

@router.get("/api/lab/report")
async def report(
    dfn: str = Query(...),
    reportId: str = Query(...),
    hsType: str = Query(...),
    date: str = Query(...),
    section: str = Query(...),
    date1: str = Query(...),
    date2: str = Query(...),
    rpc: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery(rpc)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, reportId)
    vq.add_parameter(VistaQuery.LITERAL, hsType)
    vq.add_parameter(VistaQuery.LITERAL, date)
    vq.add_parameter(VistaQuery.LITERAL, section)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    return await session.t_query(vq)


@router.post("/api/lab/print")
async def print_report(
    device: str = Query(...),
    dfn: str = Query(...),
    report: str = Query(...),
    daysBack: int = Query(...),
    tests: list[str] = Body(...),
    date1: str = Query(""),
    date2: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWRP PRINT LAB REPORTS")
    vq.add_parameter(VistaQuery.LITERAL, device)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, report)
    vq.add_parameter(VistaQuery.LITERAL, str(daysBack))
    dhl = DictionaryHashList()
    for i, t in enumerate(tests):
        dhl.add(str(i), t)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    return await session.s_query(vq)


@router.post("/api/lab/formattedreport")
async def formatted_report(
    dfn: str = Query(...),
    report: str = Query(...),
    daysBack: int = Query(...),
    tests: list[str] = Body(...),
    date1: str = Query(""),
    date2: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWRP WINPRINT LAB REPORTS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, report)
    vq.add_parameter(VistaQuery.LITERAL, str(daysBack))
    dhl = DictionaryHashList()
    for i, t in enumerate(tests):
        dhl.add(str(i), t)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    return await session.t_query(vq)


@router.get("/api/lab/useradiobuttons")
async def use_radio_buttons(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWRP1A RADIO")
    return await session.s_query(vq)


# ── Remote Lab ──────────────────────────────────────────────────────

@router.get("/api/lab/remote/cumulative")
async def remote_cumulative(
    dfn: str = Query(...),
    daysBack: int = Query(...),
    date1: str = Query(...),
    date2: str = Query(...),
    site: str = Query(...),
    remoteRpc: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("XWB REMOTE RPC")
    vq.add_parameter(VistaQuery.LITERAL, site)
    vq.add_parameter(VistaQuery.LITERAL, remoteRpc)
    vq.add_parameter(VistaQuery.LITERAL, "0")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(daysBack))
    vq.add_parameter(VistaQuery.LITERAL, date1)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    return await session.t_query(vq)


@router.get("/api/lab/remote/interim")
async def remote_interim(
    dfn: str = Query(...),
    date1: str = Query(...),
    date2: str = Query(...),
    site: str = Query(...),
    remoteRpc: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("XWB REMOTE RPC")
    vq.add_parameter(VistaQuery.LITERAL, site)
    vq.add_parameter(VistaQuery.LITERAL, remoteRpc)
    vq.add_parameter(VistaQuery.LITERAL, "0")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    return await session.t_query(vq)


@router.get("/api/lab/remote/report")
async def remote_report(
    dfn: str = Query(...),
    reportId: str = Query(...),
    hsType: str = Query(...),
    date: str = Query(...),
    section: str = Query(...),
    date1: str = Query(...),
    date2: str = Query(...),
    site: str = Query(...),
    remoteRpc: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("XWB REMOTE RPC")
    vq.add_parameter(VistaQuery.LITERAL, site)
    vq.add_parameter(VistaQuery.LITERAL, remoteRpc)
    vq.add_parameter(VistaQuery.LITERAL, "0")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, reportId + ";1")
    vq.add_parameter(VistaQuery.LITERAL, hsType)
    vq.add_parameter(VistaQuery.LITERAL, date)
    vq.add_parameter(VistaQuery.LITERAL, section)
    vq.add_parameter(VistaQuery.LITERAL, date2)
    vq.add_parameter(VistaQuery.LITERAL, date1)
    return await session.t_query(vq)


@router.post("/api/lab/remote/print")
async def remote_print(
    device: str = Query(...),
    dfn: str = Query(...),
    report: str = Query(...),
    handles: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWRP PRINT LAB REMOTE")
    vq.add_parameter(VistaQuery.LITERAL, device)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, report)
    dhl = DictionaryHashList()
    for i, h in enumerate(handles):
        dhl.add(str(i), h)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.post("/api/lab/remote/formattedreport")
async def remote_formatted_report(
    dfn: str = Query(...),
    report: str = Query(...),
    handles: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWRP PRINT WINDOWS LAB REMOTE")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, report)
    dhl = DictionaryHashList()
    for i, h in enumerate(handles):
        dhl.add(str(i), h)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


# ── Cover Sheet — Recent Labs ──────────────────────────────────────

@router.get("/api/lab/recent")
async def recent(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWCV LAB")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


# ── Simple convenience endpoints ───────────────────────────────────

@router.get("/api/lab/graphdata")
async def graph_data(dfn: str = Query(...), test: str = Query(""), daysBack: int = Query(365), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLR GRAPH DATA")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, test)
    vq.add_parameter(VistaQuery.LITERAL, str(daysBack))
    return await session.t_query(vq)


@router.get("/api/lab/worksheet")
async def ws_worksheet(dfn: str = Query(...), daysBack: int = Query(365), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLR WORKSHEET")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(daysBack))
    return await session.t_query(vq)


@router.get("/api/lab/refranges")
async def ref_ranges(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLR REF RANGES")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/lab/categories")
async def categories(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLR CUMULATIVE CATEGORIES")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/lab/resultdetail")
async def result_detail(dfn: str = Query(...), test: str = Query(""), date: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLR RESULT DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, test)
    vq.add_parameter(VistaQuery.LITERAL, date)
    return await session.t_query(vq)


@router.get("/api/lab/testgroupnames")
async def test_group_names(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLR TEST GROUP NAMES")
    return await session.t_query(vq)


@router.get("/api/lab/grouptests")
async def group_tests(name: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLR GROUP TESTS")
    vq.add_parameter(VistaQuery.LITERAL, name)
    return await session.t_query(vq)


@router.post("/api/lab/savetestgroup")
async def save_test_group(groupName: str = Query(""), tests: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWLR SAVE TEST GROUP")
    vq.add_parameter(VistaQuery.LITERAL, groupName)
    vq.add_parameter(VistaQuery.LITERAL, tests)
    return await session.s_query(vq)


@router.post("/api/lab/deletetestgroup")
async def ws_delete_test_group(name: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWLR DELETE TEST GROUP")
    vq.add_parameter(VistaQuery.LITERAL, name)
    return await session.s_query(vq)
