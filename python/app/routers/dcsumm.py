# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""DCSumm router — mirrors DCSummController.cs.

Discharge Summaries — titles, context, create/edit/sign, urgencies, document params.
TIU DC-summary RPCs, ORWTIU context RPCs.
"""

from __future__ import annotations

from typing import Optional

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.models.tiu_document import TiuDocument
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()

# TIU class constant for DC Summaries
CLS_DC_SUMM = "244"


# ── Titles / Preferences ────────────────────────────────────────────

@router.get("/api/dcsumm/titles")
async def subset_of_dcsumm_titles(
    startFrom: str = Query(""),
    direction: int = Query(1),
    idNoteTitlesOnly: bool = Query(False),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU LONG LIST OF TITLES")
    vq.add_parameter(VistaQuery.LITERAL, CLS_DC_SUMM)
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    if idNoteTitlesOnly:
        vq.add_parameter(VistaQuery.LITERAL, "1")
    return await session.t_query(vq)


@router.get("/api/dcsumm/personaltitles")
async def personal_titles(
    userDuz: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU PERSONAL TITLE LIST")
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    vq.add_parameter(VistaQuery.LITERAL, CLS_DC_SUMM)
    return await session.t_query(vq)


@router.get("/api/dcsumm/preferences")
async def personal_preferences(
    userDuz: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU GET PERSONAL PREFERENCES")
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    return await session.s_query(vq)


@router.get("/api/dcsumm/urgencies")
async def ds_urgencies(
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU GET DS URGENCIES")
    return await session.t_query(vq)


# ── Listing / Display ───────────────────────────────────────────────

@router.get("/api/dcsumm/list")
async def list_summs_for_tree(
    context: int = Query(...),
    dfn: str = Query(...),
    early: str = Query(""),
    late: str = Query(""),
    person: int = Query(0),
    occLim: int = Query(0),
    sortAscending: bool = Query(False),
    showAddenda: bool = Query(False),
    view: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[TiuDocument]:
    sort_seq = "A" if sortAscending else "D"
    vq = VistaQuery("TIU DOCUMENTS BY CONTEXT")
    vq.add_parameter(VistaQuery.LITERAL, CLS_DC_SUMM)
    vq.add_parameter(VistaQuery.LITERAL, str(context))
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, early)
    vq.add_parameter(VistaQuery.LITERAL, late)
    vq.add_parameter(VistaQuery.LITERAL, str(person))
    vq.add_parameter(VistaQuery.LITERAL, str(occLim))
    vq.add_parameter(VistaQuery.LITERAL, sort_seq)
    vq.add_parameter(VistaQuery.LITERAL, "1" if showAddenda else "0")
    results = await session.t_query(vq)
    return TiuDocument.parse_list(results)


@router.get("/api/dcsumm/authorization")
async def act_on_dc_document(
    ien: int = Query(...),
    actionName: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU AUTHORIZATION")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, actionName)
    return await session.s_query(vq)


# ── Edit / Text ─────────────────────────────────────────────────────

@router.get("/api/dcsumm/loadforedit")
async def get_dcsumm_for_edit(
    ien: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU LOAD RECORD FOR EDIT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, ".01;.06;.07;.09;1202;1205;1208;1209;1301;1302;1307;1701")
    return await session.t_query(vq)


@router.get("/api/dcsumm/loadtextonly")
async def get_dcsumm_text_only(
    ien: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU LOAD RECORD TEXT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.t_query(vq)


# ── Create / Save / Sign ────────────────────────────────────────────

@router.post("/api/dcsumm/create")
async def create_dcsumm(
    dfn: str = Query(...),
    title: int = Query(...),
    visitStr: str = Query(""),
    fieldData: list[str] = [],
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU CREATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(title))
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, "")
    dhl = DictionaryHashList()
    for i, val in enumerate(fieldData):
        dhl.add(str(i), val)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, visitStr)
    vq.add_parameter(VistaQuery.LITERAL, "1")
    return await session.t_query(vq)


@router.post("/api/dcsumm/createaddendum")
async def create_addendum(
    addendumTo: int = Query(...),
    fieldData: list[str] = [],
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU CREATE ADDENDUM RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(addendumTo))
    dhl = DictionaryHashList()
    for i, val in enumerate(fieldData):
        dhl.add(str(i), val)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, "1")
    return await session.t_query(vq)


@router.post("/api/dcsumm/update")
async def update_dcsumm(
    noteIen: int = Query(...),
    fieldData: list[str] = [],
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU UPDATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    dhl = DictionaryHashList()
    for i, val in enumerate(fieldData):
        dhl.add(str(i), val)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/dcsumm/sign")
async def sign_dc_document(
    ien: int = Query(...),
    esCode: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU SIGN RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, esCode)
    return await session.s_query(vq)


# ── Document Parameters ─────────────────────────────────────────────

@router.get("/api/dcsumm/docparams")
async def get_document_parameters(
    noteIen: int = Query(0),
    typeIen: int = Query(0),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU GET DOCUMENT PARAMETERS")
    if noteIen > 0:
        vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    else:
        vq.add_parameter(VistaQuery.LITERAL, "0")
        vq.add_parameter(VistaQuery.LITERAL, str(typeIen))
    return await session.s_query(vq)


# ── Attending / Discharge ───────────────────────────────────────────

@router.get("/api/dcsumm/attending")
async def get_attending(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORQPT ATTENDING/PRIMARY")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/dcsumm/dischargedate")
async def get_discharge_date(
    dfn: str = Query(...),
    admitDateTime: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWPT DISCHARGE")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, admitDateTime)
    return await session.s_query(vq)


# ── Context ─────────────────────────────────────────────────────────

@router.get("/api/dcsumm/context")
async def get_dcsumm_context(
    userDuz: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWTIU GET DCSUMM CONTEXT")
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    return await session.s_query(vq)


@router.post("/api/dcsumm/context")
async def save_dcsumm_context(
    context: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWTIU SAVE DCSUMM CONTEXT")
    vq.add_parameter(VistaQuery.LITERAL, context)
    return await session.s_query(vq)


@router.post("/api/dcsumm/changeattending")
async def change_attending(
    ien: int = Query(...),
    fieldData: list[str] = [],
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU UPDATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    dhl = DictionaryHashList()
    for i, val in enumerate(fieldData):
        dhl.add(str(i), val)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


# ── Simple convenience endpoints (ws/ prefix) ───────────────────────

@router.get("/api/dcsumm/ws/list")
async def ws_list(
    dfn: str = Query(...),
    view: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWCS LIST OF REPORTS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, view)
    return await session.t_query(vq)


@router.get("/api/dcsumm/text")
async def get_record_text(
    ien: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU GET RECORD TEXT")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.post("/api/dcsumm/ws/create")
async def ws_create(
    dfn: str = Query(...),
    titleIen: str = Query(...),
    text: str = Query(""),
    encounterDate: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU CREATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, titleIen)
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, "")
    dhl = DictionaryHashList()
    if text:
        dhl.add("0", text)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, encounterDate)
    vq.add_parameter(VistaQuery.LITERAL, "1")
    return await session.s_query(vq)


@router.post("/api/dcsumm/ws/savetext")
async def ws_save_text(
    ien: str = Query(...),
    text: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU UPDATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    dhl = DictionaryHashList()
    if text:
        dhl.add("0", text)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.post("/api/dcsumm/ws/sign")
async def ws_sign(
    ien: str = Query(...),
    esCode: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU SIGN RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, esCode)
    return await session.s_query(vq)


@router.post("/api/dcsumm/ws/addendum")
async def ws_addendum(
    parentIen: str = Query(...),
    text: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU CREATE ADDENDUM RECORD")
    vq.add_parameter(VistaQuery.LITERAL, parentIen)
    dhl = DictionaryHashList()
    if text:
        dhl.add("0", text)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, "1")
    return await session.s_query(vq)


@router.post("/api/dcsumm/ws/delete")
async def ws_delete(
    ien: str = Query(...),
    reason: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU DELETE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, reason)
    return await session.s_query(vq)


@router.post("/api/dcsumm/ws/changetitle")
async def ws_change_title(
    ien: str = Query(...),
    titleIen: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU SET DOCUMENT TITLE")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, titleIen)
    return await session.s_query(vq)


# ── Lock / Unlock ───────────────────────────────────────────────────

@router.post("/api/dcsumm/lock")
async def lock_record(
    ien: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU LOCK RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.post("/api/dcsumm/unlock")
async def unlock_record(
    ien: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU UNLOCK RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


# ── Additional Signers ──────────────────────────────────────────────

@router.get("/api/dcsumm/additionalsigners")
async def get_additional_signers(
    ien: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU GET ADDITIONAL SIGNERS")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.post("/api/dcsumm/addadditionalsigner")
async def add_additional_signer(
    ien: str = Query(...),
    signerDuz: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU ADD ADDITIONAL SIGNER")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, signerDuz)
    return await session.s_query(vq)


@router.post("/api/dcsumm/removeadditionalsigner")
async def remove_additional_signer(
    ien: str = Query(...),
    signerDuz: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU REMOVE ADDITIONAL SIGNER")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, signerDuz)
    return await session.s_query(vq)
