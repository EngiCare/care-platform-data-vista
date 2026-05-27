# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Consult router — mirrors ConsultController.cs.

Consults & Procedures — listing, detail, actions, services, titles, medicine results, SF513.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.models.consult import Consult
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Listing / Detail ────────────────────────────────────────────────

@router.get("/api/consult/list")
async def list_consults(
    dfn: str = Query(...),
    early: str = Query(...),
    late: str = Query(...),
    service: str = Query(...),
    status: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[Consult]:
    vq = VistaQuery("ORQQCN LIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, early)
    vq.add_parameter(VistaQuery.LITERAL, late)
    vq.add_parameter(VistaQuery.LITERAL, service)
    vq.add_parameter(VistaQuery.LITERAL, status)
    results = await session.t_query(vq)
    return Consult.parse_list(results)


@router.get("/api/consult/detail")
async def load_consult_detail(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.t_query(vq)


@router.get("/api/consult/get")
async def get_consult_rec(
    ien: int = Query(...),
    showAddenda: bool = Query(False),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN GET CONSULT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, "1" if showAddenda else "0")
    return await session.t_query(vq)


@router.get("/api/consult/linkednotes")
async def get_linked_notes(consultIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN GET CONSULT")
    vq.add_parameter(VistaQuery.LITERAL, consultIen)
    vq.add_parameter(VistaQuery.LITERAL, "1")
    lines = await session.t_query(vq)
    if not lines:
        return []
    first_piece = lines[0].split("^")[0]
    if first_piece == "-1":
        return []
    tiu_docs: list[str] = []
    for line in lines[1:]:
        piece1 = line.split("^")[0]
        parts = piece1.split(";")
        if len(parts) >= 2 and parts[1].upper().startswith("MCAR"):
            continue
        tiu_docs.append(line)
    return tiu_docs


@router.get("/api/consult/find")
async def find_consult(consultIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN FIND CONSULT")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    return await session.s_query(vq)


@router.get("/api/consult/unresolved")
async def unresolved_consults(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN UNRESOLVED")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


# ── Services / Lookups ──────────────────────────────────────────────

@router.get("/api/consult/statuses")
async def subset_of_statuses(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN STATUS")
    return await session.t_query(vq)


@router.get("/api/consult/urgencies")
async def subset_of_urgencies(consultIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN URGENCIES")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    return await session.t_query(vq)


@router.get("/api/consult/servicetree")
async def load_service_tree(purpose: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN SVCTREE")
    vq.add_parameter(VistaQuery.LITERAL, str(purpose))
    return await session.t_query(vq)


@router.get("/api/consult/serviceswithsynonyms")
async def load_services_with_synonyms(
    startService: int = Query(...),
    purpose: int = Query(...),
    showSynonyms: bool = Query(True),
    consultIen: int = Query(-1),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN SVC W/SYNONYMS")
    vq.add_parameter(VistaQuery.LITERAL, str(startService))
    vq.add_parameter(VistaQuery.LITERAL, str(purpose))
    vq.add_parameter(VistaQuery.LITERAL, "1" if showSynonyms else "0")
    if consultIen >= 0:
        vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    return await session.t_query(vq)


@router.get("/api/consult/servicelist")
async def subset_of_services(
    startFrom: str = Query(...),
    direction: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN SVCLIST")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/consult/serviceien")
async def get_service_ien(orderIen: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN GET SERVICE IEN")
    vq.add_parameter(VistaQuery.LITERAL, orderIen)
    return await session.s_query(vq)


@router.get("/api/consult/procien")
async def get_proc_ien(orderIen: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN GET PROC IEN")
    vq.add_parameter(VistaQuery.LITERAL, orderIen)
    return await session.s_query(vq)


@router.get("/api/consult/procsvcs")
async def get_proc_svcs(procIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN GET PROC SVCS")
    vq.add_parameter(VistaQuery.LITERAL, str(procIen))
    return await session.t_query(vq)


@router.get("/api/consult/ordernumber")
async def get_order_number(consultIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN GET ORDER NUMBER")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    return await session.s_query(vq)


@router.get("/api/consult/provdx")
async def get_prov_dx(svcIen: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN PROVDX")
    vq.add_parameter(VistaQuery.LITERAL, svcIen)
    return await session.s_query(vq)


@router.get("/api/consult/isprosvc")
async def is_prosthetics_svc(svcIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN ISPROSVC")
    vq.add_parameter(VistaQuery.LITERAL, str(svcIen))
    return await session.s_query(vq)


@router.get("/api/consult/userauth")
async def get_user_auth(serviceIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN GET USER AUTH")
    vq.add_parameter(VistaQuery.LITERAL, str(serviceIen))
    return await session.s_query(vq)


# ── Titles ──────────────────────────────────────────────────────────

@router.get("/api/consult/identifyconsultsclass")
async def identify_consults_class(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU IDENTIFY CONSULTS CLASS")
    return await session.s_query(vq)


@router.get("/api/consult/identifyclinprocclass")
async def identify_clinproc_class(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU IDENTIFY CLINPROC CLASS")
    return await session.s_query(vq)


@router.get("/api/consult/consulttitles")
async def subset_of_consult_titles(
    startFrom: str = Query(...),
    direction: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU LONG LIST CONSULT TITLES")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/consult/clinproctitles")
async def subset_of_clinproc_titles(
    startFrom: str = Query(...),
    direction: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU LONG LIST CLINPROC TITLES")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


# ── Order Dialog ────────────────────────────────────────────────────

@router.get("/api/consult/oddefconsult")
async def od_for_consults(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDCN32 DEF")
    vq.add_parameter(VistaQuery.LITERAL, "C")
    return await session.t_query(vq)


@router.get("/api/consult/oddefprocedure")
async def od_for_procedures(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDCN32 DEF")
    vq.add_parameter(VistaQuery.LITERAL, "P")
    return await session.t_query(vq)


@router.get("/api/consult/procedures")
async def subset_of_procedures(
    startFrom: str = Query(...),
    direction: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWDCN32 PROCEDURES")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/consult/ordermsg")
async def consult_message(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDCN32 ORDRMSG")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.get("/api/consult/newdlg")
async def get_new_dialog(
    orderType: str = Query(...),
    location: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWDCN32 NEWDLG")
    vq.add_parameter(VistaQuery.LITERAL, orderType)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


@router.get("/api/consult/defaultreason")
async def get_default_reason(
    service: str = Query(...),
    dfn: str = Query(...),
    resolve: bool = Query(False),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN DEFAULT REQUEST REASON")
    vq.add_parameter(VistaQuery.LITERAL, service)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, "1" if resolve else "0")
    return await session.t_query(vq)


@router.get("/api/consult/editdefaultreason")
async def edit_default_reason(service: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN EDIT DEFAULT REASON")
    vq.add_parameter(VistaQuery.LITERAL, service)
    return await session.s_query(vq)


@router.get("/api/consult/prerequisite")
async def get_prerequisites(
    service: str = Query(...),
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN PREREQ CHK")
    vq.add_parameter(VistaQuery.LITERAL, service)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


# ── Actions ─────────────────────────────────────────────────────────

@router.get("/api/consult/actionmenus")
async def get_action_menus(consultIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN SET ACT MENUS")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    return await session.s_query(vq)


@router.post("/api/consult/receive")
async def receive_consult(
    ien: int = Query(...),
    receivedBy: int = Query(...),
    rcptDate: str = Query(...),
    comments: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN RECEIVE")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, str(receivedBy))
    vq.add_parameter(VistaQuery.LITERAL, rcptDate)
    dhl = DictionaryHashList()
    for i, c in enumerate(comments):
        dhl.add(str(i), c)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/consult/schedule")
async def schedule_consult(
    ien: int = Query(...),
    scheduledBy: int = Query(...),
    schdDate: str = Query(...),
    alert: int = Query(...),
    alertTo: str = Query(...),
    comments: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN2 SCHEDULE CONSULT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, str(scheduledBy))
    vq.add_parameter(VistaQuery.LITERAL, schdDate)
    vq.add_parameter(VistaQuery.LITERAL, str(alert))
    vq.add_parameter(VistaQuery.LITERAL, alertTo)
    dhl = DictionaryHashList()
    for i, c in enumerate(comments):
        dhl.add(str(i), c)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/consult/deny")
async def deny_consult(
    ien: int = Query(...),
    deniedBy: int = Query(...),
    denialDate: str = Query(...),
    comments: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN DISCONTINUE")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, str(deniedBy))
    vq.add_parameter(VistaQuery.LITERAL, denialDate)
    vq.add_parameter(VistaQuery.LITERAL, "DY")
    dhl = DictionaryHashList()
    for i, c in enumerate(comments):
        dhl.add(str(i), c)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/consult/discontinue")
async def discontinue_consult(
    ien: int = Query(...),
    discontinuedBy: int = Query(...),
    discontinueDate: str = Query(...),
    comments: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN DISCONTINUE")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, str(discontinuedBy))
    vq.add_parameter(VistaQuery.LITERAL, discontinueDate)
    vq.add_parameter(VistaQuery.LITERAL, "DC")
    dhl = DictionaryHashList()
    for i, c in enumerate(comments):
        dhl.add(str(i), c)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/consult/forward")
async def forward_consult(
    ien: int = Query(...),
    toService: int = Query(...),
    forwarder: int = Query(...),
    attentionOf: int = Query(...),
    urgency: int = Query(...),
    actionDate: str = Query(...),
    comments: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN FORWARD")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, str(toService))
    vq.add_parameter(VistaQuery.LITERAL, str(forwarder))
    vq.add_parameter(VistaQuery.LITERAL, str(attentionOf))
    vq.add_parameter(VistaQuery.LITERAL, str(urgency))
    vq.add_parameter(VistaQuery.LITERAL, actionDate)
    dhl = DictionaryHashList()
    for i, c in enumerate(comments):
        dhl.add(str(i), c)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/consult/addcomment")
async def add_comment(
    ien: int = Query(...),
    alert: int = Query(...),
    alertTo: str = Query(...),
    actionDate: str = Query(...),
    comments: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN ADDCMT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    dhl = DictionaryHashList()
    for i, c in enumerate(comments):
        dhl.add(str(i), c)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, str(alert))
    vq.add_parameter(VistaQuery.LITERAL, alertTo)
    vq.add_parameter(VistaQuery.LITERAL, actionDate)
    return await session.t_query(vq)


@router.post("/api/consult/admincomplete")
async def admin_complete(
    ien: int = Query(...),
    sigFindings: str = Query(...),
    respProv: int = Query(...),
    alert: int = Query(...),
    alertTo: str = Query(...),
    actionDate: str = Query(...),
    comments: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN ADMIN COMPLETE")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, sigFindings)
    dhl = DictionaryHashList()
    for i, c in enumerate(comments):
        dhl.add(str(i), c)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, str(respProv))
    vq.add_parameter(VistaQuery.LITERAL, str(alert))
    vq.add_parameter(VistaQuery.LITERAL, alertTo)
    vq.add_parameter(VistaQuery.LITERAL, actionDate)
    return await session.t_query(vq)


@router.post("/api/consult/sigfindings")
async def sig_findings(
    ien: int = Query(...),
    sigFindingsFlag: str = Query(...),
    alert: int = Query(...),
    alertTo: str = Query(...),
    actionDate: str = Query(...),
    comments: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN SIGFIND")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, sigFindingsFlag)
    dhl = DictionaryHashList()
    for i, c in enumerate(comments):
        dhl.add(str(i), c)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, str(alert))
    vq.add_parameter(VistaQuery.LITERAL, alertTo)
    vq.add_parameter(VistaQuery.LITERAL, actionDate)
    return await session.t_query(vq)


# ── Medicine Results ────────────────────────────────────────────────

@router.get("/api/consult/medresults")
async def display_med_results(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN MED RESULTS")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.t_query(vq)


@router.get("/api/consult/assignablemedresults")
async def get_assignable_med_results(consultIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN ASSIGNABLE MED RESULTS")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    return await session.t_query(vq)


@router.get("/api/consult/removablemedresults")
async def get_removable_med_results(consultIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN REMOVABLE MED RESULTS")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    return await session.t_query(vq)


@router.get("/api/consult/medresultdetails")
async def get_med_result_details(resultId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN MED RESULT DETAILS")
    vq.add_parameter(VistaQuery.LITERAL, resultId)
    return await session.t_query(vq)


@router.post("/api/consult/attachmedresult")
async def attach_med_result(
    consultIen: int = Query(...),
    resultId: str = Query(...),
    dateTime: str = Query(...),
    responsiblePerson: int = Query(...),
    alertTo: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORQQCN ATTACH MED RESULTS")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    vq.add_parameter(VistaQuery.LITERAL, resultId)
    vq.add_parameter(VistaQuery.LITERAL, dateTime)
    vq.add_parameter(VistaQuery.LITERAL, str(responsiblePerson))
    vq.add_parameter(VistaQuery.LITERAL, alertTo)
    return await session.s_query(vq)


@router.post("/api/consult/removemedresult")
async def remove_med_result(
    consultIen: int = Query(...),
    resultId: str = Query(...),
    dateTime: str = Query(...),
    responsiblePerson: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORQQCN REMOVE MED RESULTS")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    vq.add_parameter(VistaQuery.LITERAL, resultId)
    vq.add_parameter(VistaQuery.LITERAL, dateTime)
    vq.add_parameter(VistaQuery.LITERAL, str(responsiblePerson))
    return await session.s_query(vq)


# ── Edit / Resubmit ─────────────────────────────────────────────────

@router.get("/api/consult/canedit")
async def can_edit(consultIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN CANEDIT")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    return await session.s_query(vq)


@router.get("/api/consult/loadforedit")
async def load_for_edit(consultIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN LOAD FOR EDIT")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    return await session.t_query(vq)


@router.post("/api/consult/resubmit")
async def resubmit(
    ien: int = Query(...),
    editData: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORQQCN RESUBMIT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    dhl = DictionaryHashList()
    for i, d in enumerate(editData):
        dhl.add(str(i), d)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── SF513 / Print ──────────────────────────────────────────────────

@router.get("/api/consult/showsf513")
async def show_sf513(consultIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN SHOW SF513")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    return await session.t_query(vq)


@router.get("/api/consult/printsf513")
async def print_sf513(
    consultIen: int = Query(...),
    chartCopy: str = Query(...),
    device: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORQQCN PRINT SF513")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    vq.add_parameter(VistaQuery.LITERAL, chartCopy)
    vq.add_parameter(VistaQuery.LITERAL, device)
    return await session.s_query(vq)


@router.get("/api/consult/windowsprintsf513")
async def windows_print_sf513(
    consultIen: int = Query(...),
    chartCopy: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQQCN PRINT SF513")
    vq.add_parameter(VistaQuery.LITERAL, str(consultIen))
    vq.add_parameter(VistaQuery.LITERAL, chartCopy)
    return await session.t_query(vq)


# ── Context ─────────────────────────────────────────────────────────

@router.get("/api/consult/context")
async def get_context(userDuz: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN2 GET CONTEXT")
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    return await session.s_query(vq)


@router.post("/api/consult/context")
async def save_context(context: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN2 SAVE CONTEXT")
    vq.add_parameter(VistaQuery.LITERAL, context)
    return await session.s_query(vq)


@router.get("/api/consult/savedcpfields")
async def saved_cp_fields(noteIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTIU GET SAVED CP FIELDS")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.s_query(vq)


# ── Web.CPRS Convenience Endpoints ─────────────────────────────────

@router.get("/api/consult/services")
async def services(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQCN SVC W/SYNONYMS")
    return await session.t_query(vq)


@router.post("/api/consult/create")
async def create_consult(service: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN RECEIVE")
    vq.add_parameter(VistaQuery.LITERAL, service or "")
    return await session.s_query(vq)


@router.post("/api/consult/addresult")
async def add_result(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN SET ACT MENUS")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    return await session.s_query(vq)


@router.post("/api/consult/comment")
async def comment(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN ADDCMT")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    return await session.s_query(vq)


@router.post("/api/consult/ws/receive")
async def simple_receive(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN RECEIVE")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    return await session.s_query(vq)


@router.post("/api/consult/ws/schedule")
async def simple_schedule(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN RECEIVE")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    return await session.s_query(vq)


@router.post("/api/consult/ws/deny")
async def simple_deny(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN DISCONTINUE")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    return await session.s_query(vq)


@router.post("/api/consult/ws/discontinue")
async def simple_discontinue(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN DISCONTINUE")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    return await session.s_query(vq)


@router.post("/api/consult/ws/forward")
async def simple_forward(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN FORWARD")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    return await session.s_query(vq)


@router.post("/api/consult/ws/admincomplete")
async def simple_admin_complete(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN ADMIN COMPLETE")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    return await session.s_query(vq)


@router.post("/api/consult/ws/resubmit")
async def simple_resubmit(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN RESUBMIT")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    return await session.s_query(vq)
