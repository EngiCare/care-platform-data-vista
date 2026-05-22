# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Template router — mirrors TemplateController.cs.

Template trees, boilerplate, text expansion, CRUD, access, defaults, locking,
linked data, reminder dialogs, template fields.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Template Tree ───────────────────────────────────────────────────

@router.get("/api/template/roots")
async def get_roots(userDuz: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE GETROOTS")
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    return await session.t_query(vq)


@router.get("/api/template/items")
async def get_items(templateId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE GETITEMS")
    vq.add_parameter(VistaQuery.LITERAL, templateId)
    return await session.t_query(vq)


@router.post("/api/template/setitems")
async def set_items(templateId: str = Query(...), children: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE SET ITEMS")
    vq.add_parameter(VistaQuery.LITERAL, templateId)
    dhl = DictionaryHashList()
    for i, child in enumerate(children):
        dhl.add(str(i + 1), child)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


# ── Boilerplate & Text ─────────────────────────────────────────────

@router.get("/api/template/boilerplate")
async def get_boilerplate(templateId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE GETBOIL")
    vq.add_parameter(VistaQuery.LITERAL, templateId)
    return await session.t_query(vq)


@router.post("/api/template/gettext")
async def get_text(dfn: str = Query(...), visitStr: str = Query(...), boilerplate: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE GETTEXT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, visitStr)
    dhl = DictionaryHashList()
    for i, line in enumerate(boilerplate):
        dhl.add(f"{i + 1},0", line)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/template/checkboilerplate")
async def check_boilerplate(boilerplate: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE CHECK BOILERPLATE")
    dhl = DictionaryHashList()
    for i, line in enumerate(boilerplate):
        dhl.add(f"2,{i + 1},0", line)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/template/titleboilerplate")
async def get_title_boilerplate(titleIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU GET BOILERPLATE")
    vq.add_parameter(VistaQuery.LITERAL, titleIen)
    return await session.t_query(vq)


@router.get("/api/template/boilerplatedtitles")
async def boilerplated_titles(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU LONG LIST BOILERPLATED")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/template/alltitles")
async def all_titles(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE ALL TITLES")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


# ── Template CRUD ───────────────────────────────────────────────────

@router.post("/api/template/createmodify")
async def create_modify(templateId: str = Query(...), fields: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE CREATE/MODIFY")
    vq.add_parameter(VistaQuery.LITERAL, templateId)
    dhl = DictionaryHashList()
    for field in fields:
        eq_pos = field.find("=")
        if eq_pos > 0:
            key = field[:eq_pos]
            val = field[eq_pos + 1:]
            dhl.add(key, val)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/template/delete")
async def delete_template(templateIds: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE DELETE")
    dhl = DictionaryHashList()
    for i, tid in enumerate(templateIds):
        dhl.add(str(i + 1), tid)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


# ── Access & Editor ─────────────────────────────────────────────────

@router.get("/api/template/iseditor")
async def is_editor(templateId: str = Query(...), userDuz: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU TEMPLATE ISEDITOR")
    vq.add_parameter(VistaQuery.LITERAL, templateId)
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    return await session.s_query(vq)


@router.get("/api/template/accesslevel")
async def access_level(templateId: str = Query(...), userDuz: int = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU TEMPLATE ACCESS LEVEL")
    vq.add_parameter(VistaQuery.LITERAL, templateId)
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


# ── Defaults & Description ─────────────────────────────────────────

@router.get("/api/template/defaults")
async def get_defaults(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU TEMPLATE GET DEFAULTS")
    return await session.s_query(vq)


@router.post("/api/template/setdefaults")
async def set_defaults(defaults: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU TEMPLATE SET DEFAULTS")
    vq.add_parameter(VistaQuery.LITERAL, defaults)
    return await session.s_query(vq)


@router.get("/api/template/description")
async def get_description(templateIen: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE GET DESCRIPTION")
    vq.add_parameter(VistaQuery.LITERAL, templateIen)
    return await session.t_query(vq)


# ── Locking ─────────────────────────────────────────────────────────

@router.post("/api/template/lock")
async def lock_template(templateId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU TEMPLATE LOCK")
    vq.add_parameter(VistaQuery.LITERAL, templateId)
    return await session.s_query(vq)


@router.post("/api/template/unlock")
async def unlock_template(templateId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU TEMPLATE UNLOCK")
    vq.add_parameter(VistaQuery.LITERAL, templateId)
    return await session.s_query(vq)


# ── Linked Data & Objects ──────────────────────────────────────────

@router.get("/api/template/getlink")
async def get_link(link: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU TEMPLATE GETLINK")
    vq.add_parameter(VistaQuery.LITERAL, link)
    return await session.s_query(vq)


@router.get("/api/template/objects")
async def get_objects(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU GET LIST OF OBJECTS")
    return await session.t_query(vq)


@router.get("/api/template/personalobjects")
async def personal_objects(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU TEMPLATE PERSONAL OBJECTS")
    return await session.t_query(vq)


# ── Reminder Dialogs ───────────────────────────────────────────────

@router.get("/api/template/reminderdialogs")
async def reminder_dialogs(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU REMINDER DIALOGS")
    return await session.t_query(vq)


@router.get("/api/template/remdlgok")
async def rem_dlg_ok_as_template(remDlgIen: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU REM DLG OK AS TEMPLATE")
    vq.add_parameter(VistaQuery.LITERAL, remDlgIen)
    return await session.s_query(vq)


# ── Template Fields ─────────────────────────────────────────────────

@router.get("/api/template/fieldlist")
async def field_list(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU FIELD LIST")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/template/fieldload")
async def field_load(fieldName: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU FIELD LOAD")
    vq.add_parameter(VistaQuery.LITERAL, fieldName)
    return await session.t_query(vq)


@router.get("/api/template/fieldloadbyien")
async def field_load_by_ien(ien: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU FIELD LOAD BY IEN")
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.get("/api/template/fieldcanedit")
async def field_can_edit(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU FIELD CAN EDIT")
    return await session.s_query(vq)


@router.post("/api/template/fieldsave")
async def field_save(fieldId: str = Query(...), fields: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU FIELD SAVE")
    vq.add_parameter(VistaQuery.LITERAL, fieldId)
    dhl = DictionaryHashList()
    for field in fields:
        eq_pos = field.find("=")
        if eq_pos > 0:
            key = field[:eq_pos]
            val = field[eq_pos + 1:]
            dhl.add(key, val)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/template/fieldlock")
async def field_lock(fieldId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU FIELD LOCK")
    vq.add_parameter(VistaQuery.LITERAL, fieldId)
    return await session.s_query(vq)


@router.post("/api/template/fieldunlock")
async def field_unlock(fieldId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU FIELD UNLOCK")
    vq.add_parameter(VistaQuery.LITERAL, fieldId)
    return await session.s_query(vq)


@router.post("/api/template/fielddelete")
async def field_delete(fieldId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU FIELD DELETE")
    vq.add_parameter(VistaQuery.LITERAL, fieldId)
    return await session.s_query(vq)


@router.post("/api/template/fieldexport")
async def field_export(fieldList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU FIELD EXPORT")
    dhl = DictionaryHashList()
    for i, item in enumerate(fieldList):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/template/fieldimport")
async def field_import(fieldList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU FIELD IMPORT")
    dhl = DictionaryHashList()
    for i, item in enumerate(fieldList):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/template/fieldnameunique")
async def field_name_is_unique(fieldName: str = Query(...), ien: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU FIELD NAME IS UNIQUE")
    vq.add_parameter(VistaQuery.LITERAL, fieldName)
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.s_query(vq)


@router.get("/api/template/fieldcheck")
async def field_check(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU FIELD CHECK")
    return await session.t_query(vq)


@router.post("/api/template/dolmtext")
async def do_lm_text(text: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU FIELD DOLMTEXT")
    dhl = DictionaryHashList()
    for i, line in enumerate(text):
        dhl.add(f"{i + 1},0", line)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/template/fieldlistadd")
async def field_list_add(xmlData: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU FIELD LIST ADD")
    dhl = DictionaryHashList()
    for i, item in enumerate(xmlData):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/template/fieldlistimport")
async def field_list_import(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("TIU FIELD LIST IMPORT")
    return await session.t_query(vq)
