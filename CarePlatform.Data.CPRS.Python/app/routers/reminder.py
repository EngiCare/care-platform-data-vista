# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Reminder router — mirrors ReminderController.cs.

Unevaluated reminders, categories, evaluation, education,
reminder dialogs, prompts, cover-sheet, folders, MH testing,
women's health, GEC, MST, general findings.
"""

from __future__ import annotations

import json
from typing import Optional

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Reminder Lists & Categories ────────────────────────────────────

@router.get("/api/reminder/unevaluated")
async def unevaluated(dfn: str = Query(...), location: str = Query("0"), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM REMINDERS UNEVALUATED")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, location)
    return await session.t_query(vq)


@router.get("/api/reminder/applicable")
async def applicable(dfn: str = Query(...), location: str = Query("0"), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM REMINDERS APPLICABLE")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, location)
    return await session.t_query(vq)


@router.get("/api/reminder/categories")
async def categories(dfn: str = Query(...), location: str = Query("0"), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM REMINDER CATEGORIES")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, location)
    return await session.t_query(vq)


@router.get("/api/reminder/category")
async def category(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("PXRM REMINDER CATEGORY")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.get("/api/reminder/remindersandcategories")
async def reminders_and_categories(dfn: str = Query(...), location: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("PXRM REMINDERS AND CATEGORIES")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, location)
    return await session.t_query(vq)


@router.get("/api/reminder/linkseq")
async def reminder_link_seq(ien: str = Query(...), parentIen: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPXRM REMINDER LINK SEQ")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, parentIen)
    return await session.s_query(vq)


# ── Reminder Evaluation & Detail ──────────────────────────────────

@router.get("/api/reminder/evaluate")
async def evaluate(dfn: str = Query(...), reminderIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM REMINDER EVALUATION")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, reminderIen)
    return await session.t_query(vq)


@router.post("/api/reminder/evaluatelist")
async def evaluate_list(dfn: str = Query(...), reminderIens: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM REMINDER EVALUATION")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    dhl = DictionaryHashList()
    for i, ien in enumerate(reminderIens):
        dhl.add(str(i + 1), ien)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/reminder/detail")
async def detail(dfn: str = Query(...), reminderIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM REMINDER DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, reminderIen)
    return await session.t_query(vq)


@router.get("/api/reminder/detailalt")
async def detail_alt(dfn: str = Query(...), reminderIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPX REMINDER DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, reminderIen)
    return await session.t_query(vq)


@router.get("/api/reminder/inquiry")
async def inquiry(reminderIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM REMINDER INQUIRY")
    vq.add_parameter(VistaQuery.LITERAL, reminderIen)
    return await session.t_query(vq)


@router.get("/api/reminder/web")
async def reminder_web(reminderIen: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPXRM REMINDER WEB")
    vq.add_parameter(VistaQuery.LITERAL, reminderIen)
    return await session.s_query(vq)


# ── Education ──────────────────────────────────────────────────────

@router.get("/api/reminder/educationsummary")
async def education_summary(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM EDUCATION SUMMARY")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/reminder/educationsubtopics")
async def education_subtopics(topicIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM EDUCATION SUBTOPICS")
    vq.add_parameter(VistaQuery.LITERAL, topicIen)
    return await session.t_query(vq)


@router.get("/api/reminder/educationtopic")
async def education_topic(topicIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM EDUCATION TOPIC")
    vq.add_parameter(VistaQuery.LITERAL, topicIen)
    return await session.t_query(vq)


# ── Reminder Dialogs & Prompts ────────────────────────────────────

@router.get("/api/reminder/dialog")
async def dialog(reminderIen: str = Query(...), dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM REMINDER DIALOG")
    vq.add_parameter(VistaQuery.LITERAL, reminderIen)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/reminder/dialogtiu")
async def dialog_tiu(reminderIen: str = Query(...), dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("PXRM REMINDER DIALOG (TIU)")
    vq.add_parameter(VistaQuery.LITERAL, reminderIen)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/reminder/dialogprompts")
async def dialog_prompts(dialogIen: str = Query(...), dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM DIALOG PROMPTS")
    vq.add_parameter(VistaQuery.LITERAL, dialogIen)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/reminder/dialogactive")
async def dialog_active(dialogIen: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPXRM DIALOG ACTIVE")
    vq.add_parameter(VistaQuery.LITERAL, dialogIen)
    return await session.s_query(vq)


@router.get("/api/reminder/newactive")
async def new_reminders_active(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPX NEW REMINDERS ACTIVE")
    return await session.s_query(vq)


@router.get("/api/reminder/noteheader")
async def progress_note_header(reminderIen: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPXRM PROGRESS NOTE HEADER")
    vq.add_parameter(VistaQuery.LITERAL, reminderIen)
    return await session.s_query(vq)


# ── Cover Sheet & Folders ─────────────────────────────────────────

@router.get("/api/reminder/folders")
async def get_folders(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPX GET FOLDERS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.post("/api/reminder/setfolders")
async def set_folders(dfn: str = Query(...), folders: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPX SET FOLDERS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    dhl = DictionaryHashList()
    for i, f in enumerate(folders):
        dhl.add(str(i + 1), f)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.get("/api/reminder/deflocations")
async def get_def_locations(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPX GET DEF LOCATIONS")
    return await session.t_query(vq)


@router.get("/api/reminder/insertcursor")
async def rem_insert_at_cursor(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPX REM INSERT AT CURSOR")
    return await session.s_query(vq)


@router.get("/api/reminder/newcoversheetactive")
async def new_cover_sheet_active(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPX NEW COVER SHEET ACTIVE")
    return await session.s_query(vq)


@router.get("/api/reminder/lvremlst")
async def cover_sheet_level_list(loc: str = Query(...), cls: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPX LVREMLST")
    vq.add_parameter(VistaQuery.LITERAL, loc)
    vq.add_parameter(VistaQuery.LITERAL, cls)
    return await session.t_query(vq)


@router.post("/api/reminder/savelvl")
async def save_level(items: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPX SAVELVL")
    dhl = DictionaryHashList()
    for i, item in enumerate(items):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── Women's Health & MST & GEC ────────────────────────────────────

@router.post("/api/reminder/womenhealthsave")
async def women_health_save(data: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPXRM WOMEN HEALTH SAVE")
    dhl = DictionaryHashList()
    for i, item in enumerate(data):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.post("/api/reminder/mstupdate")
async def mst_update(data: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPXRM MST UPDATE")
    dhl = DictionaryHashList()
    for i, item in enumerate(data):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.get("/api/reminder/gecdialog")
async def gec_dialog(dfn: str = Query(...), reminderIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM GEC DIALOG")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, reminderIen)
    return await session.t_query(vq)


@router.get("/api/reminder/gecfinished")
async def gec_finished(dfn: str = Query(...), reminderIen: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPXRM GEC FINISHED?")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, reminderIen)
    return await session.s_query(vq)


# ── Mental Health Integration ─────────────────────────────────────

@router.get("/api/reminder/mhv")
async def mhv(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM MHV")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/reminder/mhdlldms")
async def mh_dll_dms(testName: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQPXRM MHDLLDMS")
    vq.add_parameter(VistaQuery.LITERAL, testName)
    return await session.s_query(vq)


@router.get("/api/reminder/mhdll")
async def mh_dll(testName: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPXRM MHDLL")
    vq.add_parameter(VistaQuery.LITERAL, testName)
    return await session.t_query(vq)


# ── General Findings (PXRMRPCG / PXRMRPCC) ───────────────────────

@router.post("/api/reminder/gencancel")
async def gen_cancel(data: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("PXRMRPCG CANCEL")
    dhl = DictionaryHashList()
    for i, item in enumerate(data):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.post("/api/reminder/genfupd")
async def gen_f_update(
    data: list[str] = Body(...),
    noteIen: Optional[str] = Query(None),
    dfn: Optional[str] = Query(None),
    visit: Optional[str] = Query(None),
    user: Optional[str] = Query(None),
    encProv: Optional[str] = Query(None),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("PXRMRPCG GENFUPD")
    dhl = DictionaryHashList()
    idx = 1
    if noteIen:
        dhl.add(str(idx), f"0^{dfn or ''}^{visit or ''}^{noteIen}^{user or ''}^{encProv or ''}")
        idx += 1
    for item in data:
        dhl.add(str(idx), item)
        idx += 1
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/reminder/genfvald")
async def gen_f_validate(data: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("PXRMRPCG GENFVALD")
    dhl = DictionaryHashList()
    for i, item in enumerate(data):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/reminder/genview")
async def gen_view(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("PXRMRPCG VIEW")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.get("/api/reminder/promptvl")
async def prompt_value_list(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("PXRMRPCC PROMPTVL")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


# ── Structured Reminder Dialog Runtime ────────────────────────────

@router.get("/api/reminder/dialogdef")
async def dialog_definition(ien: str = Query(...), dfn: Optional[str] = Query(None), session: ISession = Depends(get_current_session)) -> str:
    """Returns raw lines from PXRM REMINDER DIALOG (TIU) as JSON.

    The C# version parses these into a ReminderDialogDefinition model;
    we return the raw wire data as a JSON list for now (Option A parity).
    The web client can parse client-side.
    """
    vq = VistaQuery("PXRM REMINDER DIALOG (TIU)")
    vq.add_parameter(VistaQuery.LITERAL, ien or "")
    vq.add_parameter(VistaQuery.LITERAL, dfn or "")
    lines = await session.t_query(vq)
    if not lines:
        return ""
    return json.dumps(lines)
