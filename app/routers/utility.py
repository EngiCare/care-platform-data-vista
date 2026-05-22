# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Utility router — mirrors UtilityController.cs.

Primary-care detail, tool menu, symbol table, version checks, patch checks,
RPC availability, font/size persistence, share node, DLL version checks,
COM object hooks, real-time-consult info.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Primary Care ────────────────────────────────────────────────────

@router.get("/api/utility/primarycaredetail")
async def detail_primary_care(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPT1 PCDETAIL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


# ── Tool Menu ───────────────────────────────────────────────────────

@router.get("/api/utility/toolmenu")
async def get_tool_menu(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU TOOLMENU")
    return await session.t_query(vq)


# ── Symbol Table ────────────────────────────────────────────────────

@router.get("/api/utility/symboltable")
async def list_symbol_table(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWUX SYMTAB")
    return await session.t_query(vq)


# ── Server Variables ───────────────────────────────────────────────

@router.get("/api/utility/getvar")
async def get_variable_value(reference: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("XWB GET VARIABLE VALUE")
    vq.add_parameter(VistaQuery.REFERENCE, reference)
    return await session.s_query(vq)


# ── Patch / Version / Availability ─────────────────────────────────

@router.get("/api/utility/haspatch")
async def server_has_patch(patchId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU PATCH")
    vq.add_parameter(VistaQuery.LITERAL, patchId)
    return await session.s_query(vq)


@router.get("/api/utility/spansintldateline")
async def site_spans_intl_date_line(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU OVERDL")
    return await session.s_query(vq)


@router.post("/api/utility/rpcsavailable")
async def are_rpcs_available(rpcs: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("XWB ARE RPCS AVAILABLE")
    vq.add_parameter(VistaQuery.LITERAL, "L")
    dhl = DictionaryHashList()
    for i, rpc in enumerate(rpcs):
        dhl.add(str(i), rpc)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/utility/serverversion")
async def server_version(option: str = Query(...), verClient: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU VERSRV")
    vq.add_parameter(VistaQuery.LITERAL, option)
    vq.add_parameter(VistaQuery.LITERAL, verClient)
    return await session.s_query(vq)


@router.get("/api/utility/packageversion")
async def package_version(ns: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU VERSION")
    vq.add_parameter(VistaQuery.LITERAL, ns)
    return await session.s_query(vq)


# ── Font & Layout Persistence ──────────────────────────────────────

@router.get("/api/utility/fontsize")
async def load_font_size(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCH LDFONT")
    return await session.s_query(vq)


@router.post("/api/utility/fontsize")
async def save_font_size(fontSize: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCH SAVFONT")
    vq.add_parameter(VistaQuery.LITERAL, str(fontSize))
    return await session.s_query(vq)


@router.get("/api/utility/sizesall")
async def load_all_sizes(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWCH LOADALL")
    return await session.t_query(vq)


@router.post("/api/utility/sizesall")
async def save_all_sizes(items: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCH SAVEALL")
    dhl = DictionaryHashList()
    for i, item in enumerate(items):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.get("/api/utility/size")
async def load_size(name: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCH LOADSIZ")
    vq.add_parameter(VistaQuery.LITERAL, name)
    return await session.s_query(vq)


# ── Share Node ──────────────────────────────────────────────────────

@router.post("/api/utility/sharenode")
async def set_share_node(ip: str = Query(...), handle: str = Query(...), dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPT SHARE")
    vq.add_parameter(VistaQuery.LITERAL, ip)
    vq.add_parameter(VistaQuery.LITERAL, handle)
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


# ── DLL Version Check ──────────────────────────────────────────────

@router.get("/api/utility/dllversioncheck")
async def dll_version_check(dllName: str = Query(...), dllVersion: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORUTL4 DLL")
    vq.add_parameter(VistaQuery.LITERAL, dllName)
    vq.add_parameter(VistaQuery.LITERAL, dllVersion)
    return await session.s_query(vq)


# ── COM Object Hooks ───────────────────────────────────────────────

@router.get("/api/utility/com/patientchange")
async def get_patient_change_guids(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCOM PTOBJ")
    return await session.s_query(vq)


@router.get("/api/utility/com/orderaccept")
async def get_order_accept_guids(displayGroup: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCOM ORDEROBJ")
    vq.add_parameter(VistaQuery.LITERAL, str(displayGroup))
    return await session.s_query(vq)


@router.get("/api/utility/com/objects")
async def get_all_active_com_objects(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWCOM GETOBJS")
    return await session.t_query(vq)


@router.get("/api/utility/com/details")
async def get_com_object_details(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCOM DETAILS")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


# ── Real-Time Consult Info ─────────────────────────────────────────

@router.get("/api/utility/rtcinfo")
async def get_additional_info(locIen: str = Query(...), what: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDSD1 GETINFO")
    vq.add_parameter(VistaQuery.LITERAL, locIen)
    vq.add_parameter(VistaQuery.LITERAL, what)
    return await session.t_query(vq)
