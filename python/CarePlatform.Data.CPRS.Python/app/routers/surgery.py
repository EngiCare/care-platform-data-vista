# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Surgery router — mirrors SurgeryController.cs.

Case lists, case details, surgery context, non-OR procedures, TIU operations.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/surgery/showtab")
async def show_surgery_tab(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWSR SHOW SURG TAB")
    return await session.s_query(vq)


@router.get("/api/surgery/identifyclass")
async def identify_surgery_class(className: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU IDENTIFY SURGERY CLASS")
    vq.add_parameter(VistaQuery.LITERAL, className)
    return await session.s_query(vq)


@router.get("/api/surgery/titles")
async def subset_of_surgery_titles(startFrom: str = Query(""), direction: int = Query(1), className: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU LONG LIST SURGERY TITLES")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    vq.add_parameter(VistaQuery.LITERAL, className)
    return await session.t_query(vq)


@router.get("/api/surgery/templatelist")
async def boilerplated_surgery_titles(startFrom: str = Query(""), direction: int = Query(1), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU LONG LIST BOILERPLATED")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/surgery/issurgerytitle")
async def is_surgery_title(titleIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU IS THIS A SURGERY?")
    vq.add_parameter(VistaQuery.LITERAL, str(titleIen))
    return await session.s_query(vq)


@router.get("/api/surgery/caselist")
async def get_surg_case_list(dfn: str = Query(...), early: str = Query(...), late: str = Query(...), context: int = Query(...), max: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWSR LIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, early)
    vq.add_parameter(VistaQuery.LITERAL, late)
    vq.add_parameter(VistaQuery.LITERAL, str(context))
    vq.add_parameter(VistaQuery.LITERAL, str(max))
    return await session.t_query(vq)


@router.get("/api/surgery/cases")
async def list_surgery_cases(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWSR CASELIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/surgery/reporttext")
async def load_report_text(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU GET RECORD TEXT")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.t_query(vq)


@router.get("/api/surgery/reportdetail")
async def load_report_detail(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU DETAILED DISPLAY")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.t_query(vq)


@router.get("/api/surgery/context")
async def get_surg_case_context(userDuz: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWSR GET SURG CONTEXT")
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    return await session.s_query(vq)


@router.post("/api/surgery/context")
async def save_surg_case_context(context: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWSR SAVE SURG CONTEXT")
    vq.add_parameter(VistaQuery.LITERAL, context)
    return await session.s_query(vq)


@router.get("/api/surgery/request")
async def get_case_ref_for_note(noteIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU GET REQUEST")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.s_query(vq)


@router.get("/api/surgery/onecase")
async def get_single_case(noteIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWSR ONECASE")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.t_query(vq)


@router.get("/api/surgery/personaltitles")
async def personal_titles(duz: int = Query(...), surgeryClass: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU PERSONAL TITLE LIST")
    vq.add_parameter(VistaQuery.LITERAL, str(duz))
    vq.add_parameter(VistaQuery.LITERAL, str(surgeryClass))
    return await session.t_query(vq)


@router.get("/api/surgery/isnonor")
async def is_non_or_procedure(caseIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWSR IS NON-OR PROCEDURE")
    vq.add_parameter(VistaQuery.LITERAL, str(caseIen))
    return await session.s_query(vq)


# ── Simple convenience endpoints ───────────────────────────────────

@router.get("/api/surgery/list")
async def list_reports(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWSR RPTLIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/surgery/report")
async def report(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWSR RPTTEXT")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.post("/api/surgery/create")
async def ws_create(dfn: str = Query(...), titleIen: str = Query(""), text: str = Query(""), encounterDate: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU CREATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, titleIen)
    vq.add_parameter(VistaQuery.LITERAL, text)
    vq.add_parameter(VistaQuery.LITERAL, encounterDate)
    return await session.s_query(vq)


@router.post("/api/surgery/save")
async def ws_save(ien: str = Query(...), text: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU UPDATE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    dhl = DictionaryHashList()
    if text:
        dhl.add("0", text)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.post("/api/surgery/sign")
async def ws_sign(ien: str = Query(...), esCode: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU SIGN RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, esCode)
    return await session.s_query(vq)


@router.post("/api/surgery/delete")
async def ws_delete(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU DELETE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.s_query(vq)


@router.post("/api/surgery/addendum")
async def ws_addendum(parentIen: str = Query(...), text: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU CREATE ADDENDUM RECORD")
    vq.add_parameter(VistaQuery.LITERAL, parentIen)
    dhl = DictionaryHashList()
    if text:
        dhl.add("0", text)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, "1")
    return await session.s_query(vq)


@router.post("/api/surgery/changetitle")
async def ws_change_title(ien: str = Query(...), titleIen: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU SET DOCUMENT TITLE")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, titleIen)
    return await session.s_query(vq)


@router.post("/api/surgery/lock")
async def ws_lock(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU LOCK RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.post("/api/surgery/unlock")
async def ws_unlock(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU UNLOCK RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.get("/api/surgery/requirescosignature")
async def ws_requires_cosignature(titleIen: str = Query(""), authorDuz: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU REQUIRES COSIGNATURE")
    vq.add_parameter(VistaQuery.LITERAL, titleIen)
    vq.add_parameter(VistaQuery.LITERAL, authorDuz)
    return await session.s_query(vq)


@router.get("/api/surgery/canchangecosigner")
async def ws_can_change_cosigner(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU CAN CHANGE COSIGNER")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.get("/api/surgery/additionalsigners")
async def ws_get_additional_signers(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU GET ADDITIONAL SIGNERS")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.post("/api/surgery/additionalsigners")
async def ws_update_additional_signers(ien: str = Query(...), signerDuz: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU UPDATE ADDITIONAL SIGNERS")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, signerDuz)
    return await session.s_query(vq)


@router.get("/api/surgery/authorization")
async def ws_authorization(ien: str = Query(...), action: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU AUTHORIZATION")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, action)
    return await session.s_query(vq)


@router.get("/api/surgery/justifydelete")
async def ws_justify_delete(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU JUSTIFY DELETE?")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.post("/api/surgery/deletewreason")
async def ws_delete_with_reason(ien: str = Query(...), reason: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU DELETE RECORD")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    vq.add_parameter(VistaQuery.LITERAL, reason)
    return await session.s_query(vq)


@router.get("/api/surgery/lastsaveclean")
async def ws_last_save_clean(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU WAS THIS SAVED?")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.get("/api/surgery/iscosigneddoc")
async def ws_is_cosigned_doc(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU COSIGN DOCUMENT")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.get("/api/surgery/boilerplate")
async def ws_boilerplate(titleIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE GET BOILERPLATE")
    vq.add_parameter(VistaQuery.LITERAL, titleIen)
    return await session.t_query(vq)


@router.get("/api/surgery/printtext")
async def ws_print_text(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU DETAILED DISPLAY")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.get("/api/surgery/pcedata")
async def ws_pce_data(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE PCE4NOTE")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)
