# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Order dialog router — mirrors OrderDialogController.cs.

Order dialogs, menus, order sets, quick orders, prompts, order checks.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query
from typing import Optional

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Dialog Definitions ────────────────────────────────────────────

@router.get("/api/orderdialog/definition")
async def definition(dialogName: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX DLGDEF")
    vq.add_parameter(VistaQuery.LITERAL, dialogName)
    return await session.t_query(vq)


@router.get("/api/orderdialog/loadresponses")
async def load_responses(orderId: str = Query(...), transfer: bool = Query(False), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX LOADRSP")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    vq.add_parameter(VistaQuery.LITERAL, "1" if transfer else "0")
    return await session.t_query(vq)


@router.get("/api/orderdialog/buildresponses")
async def build_responses(inputId: str = Query(...), extra: str = Query(""), forIMO: bool = Query(False), location: int = Query(0), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXM1 BLDQRSP")
    vq.add_parameter(VistaQuery.LITERAL, inputId)
    vq.add_parameter(VistaQuery.LITERAL, extra)
    vq.add_parameter(VistaQuery.LITERAL, "1" if forIMO else "0")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.get("/api/orderdialog/dialogfororder")
async def dialog_for_order(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX DLGID")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/orderdialog/formfororder")
async def form_for_order(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX FORMID")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/orderdialog/formfordialog")
async def form_for_dialog(dialogIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXM FORMID")
    vq.add_parameter(VistaQuery.LITERAL, str(dialogIen))
    return await session.s_query(vq)


@router.get("/api/orderdialog/identify")
async def identify(dialog: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXM DLGNAME")
    vq.add_parameter(VistaQuery.LITERAL, dialog)
    return await session.s_query(vq)


@router.get("/api/orderdialog/disabledmessage")
async def disabled_message(dlgIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX DISMSG")
    vq.add_parameter(VistaQuery.LITERAL, str(dlgIen))
    return await session.s_query(vq)


@router.get("/api/orderdialog/dgroupname")
async def dgroup_name(name: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX DGNM")
    vq.add_parameter(VistaQuery.LITERAL, name)
    return await session.s_query(vq)


@router.get("/api/orderdialog/dgroupfordialog")
async def dgroup_for_dialog(dialogName: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX DGRP")
    vq.add_parameter(VistaQuery.LITERAL, dialogName)
    return await session.s_query(vq)


@router.get("/api/orderdialog/askanother")
async def ask_another(dialog: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDX AGAIN")
    vq.add_parameter(VistaQuery.LITERAL, dialog)
    return await session.s_query(vq)


@router.get("/api/orderdialog/oimessage")
async def oi_message(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX MSG")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.t_query(vq)


@router.get("/api/orderdialog/orderitems")
async def order_items(startFrom: str = Query(...), direction: int = Query(...), xref: str = Query(""), quickOrderDlgIen: int = Query(0), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX ORDITM")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    vq.add_parameter(VistaQuery.LITERAL, xref)
    vq.add_parameter(VistaQuery.LITERAL, str(quickOrderDlgIen))
    return await session.t_query(vq)


# ── Menus & Order Sets ────────────────────────────────────────────

@router.get("/api/orderdialog/menu")
async def menu(menuIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXM MENU")
    vq.add_parameter(VistaQuery.LITERAL, str(menuIen))
    return await session.t_query(vq)


@router.get("/api/orderdialog/orderset")
async def order_set(setIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXM LOADSET")
    vq.add_parameter(VistaQuery.LITERAL, str(setIen))
    return await session.t_query(vq)


@router.get("/api/orderdialog/writeorders")
async def write_orders(location: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDX WRLST")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.get("/api/orderdialog/menustyle")
async def menu_style(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXM MSTYLE")
    return await session.s_query(vq)


@router.get("/api/orderdialog/resolvescreen")
async def resolve_screen(reference: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXM RSCRN")
    vq.add_parameter(VistaQuery.LITERAL, reference)
    return await session.s_query(vq)


# ── Prompting ─────────────────────────────────────────────────────

@router.get("/api/orderdialog/prompts")
async def prompts(dialog: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXM PROMPTS")
    vq.add_parameter(VistaQuery.LITERAL, dialog)
    return await session.t_query(vq)


@router.post("/api/orderdialog/autoaccept")
async def auto_accept(dfn: str = Query(...), provider: int = Query(...), location: int = Query(...), dialog: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXM AUTOACK")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, dialog)
    return await session.t_query(vq)


# ── Quick Orders ──────────────────────────────────────────────────

@router.get("/api/orderdialog/quicklist")
async def quick_list(dGroup: str = Query(...), listType: str = Query("Q"), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXQ GETQLST")
    vq.add_parameter(VistaQuery.LITERAL, dGroup)
    vq.add_parameter(VistaQuery.LITERAL, listType)
    return await session.t_query(vq)


@router.get("/api/orderdialog/quickname")
async def quick_name(crc: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXQ GETQNAM")
    vq.add_parameter(VistaQuery.LITERAL, crc)
    return await session.s_query(vq)


@router.post("/api/orderdialog/quicklist")
async def save_quick_list(dGroup: str = Query(...), items: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXQ PUTQLST")
    vq.add_parameter(VistaQuery.LITERAL, dGroup)
    dhl = DictionaryHashList()
    for i, item in enumerate(items):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


# ── Order Checks ──────────────────────────────────────────────────

@router.get("/api/orderdialog/fillerid")
async def filler_id(dialogIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXC FILLID")
    vq.add_parameter(VistaQuery.LITERAL, str(dialogIen))
    return await session.s_query(vq)


@router.get("/api/orderdialog/checksenabled")
async def checks_enabled(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXC ON")
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.s_query(vq)


@router.get("/api/orderdialog/checksondisplay")
async def checks_on_display(dfn: str = Query(...), fillerId: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXC DISPLAY")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, fillerId)
    return await session.t_query(vq)


@router.post("/api/orderdialog/checksonaccept")
async def checks_on_accept(dfn: str = Query(...), fillerId: str = Query(...), startDtTm: str = Query(...), location: int = Query(...), dupORIFN: str = Query(""), renewal: bool = Query(False), oiList: Optional[list[str]] = Body(None), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXC ACCEPT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, fillerId)
    vq.add_parameter(VistaQuery.LITERAL, startDtTm)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    dhl = DictionaryHashList()
    if oiList:
        for i, oi in enumerate(oiList):
            dhl.add(str(i), oi)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, dupORIFN)
    vq.add_parameter(VistaQuery.LITERAL, "1" if renewal else "0")
    return await session.t_query(vq)


@router.post("/api/orderdialog/checksondelay")
async def checks_on_delay(dfn: str = Query(...), fillerId: str = Query(...), startDtTm: str = Query(...), location: int = Query(...), oiList: Optional[list[str]] = Body(None), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXC DELAY")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, fillerId)
    vq.add_parameter(VistaQuery.LITERAL, startDtTm)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    dhl = DictionaryHashList()
    if oiList:
        for i, oi in enumerate(oiList):
            dhl.add(str(i), oi)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/orderdialog/sessionchecks")
async def session_checks(dfn: str = Query(...), orderList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXC SESSION")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    dhl = DictionaryHashList()
    for i, o in enumerate(orderList):
        dhl.add(str(i), o)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/orderdialog/deletechecked")
async def delete_checked(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXC DELORD")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


# ── Lookups ───────────────────────────────────────────────────────

@router.get("/api/orderdialog/entries")
async def entries(startFrom: str = Query(...), direction: int = Query(...), xref: str = Query(""), gblRef: str = Query(""), screenRef: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDOR LKSCRN")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    vq.add_parameter(VistaQuery.LITERAL, xref)
    vq.add_parameter(VistaQuery.LITERAL, gblRef)
    vq.add_parameter(VistaQuery.LITERAL, screenRef)
    return await session.t_query(vq)


@router.get("/api/orderdialog/validatenum")
async def validate_num(value: str = Query(...), domain: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDOR VALNUM")
    vq.add_parameter(VistaQuery.LITERAL, value)
    vq.add_parameter(VistaQuery.LITERAL, domain)
    return await session.s_query(vq)


@router.post("/api/orderdialog/clearrecall")
async def clear_recall(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDXM2 CLRRCL")
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.t_query(vq)


@router.get("/api/orderdialog/isinpatientqo")
async def is_inpatient_qo(dlgId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXM3 ISUDQO")
    vq.add_parameter(VistaQuery.LITERAL, dlgId)
    return await session.s_query(vq)


@router.get("/api/orderdialog/issupply")
async def is_supply(dlgId: str = Query(...), qoDlg: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDXR01 ISSPLY")
    vq.add_parameter(VistaQuery.LITERAL, dlgId)
    vq.add_parameter(VistaQuery.LITERAL, qoDlg)
    return await session.s_query(vq)


@router.get("/api/orderdialog/dlgien")
async def dlg_ien(dlgName: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("OREVNTX1 DLGIEN")
    vq.add_parameter(VistaQuery.LITERAL, dlgName)
    return await session.s_query(vq)
