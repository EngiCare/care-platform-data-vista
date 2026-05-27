# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Report router — mirrors ReportController.cs.

Report lists, column headers, text retrieval, printing,
health summary components, imaging exams, patient procedures, nutrition,
remote-data access (CIRN / HDR / XWB).
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.filemandate import to_fm_range
from app.models.report import ReportDefinition
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Helpers ────────────────────────────────────────────────────────

def _extract_section(lines: list[str], section: str) -> list[str]:
    """Extract a section from multi-section ORWRP REPORT LISTS response."""
    result: list[str] = []
    in_section = False
    for line in lines:
        if line == section:
            in_section = True
            continue
        if line == "$$END":
            if in_section:
                break
            continue
        if in_section:
            result.append(line)
    return result


async def _stamp_qualifier_types(tree: list[ReportDefinition], session: ISession) -> None:
    """Stamp QualifierType onto every node from ORWRP REPORT LISTS."""
    try:
        vq = VistaQuery("ORWRP REPORT LISTS")
        rows = await session.t_query(vq)
        section = _extract_section(rows, "[REPORT LIST]")
        qt_map: dict[str, int] = {}
        for line in section:
            if not line or not line.strip():
                continue
            p = line.split("^")
            if len(p) < 3:
                continue
            ifn = p[0]
            try:
                qt = int(p[2])
            except ValueError:
                continue
            qt_map[ifn] = qt
        _apply_qualifier_types(tree, qt_map)
    except Exception:
        pass


def _apply_qualifier_types(nodes: list[ReportDefinition], qt_map: dict[str, int]) -> None:
    for n in nodes:
        if n.ifn and n.ifn in qt_map:
            n.qualifier_type = qt_map[n.ifn]
        if n.children:
            _apply_qualifier_types(n.children, qt_map)


# ── Report Lists ──────────────────────────────────────────────────

@router.get("/api/report/lists")
async def report_lists(session: ISession = Depends(get_current_session)) -> list[ReportDefinition]:
    vq = VistaQuery("ORWRP3 EXPAND COLUMNS")
    vq.add_parameter(VistaQuery.LITERAL, "REPORTS")
    results = await session.t_query(vq)
    tree = ReportDefinition.parse_tree_from_expand_columns(results)
    await _stamp_qualifier_types(tree, session)
    return tree


@router.get("/api/report/labtree")
async def lab_report_tree(session: ISession = Depends(get_current_session)) -> list[ReportDefinition]:
    vq = VistaQuery("ORWRP3 EXPAND COLUMNS")
    vq.add_parameter(VistaQuery.LITERAL, "LABS")
    results = await session.t_query(vq)
    tree = ReportDefinition.parse_tree_from_expand_columns(results)
    await _stamp_qualifier_types(tree, session)
    return tree


