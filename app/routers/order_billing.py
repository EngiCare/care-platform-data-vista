# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Order billing router — mirrors OrderBillingController.cs.

Billing-related order data — treatment factors, personal Dx, ORWDBA RPCs.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Master Status / Treatment Factors ─────────────────────────────

@router.get("/api/orderbilling/masterstatus")
async def master_status(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDBA1 SCLST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/orderbilling/getdefaults")
async def get_defaults(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDBA1 GETORDX")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/orderbilling/treatmentfactors")
async def treatment_factors(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDBA3 HINTS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/orderbilling/hasvisit")
async def has_visit(dfn: str = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDBA7 GETIEN")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


@router.get("/api/orderbilling/orderconsult")
async def order_consult(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDBA8 ORCSLT")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


# ── Personal Dx List ──────────────────────────────────────────────

@router.get("/api/orderbilling/personaldxlist")
async def personal_dx_list(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDBA4 GETPDXL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.post("/api/orderbilling/personaldxlist")
async def save_personal_dx_list(dfn: str = Query(...), dxList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDBA4 SAVPDXL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    dhl = DictionaryHashList()
    for i, dx in enumerate(dxList):
        dhl.add(str(i), dx)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.get("/api/orderbilling/defaultdx")
async def default_dx(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDBA1 ORPKGTYP")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


# ── Hints / ICD ──────────────────────────────────────────────────

@router.get("/api/orderbilling/orderable")
async def orderable(orderId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDBA2 GETDUDC")
    vq.add_parameter(VistaQuery.LITERAL, orderId)
    return await session.s_query(vq)


@router.get("/api/orderbilling/dxhint")
async def dx_hint(oi: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDBA2 ADDPDL")
    vq.add_parameter(VistaQuery.LITERAL, oi)
    return await session.s_query(vq)


@router.get("/api/orderbilling/icdlookup")
async def icd_lookup(searchText: str = Query(...), dateTime: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDBA5 STSAREA")
    vq.add_parameter(VistaQuery.LITERAL, searchText)
    vq.add_parameter(VistaQuery.LITERAL, dateTime)
    return await session.t_query(vq)


@router.get("/api/orderbilling/icdcodename")
async def icd_code_name(icdIen: str = Query(...), dateTime: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDBA7 ISWITCH")
    vq.add_parameter(VistaQuery.LITERAL, icdIen)
    vq.add_parameter(VistaQuery.LITERAL, dateTime)
    return await session.s_query(vq)
