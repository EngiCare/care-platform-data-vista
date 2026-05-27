# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Options router — mirrors OptionsController.cs.

Notifications, order checks, surrogates, co-signers, document classes/titles,
lab/appt/imaging/encounter ranges, reminders, clinic defaults, patient lists/teams,
combos, report defaults, med ranges, copy-paste, tabs, and other settings.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Notifications ───────────────────────────────────────────────────

@router.get("/api/options/notifications")
async def get_notifications(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPP GETNOT")
    return await session.t_query(vq)


@router.post("/api/options/notifications")
async def save_notifications(items: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SAVENOT")
    dhl = DictionaryHashList()
    for i, item in enumerate(items):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.get("/api/options/notificationdefaults")
async def get_notification_defaults(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP GETNOTO")
    return await session.s_query(vq)


@router.post("/api/options/notificationdefaults")
async def save_notification_defaults(value: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SAVENOTO")
    vq.add_parameter(VistaQuery.LITERAL, value)
    return await session.s_query(vq)


@router.post("/api/options/notifications/clear")
async def clear_notifications(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP CLEARNOT")
    return await session.s_query(vq)


# ── Order Checks ────────────────────────────────────────────────────

@router.get("/api/options/orderchecks")
async def get_order_checks(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPP GETOC")
    return await session.t_query(vq)


@router.post("/api/options/orderchecks")
async def save_order_checks(items: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SAVEOC")
    dhl = DictionaryHashList()
    for i, item in enumerate(items):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── Surrogates ──────────────────────────────────────────────────────

@router.get("/api/options/surrogate")
async def get_surrogate_info(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP GETSURR")
    return await session.s_query(vq)


@router.get("/api/options/surrogate/check")
async def check_surrogate(surrogate: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP CHKSURR")
    vq.add_parameter(VistaQuery.LITERAL, str(surrogate))
    return await session.s_query(vq)


@router.post("/api/options/surrogate")
async def save_surrogate_info(value: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SAVESURR")
    vq.add_parameter(VistaQuery.LITERAL, value)
    return await session.s_query(vq)


# ── Co-signers ──────────────────────────────────────────────────────

@router.get("/api/options/cosigners")
async def get_cosigners(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPP GETCOS")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/options/cosigners/default")
async def get_default_cosigner(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP GETDCOS")
    return await session.s_query(vq)


@router.post("/api/options/cosigners/default")
async def set_default_cosigner(value: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SETDCOS")
    vq.add_parameter(VistaQuery.LITERAL, str(value))
    return await session.s_query(vq)


# ── Subjects ────────────────────────────────────────────────────────

@router.get("/api/options/subject")
async def get_subject(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP GETSUB")
    return await session.s_query(vq)


@router.post("/api/options/subject")
async def set_subject(value: bool = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SETSUB")
    vq.add_parameter(VistaQuery.LITERAL, "1" if value else "0")
    return await session.s_query(vq)


# ── Document Classes & Titles ──────────────────────────────────────

@router.get("/api/options/classes")
async def get_classes(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPN GETCLASS")
    return await session.t_query(vq)


@router.get("/api/options/titlesforclass")
async def get_titles_for_class(classValue: int = Query(...), startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU LONG LIST OF TITLES")
    vq.add_parameter(VistaQuery.LITERAL, str(classValue))
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/options/titlesforuser")
async def get_titles_for_user(classValue: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPP GETTU")
    vq.add_parameter(VistaQuery.LITERAL, str(classValue))
    return await session.t_query(vq)


@router.get("/api/options/titledefault")
async def get_title_default(classValue: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP GETTD")
    vq.add_parameter(VistaQuery.LITERAL, str(classValue))
    return await session.s_query(vq)


@router.post("/api/options/documentdefaults")
async def save_document_defaults(classValue: int = Query(...), titleDefault: int = Query(...), items: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SAVET")
    vq.add_parameter(VistaQuery.LITERAL, str(classValue))
    vq.add_parameter(VistaQuery.LITERAL, str(titleDefault))
    dhl = DictionaryHashList()
    for i, item in enumerate(items):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── Lab Days ────────────────────────────────────────────────────────

@router.get("/api/options/labdays")
async def get_lab_days(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPO CSLABD")
    return await session.s_query(vq)


@router.get("/api/options/labdays/user")
async def get_lab_user_days(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP CSLAB")
    return await session.s_query(vq)


# ── Appointment Days ───────────────────────────────────────────────

@router.get("/api/options/apptdays")
async def get_appt_days(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD1 GETCSDEF")
    return await session.s_query(vq)


@router.get("/api/options/apptdays/user")
async def get_appt_user_days(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD1 GETCSRNG")
    return await session.s_query(vq)


@router.post("/api/options/days")
async def set_days(values: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD1 PUTCSRNG")
    vq.add_parameter(VistaQuery.LITERAL, values)
    return await session.s_query(vq)


# ── Imaging Days ────────────────────────────────────────────────────

@router.get("/api/options/imagingdays")
async def get_imaging_days(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPO GETIMGD")
    return await session.s_query(vq)


@router.get("/api/options/imagingdays/user")
async def get_imaging_user_days(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP GETIMG")
    return await session.s_query(vq)


@router.post("/api/options/imagingdays")
async def set_imaging_days(maxNum: int = Query(...), startDays: int = Query(...), stopDays: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SETIMG")
    vq.add_parameter(VistaQuery.LITERAL, str(maxNum))
    vq.add_parameter(VistaQuery.LITERAL, str(startDays))
    vq.add_parameter(VistaQuery.LITERAL, str(stopDays))
    return await session.s_query(vq)


# ── Reminders ───────────────────────────────────────────────────────

@router.get("/api/options/reminders")
async def get_reminders(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPP GETREM")
    return await session.t_query(vq)


@router.post("/api/options/reminders")
async def set_reminders(items: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SETREM")
    dhl = DictionaryHashList()
    for i, item in enumerate(items):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── Clinic Defaults ─────────────────────────────────────────────────

@router.get("/api/options/clinicrange")
async def get_clinic_user_days(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP CLRANGE")
    return await session.s_query(vq)


@router.get("/api/options/clinicdays")
async def get_clinic_defaults(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP CLDAYS")
    return await session.s_query(vq)


@router.post("/api/options/clinicdefaults")
async def save_clinic_defaults(values: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SAVECD")
    vq.add_parameter(VistaQuery.LITERAL, values)
    return await session.s_query(vq)


@router.get("/api/options/listorder")
async def get_list_order(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SORTDEF")
    return await session.s_query(vq)


@router.get("/api/options/listsourcedefaults")
async def get_list_source_defaults(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP LSDEF")
    return await session.s_query(vq)


@router.post("/api/options/ptlistdefaults")
async def save_pt_list_defaults(values: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SAVEPLD")
    vq.add_parameter(VistaQuery.LITERAL, values)
    return await session.s_query(vq)


# ── Patient Lists & Teams ──────────────────────────────────────────

@router.get("/api/options/personallists")
async def get_personal_lists(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPT PERSONAL LISTS")
    return await session.t_query(vq)


@router.get("/api/options/allteams")
async def get_all_teams(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT TEAMS")
    return await session.t_query(vq)


@router.get("/api/options/teams")
async def get_teams(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPP TEAMS")
    return await session.t_query(vq)


@router.get("/api/options/ateams")
async def get_a_teams(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPT ATEAMS")
    return await session.t_query(vq)


@router.get("/api/options/pcmmteams")
async def get_pcmm_teams(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPP PCMTEAMS")
    return await session.t_query(vq)


@router.post("/api/options/lists/delete")
async def delete_list(listName: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP DELLIST")
    vq.add_parameter(VistaQuery.LITERAL, listName)
    return await session.s_query(vq)


@router.post("/api/options/lists/new")
async def new_list(listName: str = Query(...), visibility: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP NEWLIST")
    vq.add_parameter(VistaQuery.LITERAL, listName)
    vq.add_parameter(VistaQuery.LITERAL, str(visibility))
    return await session.s_query(vq)


@router.post("/api/options/lists/save")
async def save_list_changes(listIEN: int = Query(...), listVisibility: int = Query(...), items: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SAVELIST")
    dhl = DictionaryHashList()
    for i, item in enumerate(items):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, str(listIEN))
    vq.add_parameter(VistaQuery.LITERAL, str(listVisibility))
    return await session.s_query(vq)


@router.get("/api/options/teams/users")
async def list_users_by_team(teamId: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPT GETTEAM")
    vq.add_parameter(VistaQuery.LITERAL, str(teamId))
    return await session.t_query(vq)


@router.get("/api/options/pcmmteams/users")
async def list_users_by_pcmm_team(teamId: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPT GETPTEAM")
    vq.add_parameter(VistaQuery.LITERAL, str(teamId))
    return await session.t_query(vq)


@router.post("/api/options/lists/remove")
async def remove_list(listIEN: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP REMLIST")
    vq.add_parameter(VistaQuery.LITERAL, str(listIEN))
    return await session.s_query(vq)


@router.post("/api/options/lists/add")
async def add_list(listIEN: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP ADDLIST")
    vq.add_parameter(VistaQuery.LITERAL, str(listIEN))
    return await session.s_query(vq)


# ── Combos ──────────────────────────────────────────────────────────

@router.get("/api/options/combos")
async def get_combo(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPP GETCOMBO")
    return await session.t_query(vq)


@router.post("/api/options/combos")
async def set_combo(items: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SETCOMBO")
    dhl = DictionaryHashList()
    for i, item in enumerate(items):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


# ── Report Defaults ─────────────────────────────────────────────────

@router.get("/api/options/reportdefaults")
async def get_default_reports_setting(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD GETDFLT")
    return await session.s_query(vq)


@router.post("/api/options/reportdefaults/delete")
async def delete_user_level_reports_setting(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD DELDFLT")
    return await session.s_query(vq)


@router.post("/api/options/reportdefaults/activate")
async def activate_default_setting(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD ACTDF")
    return await session.s_query(vq)


@router.post("/api/options/reportdefaults")
async def set_default_reports_setting(value: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD SUDF")
    vq.add_parameter(VistaQuery.LITERAL, value)
    return await session.s_query(vq)


@router.post("/api/options/reportdefaults/individual")
async def set_individual_report_setting(value1: str = Query(...), value2: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD SUINDV")
    vq.add_parameter(VistaQuery.LITERAL, value1)
    vq.add_parameter(VistaQuery.LITERAL, value2)
    return await session.s_query(vq)


@router.get("/api/options/reportdefaults/retrieve")
async def retrieve_default_setting(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD RSDFLT")
    return await session.s_query(vq)


# ── Med Ranges ──────────────────────────────────────────────────────

@router.get("/api/options/medrange")
async def get_range_for_meds(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD GETOCM")
    return await session.s_query(vq)


@router.post("/api/options/medrange")
async def put_range_for_meds(value: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD PUTOCM")
    vq.add_parameter(VistaQuery.LITERAL, value)
    return await session.s_query(vq)


@router.get("/api/options/medrange/inpatient")
async def get_range_for_meds_in(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD GETOCMIN")
    return await session.s_query(vq)


@router.post("/api/options/medrange/inpatient")
async def put_range_for_meds_in(value: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD PUTOCMIN")
    vq.add_parameter(VistaQuery.LITERAL, value)
    return await session.s_query(vq)


@router.get("/api/options/medrange/outpatient")
async def get_range_for_meds_op(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD GETOCMOP")
    return await session.s_query(vq)


@router.post("/api/options/medrange/outpatient")
async def put_range_for_meds_op(value: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD PUTOCMOP")
    vq.add_parameter(VistaQuery.LITERAL, value)
    return await session.s_query(vq)


# ── Encounter Ranges ───────────────────────────────────────────────

@router.get("/api/options/encounterrange/defaults")
async def get_range_for_encs_default(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD1 GETEFDAT")
    return await session.s_query(vq)


@router.get("/api/options/encounterrange")
async def get_range_for_encs_user(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD1 GETEDATS")
    return await session.s_query(vq)


@router.post("/api/options/encounterrange")
async def put_range_for_encs(values: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD1 PUTEDATS")
    vq.add_parameter(VistaQuery.LITERAL, values)
    return await session.s_query(vq)


@router.get("/api/options/encounterrange/futuredays")
async def get_enc_future_days(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPD1 GETEAFL")
    return await session.s_query(vq)


# ── Other / Tabs / Copy-Paste ──────────────────────────────────────

@router.get("/api/options/other")
async def get_other(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP GETOTHER")
    return await session.s_query(vq)


@router.post("/api/options/other")
async def set_other(info: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTPP SETOTHER")
    vq.add_parameter(VistaQuery.LITERAL, info)
    return await session.s_query(vq)


@router.get("/api/options/tabs")
async def get_other_tabs(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWTPO GETTABS")
    return await session.t_query(vq)


@router.get("/api/options/copypaste")
async def get_copy_paste(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTIU LDCPIDNT")
    return await session.s_query(vq)


@router.post("/api/options/copypaste")
async def save_copy_paste(value: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWTIU SVCPIDNT")
    vq.add_parameter(VistaQuery.LITERAL, value)
    return await session.s_query(vq)


# ── Web.CPRS Convenience Endpoints ─────────────────────────────────

@router.post("/api/options/newlist")
async def new_list_alias(name: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPT SAVE LIST")
    vq.add_parameter(VistaQuery.LITERAL, name)
    return await session.s_query(vq)


@router.post("/api/options/deletelist")
async def delete_list_alias(ien: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPT DELETE LIST")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)
