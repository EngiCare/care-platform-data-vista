# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Problem router — mirrors ProblemController.cs.

Problem list CRUD, lexicon search, filters, categories, providers.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.models.problem import Problem, ProblemLexiconResult
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Problem List & Detail ───────────────────────────────────────────

@router.get("/api/problem/list")
async def problem_list(
    dfn: str = Query(...),
    status: str = Query(...),
    fmDate: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[Problem]:
    vq = VistaQuery("ORQQPL PROBLEM LIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, status)
    vq.add_parameter(VistaQuery.LITERAL, fmDate)
    results = await session.t_query(vq)
    return Problem.parse_list(results)


@router.get("/api/problem/detail")
async def detail(
    dfn: str = Query(...),
    ien: int = Query(...),
    dateTime: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, dfn + "^" + dateTime)
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.t_query(vq)


@router.get("/api/problem/comments")
async def comments(
    problemIfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL PROB COMMENTS")
    vq.add_parameter(VistaQuery.LITERAL, problemIfn)
    return await session.t_query(vq)


@router.get("/api/problem/audithistory")
async def audit_history(
    problemIfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL AUDIT HIST")
    vq.add_parameter(VistaQuery.LITERAL, problemIfn)
    return await session.t_query(vq)


# ── Problem CRUD ────────────────────────────────────────────────────

@router.get("/api/problem/editload")
async def edit_load(
    problemIfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL EDIT LOAD")
    vq.add_parameter(VistaQuery.LITERAL, problemIfn)
    return await session.t_query(vq)


@router.post("/api/problem/editsave")
async def edit_save(
    problemIfn: str = Query(...),
    providerId: int = Query(...),
    ptVamc: str = Query(...),
    primUser: str = Query(...),
    problemFile: list[str] = Body(...),
    searchString: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL EDIT SAVE")
    vq.add_parameter(VistaQuery.LITERAL, problemIfn)
    vq.add_parameter(VistaQuery.LITERAL, str(providerId))
    vq.add_parameter(VistaQuery.LITERAL, ptVamc)
    vq.add_parameter(VistaQuery.LITERAL, primUser)
    dhl = DictionaryHashList()
    for i, item in enumerate(problemFile):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, searchString)
    return await session.t_query(vq)


@router.post("/api/problem/addsave")
async def add_save(
    patientInfo: str = Query(...),
    providerId: int = Query(...),
    ptVamc: str = Query(...),
    problemFile: list[str] = Body(...),
    searchString: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL ADD SAVE")
    vq.add_parameter(VistaQuery.LITERAL, patientInfo)
    vq.add_parameter(VistaQuery.LITERAL, str(providerId))
    vq.add_parameter(VistaQuery.LITERAL, ptVamc)
    dhl = DictionaryHashList()
    for i, item in enumerate(problemFile):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, searchString)
    return await session.t_query(vq)


@router.post("/api/problem/delete")
async def delete_problem(
    problemIfn: str = Query(...),
    providerId: int = Query(...),
    ptVamc: str = Query(...),
    comment: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL DELETE")
    vq.add_parameter(VistaQuery.LITERAL, problemIfn)
    vq.add_parameter(VistaQuery.LITERAL, str(providerId))
    vq.add_parameter(VistaQuery.LITERAL, ptVamc)
    vq.add_parameter(VistaQuery.LITERAL, comment)
    return await session.t_query(vq)


@router.post("/api/problem/update")
async def update(
    altProbFile: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL UPDATE")
    dhl = DictionaryHashList()
    for i, item in enumerate(altProbFile):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/problem/verify")
async def verify(
    problemIfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL VERIFY")
    vq.add_parameter(VistaQuery.LITERAL, problemIfn)
    return await session.t_query(vq)


@router.post("/api/problem/replace")
async def replace_problem(
    problemIfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL REPLACE")
    vq.add_parameter(VistaQuery.LITERAL, problemIfn)
    return await session.t_query(vq)


@router.get("/api/problem/checkduplicate")
async def check_duplicate(
    dfn: str = Query(...),
    termIen: str = Query(...),
    termText: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORQQPL CHECK DUP")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, termIen)
    vq.add_parameter(VistaQuery.LITERAL, termText)
    return await session.s_query(vq)


# ── Initialization & Preferences ───────────────────────────────────

@router.get("/api/problem/initpatient")
async def init_patient(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL INIT PT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/problem/inituser")
async def init_user(
    providerId: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL INIT USER")
    vq.add_parameter(VistaQuery.LITERAL, str(providerId))
    return await session.t_query(vq)


@router.post("/api/problem/saveviewpreferences")
async def save_view_preferences(
    viewPref: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORQQPL SAVEVIEW")
    vq.add_parameter(VistaQuery.LITERAL, viewPref)
    return await session.s_query(vq)


# ── Lexicon & Search ───────────────────────────────────────────────

@router.get("/api/problem/lexiconsearch")
async def lexicon_search(
    searchFor: str = Query(...),
    view: str = Query(...),
    fmDate: str = Query(""),
    extend: bool = Query(True),
    session: ISession = Depends(get_current_session),
) -> list[ProblemLexiconResult]:
    vq = VistaQuery("ORQQPL4 LEX")
    vq.add_parameter(VistaQuery.LITERAL, searchFor)
    vq.add_parameter(VistaQuery.LITERAL, view)
    vq.add_parameter(VistaQuery.LITERAL, fmDate)
    vq.add_parameter(VistaQuery.LITERAL, "1" if extend else "0")
    results = await session.t_query(vq)
    return ProblemLexiconResult.parse_list(results)


@router.post("/api/problem/ntrtbulletin")
async def ntrt_bulletin(
    term: str = Query(...),
    providerId: str = Query(...),
    patientId: str = Query(...),
    comment: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORQQPL PROBLEM NTRT BULLETIN")
    vq.add_parameter(VistaQuery.LITERAL, term)
    vq.add_parameter(VistaQuery.LITERAL, providerId)
    vq.add_parameter(VistaQuery.LITERAL, patientId)
    vq.add_parameter(VistaQuery.LITERAL, comment)
    return await session.s_query(vq)


# ── Filters & Providers ────────────────────────────────────────────

@router.get("/api/problem/patientproviders")
async def patient_providers(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQPT PATIENT TEAM PROVIDERS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/problem/providerlist")
async def provider_list(
    flag: str = Query(...),
    number: int = Query(...),
    from_: str = Query(..., alias="from"),
    part: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL PROVIDER LIST")
    vq.add_parameter(VistaQuery.LITERAL, flag)
    vq.add_parameter(VistaQuery.LITERAL, str(number))
    vq.add_parameter(VistaQuery.LITERAL, from_)
    vq.add_parameter(VistaQuery.LITERAL, part)
    return await session.t_query(vq)


@router.post("/api/problem/providerfilterlist")
async def provider_filter_list(
    provList: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL PROV FILTER LIST")
    dhl = DictionaryHashList()
    for i, item in enumerate(provList):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/problem/clinicfilterlist")
async def clinic_filter_list(
    locList: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL CLIN FILTER LIST")
    dhl = DictionaryHashList()
    for i, item in enumerate(locList):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/problem/servicefilterlist")
async def service_filter_list(
    locList: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL SERV FILTER LIST")
    dhl = DictionaryHashList()
    for i, item in enumerate(locList):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/problem/clinicsearch")
async def clinic_search(
    searchArg: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL CLIN SRCH")
    vq.add_parameter(VistaQuery.LITERAL, searchArg)
    return await session.t_query(vq)


@router.get("/api/problem/servicesearch")
async def service_search(
    startFrom: str = Query(...),
    direction: int = Query(...),
    all: bool = Query(False),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL SRVC SRCH")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    vq.add_parameter(VistaQuery.LITERAL, "1" if all else "0")
    return await session.t_query(vq)


# ── User Problem Categories ───────────────────────────────────────

@router.get("/api/problem/usercategories")
async def user_categories(
    provider: int = Query(...),
    location: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL USER PROB CATS")
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.get("/api/problem/userproblemlist")
async def user_problem_list(
    categoryIen: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQPL USER PROB LIST")
    vq.add_parameter(VistaQuery.LITERAL, categoryIen)
    return await session.t_query(vq)
