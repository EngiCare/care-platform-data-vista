# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Order lab router — mirrors OrderLabController.cs.

Lab order entry dialogs — ORWDLR32/33, ORWDXVB, ORCDLR2 RPCs.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Lab Dialog Defaults ───────────────────────────────────────────

@router.get("/api/orderlab/defaults")
async def od_for_lab(location: int = Query(...), division: int = Query(0), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDLR32 DEF")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, str(division))
    return await session.t_query(vq)


@router.get("/api/orderlab/loadtest")
async def load_lab_test_data(labTestIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDLR32 LOAD")
    vq.add_parameter(VistaQuery.LITERAL, labTestIen)
    return await session.t_query(vq)


# ── Samples & Specimens ──────────────────────────────────────────

@router.get("/api/orderlab/allsamples")
async def all_samples(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDLR32 ALLSAMP")
    return await session.t_query(vq)


@router.get("/api/orderlab/allspecimens")
async def all_specimens(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDLR32 ALLSPEC")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/orderlab/abbrevspecimens")
async def abbrev_specimens(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDLR32 ABBSPEC")
    return await session.t_query(vq)


@router.get("/api/orderlab/onesample")
async def one_sample(sampleIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDLR32 ONE SAMPLE")
    vq.add_parameter(VistaQuery.LITERAL, str(sampleIen))
    return await session.t_query(vq)


@router.get("/api/orderlab/onespecimen")
async def one_specimen(specimenIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDLR32 ONE SPECIMEN")
    vq.add_parameter(VistaQuery.LITERAL, str(specimenIen))
    return await session.s_query(vq)


# ── Collection Times / Scheduling ────────────────────────────────

@router.get("/api/orderlab/stopdate")
async def calc_stop_date(text: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDLR32 STOP")
    vq.add_parameter(VistaQuery.LITERAL, text)
    return await session.s_query(vq)


@router.get("/api/orderlab/maxdays")
async def max_days(location: int = Query(...), schedule: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDLR32 MAXDAYS")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, str(schedule))
    return await session.s_query(vq)


@router.get("/api/orderlab/iscollecttime")
async def is_lab_collect_time(dateTime: str = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDLR32 LAB COLL TIME")
    vq.add_parameter(VistaQuery.LITERAL, dateTime)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


@router.get("/api/orderlab/futurecollectdays")
async def future_collect_days(location: int = Query(...), division: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDLR33 FUTURE LAB COLLECTS")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, str(division))
    return await session.s_query(vq)


@router.get("/api/orderlab/immedcollecttimes")
async def immediate_collect_times(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDLR32 IMMED COLLECT")
    return await session.t_query(vq)


@router.get("/api/orderlab/defaultimmedcolltime")
async def default_immed_coll_time(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDLR32 IC DEFAULT")
    return await session.s_query(vq)


@router.get("/api/orderlab/validimmedcolltime")
async def valid_immed_coll_time(collTime: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDLR32 IC VALID")
    vq.add_parameter(VistaQuery.LITERAL, collTime)
    return await session.s_query(vq)


@router.get("/api/orderlab/labtimes")
async def get_lab_times(labDate: str = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDLR32 GET LAB TIMES")
    vq.add_parameter(VistaQuery.LITERAL, labDate)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.get("/api/orderlab/lastcollectiontime")
async def last_collection_time(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDLR33 LASTTIME")
    return await session.s_query(vq)


@router.get("/api/orderlab/lctowcinstructions")
async def lc_to_wc_instructions(location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDLR33 LC TO WC")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


# ── LC to WC Checks ──────────────────────────────────────────────

@router.post("/api/orderlab/checkonelargetowardcoll")
async def check_one_lc_to_wc(location: int = Query(...), startDate: str = Query(...), collType: str = Query(...), schedule: str = Query(...), duration: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORCDLR2 CHECK ONE LC TO WC")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, startDate)
    vq.add_parameter(VistaQuery.LITERAL, collType)
    vq.add_parameter(VistaQuery.LITERAL, schedule)
    vq.add_parameter(VistaQuery.LITERAL, duration)
    return await session.t_query(vq)


@router.post("/api/orderlab/checkalllargetowardcoll")
async def check_all_lc_to_wc(location: int = Query(...), orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORCDLR2 CHECK ALL LC TO WC")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


# ── Blood Bank ────────────────────────────────────────────────────

@router.get("/api/orderlab/bloodcomponents")
async def blood_components(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXVB COMPORD")
    return await session.t_query(vq)


@router.get("/api/orderlab/diagnostictests")
async def diagnostic_tests(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXVB3 DIAGORD")
    return await session.t_query(vq)


@router.get("/api/orderlab/nursadminsuppress")
async def nurs_admin_suppress(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXVB NURSADMN")
    return await session.s_query(vq)


@router.get("/api/orderlab/statallowed")
async def stat_allowed(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXVB STATALOW")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/orderlab/removecolldefault")
async def remove_coll_time_default(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXVB3 COLLTIM")
    return await session.s_query(vq)


@router.get("/api/orderlab/swappanel")
async def swap_panel_location(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXVB3 SWPANEL")
    return await session.s_query(vq)


@router.post("/api/orderlab/bloodresultsraw")
async def patient_blood_results_raw(dfn: str = Query(...), tests: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXVB RAW")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    dhl = DictionaryHashList()
    for i, t in enumerate(tests):
        dhl.add(str(i), t)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/orderlab/bloodresults")
async def patient_blood_results(dfn: str = Query(...), tests: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXVB RESULTS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    dhl = DictionaryHashList()
    for i, t in enumerate(tests):
        dhl.add(str(i), t)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/orderlab/patientbbinfo")
async def patient_bb_info(dfn: str = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXVB GETALL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.get("/api/orderlab/subtype")
async def get_sub_type(testName: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXVB SUBCHK")
    vq.add_parameter(VistaQuery.LITERAL, testName)
    return await session.s_query(vq)


@router.get("/api/orderlab/tnsdaysback")
async def tns_days_back(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXVB VBTNS")
    return await session.s_query(vq)
