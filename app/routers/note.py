# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Note router — mirrors NoteController.cs.

TIU progress notes — list, read, create, edit, sign, delete, print, cosigner management.
"""

from __future__ import annotations

from typing import Optional

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.models.note import CreateNoteResult
from app.models.tiu_document import TiuDocument
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Note Listing & Context ──────────────────────────────────────────

@router.get("/api/note/list")
async def list_notes(
    dfn: str = Query(...),
    context: int = Query(...),
    early: str = Query(""),
    late: str = Query(""),
    person: int = Query(0),
    occurrenceLimit: int = Query(0),
    sortSequence: str = Query(""),
    showAddenda: bool = Query(False),
    session: ISession = Depends(get_current_session),
) -> list[TiuDocument]:
    vq = VistaQuery("TIU DOCUMENTS BY CONTEXT")
    vq.add_parameter(VistaQuery.LITERAL, "3")
    vq.add_parameter(VistaQuery.LITERAL, str(context))
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, early)
    vq.add_parameter(VistaQuery.LITERAL, late)
    vq.add_parameter(VistaQuery.LITERAL, str(person))
    vq.add_parameter(VistaQuery.LITERAL, str(occurrenceLimit))
    vq.add_parameter(VistaQuery.LITERAL, sortSequence)
    vq.add_parameter(VistaQuery.LITERAL, "1" if showAddenda else "0")
    results = await session.t_query(vq)
    return TiuDocument.parse_list(results)


@router.get("/api/note/summaries")
async def summaries(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU SUMMARIES")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/note/context")
async def get_context(duz: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTIU GET TIU CONTEXT")
    vq.add_parameter(VistaQuery.LITERAL, str(duz))
    return await session.s_query(vq)


@router.post("/api/note/savecontext")
async def save_context(contextString: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTIU SAVE TIU CONTEXT")
    vq.add_parameter(VistaQuery.LITERAL, contextString)
    return await session.s_query(vq)


@router.get("/api/note/siteparameters")
async def site_parameters(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU GET SITE PARAMETERS")
    return await session.s_query(vq)


# ── Note Reading ────────────────────────────────────────────────────

@router.get("/api/note/text")
async def note_text(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU GET RECORD TEXT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.t_query(vq)


@router.get("/api/note/detaileddisplay")
async def detailed_display(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU DETAILED DISPLAY")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.t_query(vq)


@router.get("/api/note/loadforedit")
async def load_for_edit(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU LOAD RECORD FOR EDIT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, ".01;.06;.07;1301;1204;1208;1701;1205;1405;2101;70201;70202")
    return await session.t_query(vq)


@router.get("/api/note/edittextonly")
async def edit_text_only(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU LOAD RECORD TEXT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.t_query(vq)


@router.get("/api/note/boilerplate")
async def boilerplate(
    title: int = Query(...),
    dfn: str = Query(...),
    visitStr: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU LOAD BOILERPLATE TEXT")
    vq.add_parameter(VistaQuery.LITERAL, str(title))
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, visitStr)
    return await session.t_query(vq)


@router.get("/api/note/listboxitem")
async def listbox_item(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTIU GET LISTBOX ITEM")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.get("/api/note/hastext")
async def has_text(noteIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTIU CHKTXT")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.s_query(vq)


@router.get("/api/notes/count")
async def note_count(dfn: int = Query(...), session: ISession = Depends(get_current_session)) -> int:
    vq = VistaQuery("ORCNOTE GET TOTAL")
    vq.add_parameter(VistaQuery.LITERAL, str(dfn))
    raw = await session.s_query(vq)
    try:
        return int((raw or "0").strip())
    except ValueError:
        return 0


# ── Note Titles ─────────────────────────────────────────────────────

@router.get("/api/note/titles")
async def titles(
    startFrom: str = Query(...),
    direction: int = Query(...),
    idNotesOnly: bool = Query(False),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU LONG LIST OF TITLES")
    vq.add_parameter(VistaQuery.LITERAL, "3")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    if idNotesOnly:
        vq.add_parameter(VistaQuery.LITERAL, "1")
    return await session.t_query(vq)


@router.get("/api/note/printname")
async def print_name(titleIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU GET PRINT NAME")
    vq.add_parameter(VistaQuery.LITERAL, str(titleIen))
    return await session.s_query(vq)


@router.get("/api/note/documenttitle")
async def document_title(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU GET DOCUMENT TITLE")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.get("/api/note/isconsulttitle")
async def is_consult_title(titleIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU IS THIS A CONSULT?")
    vq.add_parameter(VistaQuery.LITERAL, str(titleIen))
    return await session.s_query(vq)


@router.get("/api/note/isprftitle")
async def is_prf_title(titleIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU ISPRF")
    vq.add_parameter(VistaQuery.LITERAL, str(titleIen))
    return await session.s_query(vq)


@router.get("/api/note/isclinproctitle")
async def is_clinproc_title(titleIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU IS THIS A CLINPROC?")
    vq.add_parameter(VistaQuery.LITERAL, str(titleIen))
    return await session.s_query(vq)


@router.get("/api/note/personaltitles")
async def personal_titles(
    duz: int = Query(...),
    docClass: int = Query(3),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU PERSONAL TITLE LIST")
    vq.add_parameter(VistaQuery.LITERAL, str(duz))
    vq.add_parameter(VistaQuery.LITERAL, str(docClass))
    return await session.t_query(vq)


@router.get("/api/note/personalpreferences")
async def personal_preferences(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU GET PERSONAL PREFERENCES")
    return await session.s_query(vq)


# ── Note Create / Edit / Save ──────────────────────────────────────

@router.post("/api/note/create")
async def create(
    dfn: str = Query(...),
    title: int = Query(...),
    visitStr: str = Query(...),
    authorDuz: int = Query(...),
    fmDate: str = Query(""),
    cosignerDuz: int = Query(0),
    subject: str = Query(""),
    consultIen: int = Query(0),
    session: ISession = Depends(get_current_session),
) -> CreateNoteResult:
    vq = VistaQuery("TIU CREATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(title))
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, "")
    pkg_ref = f"{consultIen};GMR(123," if consultIen > 0 else ""
    fields = f".01;{title}|1202;{authorDuz}|1301;{fmDate}|1205;{cosignerDuz}|1208;{subject}|1701;"
    if pkg_ref:
        fields += f"|1405;{pkg_ref}"
    vq.add_parameter(VistaQuery.LITERAL, fields)
    vq.add_parameter(VistaQuery.LITERAL, visitStr)
    vq.add_parameter(VistaQuery.LITERAL, "1")
    result = await session.s_query(vq)
    return CreateNoteResult.parse(result)


@router.post("/api/note/createaddendum")
async def create_addendum(
    addendumTo: int = Query(...),
    authorDuz: int = Query(...),
    fmDate: str = Query(""),
    cosignerDuz: int = Query(0),
    session: ISession = Depends(get_current_session),
) -> CreateNoteResult:
    vq = VistaQuery("TIU CREATE ADDENDUM RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(addendumTo))
    fields = f"1202;{authorDuz}|1301;{fmDate}|1208;{cosignerDuz}"
    vq.add_parameter(VistaQuery.LITERAL, fields)
    vq.add_parameter(VistaQuery.LITERAL, "1")
    result = await session.s_query(vq)
    return CreateNoteResult.parse(result)


@router.post("/api/note/update")
async def update(
    noteIen: int = Query(...),
    fields: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU UPDATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    vq.add_parameter(VistaQuery.LITERAL, fields)
    return await session.s_query(vq)


@router.post("/api/note/settext")
async def set_text(
    noteIen: int = Query(...),
    noteText: list[str] = Body(...),
    suppress: int = Query(0),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU SET DOCUMENT TEXT")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    dhl = DictionaryHashList()
    for i, line in enumerate(noteText):
        dhl.add(f'"TEXT",{i + 1},0', line)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, str(suppress))
    return await session.s_query(vq)


# ── Note Actions (Sign, Delete, Lock, etc.) ────────────────────────

@router.get("/api/note/authorization")
async def authorization(
    ien: int = Query(...),
    actionName: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU AUTHORIZATION")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, actionName)
    return await session.s_query(vq)


@router.post("/api/note/sign")
async def sign(
    ien: int = Query(...),
    esCode: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU SIGN RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, esCode)
    return await session.s_query(vq)


@router.post("/api/note/delete")
async def delete_note(
    ien: int = Query(...),
    reason: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU DELETE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, reason)
    return await session.t_query(vq)


@router.get("/api/note/justifydelete")
async def justify_delete(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU JUSTIFY DELETE?")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.post("/api/note/lock")
async def lock(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU LOCK RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.post("/api/note/unlock")
async def unlock(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU UNLOCK RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.get("/api/note/wassaved")
async def was_saved(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU WAS THIS SAVED?")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.get("/api/note/authorsigned")
async def author_signed(
    ien: int = Query(...),
    duz: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU HAS AUTHOR SIGNED?")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, str(duz))
    return await session.s_query(vq)


@router.get("/api/note/whichsignatureaction")
async def which_signature_action(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU WHICH SIGNATURE ACTION")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.get("/api/note/requirescosignature")
async def requires_cosignature(
    titleOrType: int = Query(...),
    docOrZero: int = Query(...),
    authorDuz: int = Query(...),
    fmDate: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU REQUIRES COSIGNATURE")
    vq.add_parameter(VistaQuery.LITERAL, str(titleOrType))
    vq.add_parameter(VistaQuery.LITERAL, str(docOrZero))
    vq.add_parameter(VistaQuery.LITERAL, str(authorDuz))
    if fmDate:
        vq.add_parameter(VistaQuery.LITERAL, fmDate)
    return await session.s_query(vq)


@router.get("/api/note/canchangecosigner")
async def can_change_cosigner(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU CAN CHANGE COSIGNER?")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


# ── Additional Signers ──────────────────────────────────────────────

@router.get("/api/note/additionalsigners")
async def additional_signers(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU GET ADDITIONAL SIGNERS")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.t_query(vq)


@router.post("/api/note/updateadditionalsigners")
async def update_additional_signers(
    ien: int = Query(...),
    signers: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU UPDATE ADDITIONAL SIGNERS")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    dhl = DictionaryHashList()
    for i, signer in enumerate(signers):
        dhl.add(str(i), signer)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── Print & Formatted Output ───────────────────────────────────────

@router.post("/api/note/print")
async def print_note(
    noteIen: int = Query(...),
    device: str = Query(...),
    chartCopy: bool = Query(False),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU PRINT RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    vq.add_parameter(VistaQuery.LITERAL, device)
    vq.add_parameter(VistaQuery.LITERAL, "1" if chartCopy else "0")
    return await session.s_query(vq)


@router.get("/api/note/formattedtext")
async def formatted_text(
    noteIen: int = Query(...),
    chartCopy: bool = Query(False),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWTIU WINPRINT NOTE")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    vq.add_parameter(VistaQuery.LITERAL, "1" if chartCopy else "0")
    return await session.t_query(vq)


@router.get("/api/note/canprint")
async def can_print(noteIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU CAN PRINT WORK/CHART COPY")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.s_query(vq)


@router.get("/api/note/documentparameters")
async def document_parameters(noteIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU GET DOCUMENT PARAMETERS")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.s_query(vq)


# ── Consult / Visit References ──────────────────────────────────────

@router.get("/api/note/visitstring")
async def visit_string(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE NOTEVSTR")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.get("/api/note/packagereference")
async def package_reference(noteIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU GET REQUEST")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.s_query(vq)


@router.get("/api/note/consultrequests")
async def consult_requests(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("GMRC LIST CONSULT REQUESTS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/note/unresolvedconsults")
async def unresolved_consults(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQQCN UNRESOLVED")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/note/onevisitcheck")
async def one_visit_check(
    noteIen: str = Query(...),
    dfn: str = Query(...),
    visitStr: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU ONE VISIT NOTE?")
    vq.add_parameter(VistaQuery.LITERAL, noteIen)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, visitStr)
    return await session.s_query(vq)


@router.get("/api/note/ancillarymessages")
async def ancillary_messages(
    ien: int = Query(...),
    action: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU ANCILLARY PACKAGE MESSAGE")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, action)
    return await session.t_query(vq)


# ── ID Notes & Linking ──────────────────────────────────────────────

@router.get("/api/note/canlink")
async def can_link(title: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTIU CANLINK")
    vq.add_parameter(VistaQuery.LITERAL, str(title))
    return await session.s_query(vq)


@router.get("/api/note/canattach")
async def can_attach(docId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU ID CAN ATTACH")
    vq.add_parameter(VistaQuery.LITERAL, docId)
    return await session.s_query(vq)


@router.get("/api/note/canreceive")
async def can_receive(docId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU ID CAN RECEIVE")
    vq.add_parameter(VistaQuery.LITERAL, docId)
    return await session.s_query(vq)


@router.post("/api/note/attach")
async def attach(
    docId: str = Query(...),
    parentDocId: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU ID ATTACH ENTRY")
    vq.add_parameter(VistaQuery.LITERAL, docId)
    vq.add_parameter(VistaQuery.LITERAL, parentDocId)
    return await session.s_query(vq)


@router.post("/api/note/detach")
async def detach(docId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU ID DETACH ENTRY")
    vq.add_parameter(VistaQuery.LITERAL, docId)
    return await session.s_query(vq)


@router.post("/api/note/updatefield")
async def update_field(
    noteIen: int = Query(...),
    field: str = Query(...),
    value: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU UPDATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    dhl = DictionaryHashList()
    dhl.add(field, value)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── User Class ──────────────────────────────────────────────────────

@router.get("/api/note/userclasses")
async def user_classes(
    startFrom: str = Query(...),
    direction: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("TIU USER CLASS LONG LIST")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/note/divclassinfo")
async def div_class_info(userIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU DIV AND CLASS INFO")
    vq.add_parameter(VistaQuery.LITERAL, str(userIen))
    return await session.t_query(vq)


@router.get("/api/note/userinactive")
async def user_inactive(ein: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU USER INACTIVE?")
    vq.add_parameter(VistaQuery.LITERAL, ein)
    return await session.s_query(vq)


# ── PCE / Encounter Findings ───────────────────────────────────────

@router.post("/api/note/savepce")
async def save_pce(
    noteIen: int = Query(...),
    entries: list[str] = Body(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWPCE SAVE")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    dhl = DictionaryHashList()
    for i, entry in enumerate(entries):
        dhl.add(str(i + 1), entry or "")
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.post("/api/note/savehealthfactors")
async def save_health_factors(
    noteIen: int = Query(...),
    entries: list[str] = Body(...),
    dfn: Optional[str] = Query(None),
    visit: Optional[str] = Query(None),
    user: Optional[str] = Query(None),
    encProv: Optional[str] = Query(None),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("PXRMRPCG GENFUPD")
    dhl = DictionaryHashList()
    dhl.add("1", f"0^{dfn or ''}^{visit or ''}^{noteIen}^{user or ''}^{encProv or ''}")
    for i, entry in enumerate(entries):
        dhl.add(str(i + 2), entry or "")
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── Simplified Aliases (Web.CPRS) ──────────────────────────────────

@router.post("/api/note/addendum")
async def addendum(
    dfn: str = Query(...),
    parentIen: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU CREATE ADDENDUM RECORD")
    vq.add_parameter(VistaQuery.LITERAL, str(parentIen))
    return await session.s_query(vq)


@router.post("/api/note/changetitle")
async def change_title(
    ien: str = Query(...),
    title: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU SET DOCUMENT TITLE")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, title)
    return await session.s_query(vq)


@router.post("/api/note/updatetext")
async def update_text(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU UPDATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.get("/api/note/properties")
async def properties(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU ID CAN ATTACH")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.post("/api/note/saveproperties")
async def save_properties(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU SET DOCUMENT TITLE")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.post("/api/note/changeauthor")
async def change_author(
    ien: str = Query(...),
    authorDuz: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("TIU UPDATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, f"1202/{authorDuz}")
    return await session.s_query(vq)