@router.get("/api/report/dateranges")
async def date_ranges(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP REPORT LISTS")
    results = await session.t_query(vq)
    return _extract_section(results, "[DATE RANGES]")


@router.get("/api/report/hstypes")
async def health_summary_types(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP REPORT LISTS")
    results = await session.t_query(vq)
    return _extract_section(results, "[HEALTH SUMMARY TYPES]")


@router.get("/api/report/lablists")
async def lab_report_lists(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP LAB REPORT LISTS")
    return await session.t_query(vq)


@router.get("/api/report/columnheaders")
async def column_headers(reportId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP COLUMN HEADERS")
    vq.add_parameter(VistaQuery.LITERAL, reportId)
    return await session.t_query(vq)


@router.get("/api/report/expandcolumns")
async def expand_columns(dfn: str = Query(...), reportId: str = Query(...), item: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP3 EXPAND COLUMNS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, reportId)
    vq.add_parameter(VistaQuery.LITERAL, item)
    return await session.t_query(vq)


@router.post("/api/report/savecol")
async def save_columns(column: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCH SAVECOL")
    vq.add_parameter(VistaQuery.LITERAL, column or "")
    return await session.s_query(vq)


@router.get("/api/report/nutrition")
async def nutrition_list(dfn: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP1 LISTNUTR")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/report/surgerylist")
async def surgery_report_list(dfn: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWSR RPTLIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


# ── Consult Reports ───────────────────────────────────────────────

@router.get("/api/report/consultlist")
async def consult_report_list(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWCS LIST OF CONSULT REPORTS")
    return await session.t_query(vq)


@router.get("/api/report/consulttext")
async def consult_report_text(
    dfn: str = Query(...),
    reportId: str = Query(...),
    consultIen: str = Query(...),
    alpha: str = Query(""),
    omega: str = Query(""),
    nRpts: int = Query(0),
    ien: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    a, o = to_fm_range(alpha, omega)
    vq = VistaQuery("ORWCS REPORT TEXT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, reportId)
    vq.add_parameter(VistaQuery.LITERAL, consultIen)
    vq.add_parameter(VistaQuery.LITERAL, a)
    vq.add_parameter(VistaQuery.LITERAL, o)
    vq.add_parameter(VistaQuery.LITERAL, str(nRpts))
    if ien:
        vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


# ── Imaging & Procedures ─────────────────────────────────────────

@router.get("/api/report/imagingexams")
async def imaging_exams(dfn: str = Query(...), alpha: str = Query(""), omega: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    a, o = to_fm_range(alpha, omega)
    vq = VistaQuery("ORWRA IMAGING EXAMS1")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    if a:
        vq.add_parameter(VistaQuery.LITERAL, a)
        vq.add_parameter(VistaQuery.LITERAL, o)
    return await session.t_query(vq)


@router.get("/api/report/patientprocedures")
async def patient_procedures(dfn: str = Query(...), alpha: str = Query(""), omega: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    a, o = to_fm_range(alpha, omega)
    vq = VistaQuery("ORWMC PATIENT PROCEDURES1")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    if a:
        vq.add_parameter(VistaQuery.LITERAL, a)
        vq.add_parameter(VistaQuery.LITERAL, o)
    return await session.t_query(vq)


@router.get("/api/report/getimg")
async def get_img(noteIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD GETIMG")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.s_query(vq)


# ── Printing ──────────────────────────────────────────────────────

@router.post("/api/report/printvreport")
async def print_vista_report(reportData: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP PRINT V REPORT")
    dhl = DictionaryHashList()
    for i, item in enumerate(reportData):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/report/printremotereport")
async def print_remote_report(reportData: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP PRINT REMOTE REPORT")
    dhl = DictionaryHashList()
    for i, item in enumerate(reportData):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/report/printreport")
async def print_report(reportData: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP PRINT REPORT")
    dhl = DictionaryHashList()
    for i, item in enumerate(reportData):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/report/printwindowsremote")
async def print_windows_remote(reportData: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP PRINT WINDOWS REMOTE")
    dhl = DictionaryHashList()
    for i, item in enumerate(reportData):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/report/printwindowsreport")
async def print_windows_report(reportData: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP PRINT WINDOWS REPORT")
    dhl = DictionaryHashList()
    for i, item in enumerate(reportData):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/report/winprintdefault")
async def win_print_default(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWRP WINPRINT DEFAULT")
    return await session.s_query(vq)


@router.post("/api/report/savedefaultprinter")
async def save_default_printer(printer: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWRP SAVE DEFAULT PRINTER")
    vq.add_parameter(VistaQuery.LITERAL, printer)
    return await session.s_query(vq)


# ── Health Summary Components ─────────────────────────────────────

@router.get("/api/report/hsfilelookup")
async def hs_file_lookup(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP2 HS FILE LOOKUP")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/report/hscompfiles")
async def hs_comp_files(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP2 HS COMP FILES")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.get("/api/report/hssubitems")
async def hs_subitems(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP2 HS SUBITEMS")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.get("/api/report/hsreporttext")
async def hs_report_text(dfn: str = Query(...), components: str = Query(...), reportId: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP2 HS REPORT TEXT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, components)
    if reportId:
        vq.add_parameter(VistaQuery.LITERAL, reportId)
    return await session.t_query(vq)


@router.get("/api/report/hscomponents")
async def hs_components(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP2 HS COMPONENTS")
    return await session.t_query(vq)


@router.get("/api/report/compabv")
async def comp_abv(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWRP2 COMPABV")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.get("/api/report/compdisp")
async def comp_disp(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP2 COMPDISP")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.get("/api/report/hscomponentsubs")
async def hs_component_subs(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP2 HS COMPONENT SUBS")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.get("/api/report/getlkup")
async def get_lookup(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWRP2 GETLKUP")
    return await session.s_query(vq)


@router.post("/api/report/savlkup")
async def save_lookup(value: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWRP2 SAVLKUP")
    vq.add_parameter(VistaQuery.LITERAL, value)
    return await session.s_query(vq)


# ── Remote Data (CIRN / HDR / XWB) ───────────────────────────────

@router.get("/api/report/autordv")
async def auto_rdv(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCIRN AUTORDV")
    return await session.s_query(vq)


@router.get("/api/report/hdron")
async def hdr_on(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCIRN HDRON")
    return await session.s_query(vq)


@router.post("/api/report/hdrmodify")
async def hdr_modify(dfn: str = Query(...), hdrParams: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP4 HDR MODIFY")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    dhl = DictionaryHashList()
    for i, item in enumerate(hdrParams):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/report/deferredclearall")
async def deferred_clear_all(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("XWB DEFERRED CLEARALL")
    return await session.s_query(vq)


@router.post("/api/report/remoterpc")
async def remote_rpc(rpcData: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("XWB REMOTE RPC")
    dhl = DictionaryHashList()
    for i, item in enumerate(rpcData):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/report/directrpc")
async def direct_rpc(rpcData: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("XWB DIRECT RPC")
    dhl = DictionaryHashList()
    for i, item in enumerate(rpcData):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/report/remotestatuscheck")
async def remote_status_check(handle: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("XWB REMOTE STATUS CHECK")
    vq.add_parameter(VistaQuery.LITERAL, handle)
    return await session.t_query(vq)


@router.get("/api/report/remotegetdata")
async def remote_get_data(handle: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("XWB REMOTE GETDATA")
    vq.add_parameter(VistaQuery.LITERAL, handle)
    return await session.t_query(vq)


# ── Simple convenience endpoints ─────────────────────────────────

@router.get("/api/report/run")
async def run_report(
    dfn: str = Query(...),
    reportId: str = Query(...),
    hsTag: str = Query(""),
    hsType: str = Query(""),
    daysBack: str = Query(""),
    examId: str = Query(""),
    alpha: str = Query(""),
    omega: str = Query(""),
    rpcName: str = Query(""),
    maxOcc: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    a, o = to_fm_range(alpha, omega)

    effective_hs_tag = hsTag or ""
    if maxOcc is not None:
        pieces = effective_hs_tag.split("^")
        while len(pieces) < 4:
            pieces.append("")
        pieces[3] = maxOcc
        effective_hs_tag = "^".join(pieces)

    rpc = rpcName if rpcName else "ORWRP REPORT TEXT"
    a_report = reportId if not effective_hs_tag else f"{reportId}~{effective_hs_tag}"

    vq = VistaQuery(rpc)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, a_report)
    vq.add_parameter(VistaQuery.LITERAL, hsType)
    vq.add_parameter(VistaQuery.LITERAL, daysBack)
    vq.add_parameter(VistaQuery.LITERAL, examId)
    vq.add_parameter(VistaQuery.LITERAL, a)
    vq.add_parameter(VistaQuery.LITERAL, o)
    return await session.t_query(vq)


@router.get("/api/report/tabledata")
async def table_data(dfn: str = Query(...), reportId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWRP REPORT TEXT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, reportId)
    return await session.t_query(vq)


@router.post("/api/report/runadhoc")
async def run_adhoc(dfn: str = Query(...), components: str = Query(""), dateFrom: str = Query(""), dateTo: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWRP2 HS REPORT TEXT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, components)
    vq.add_parameter(VistaQuery.LITERAL, dateFrom)
    vq.add_parameter(VistaQuery.LITERAL, dateTo)
    return await session.s_query(vq)
