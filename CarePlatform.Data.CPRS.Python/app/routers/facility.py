# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Facility router — mirrors FacilityController.cs.

Remote facility access, HL7 link check, VistaWeb, JLV.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/facility/remotefacilities")
async def remote_facilities(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWCIRN FACLIST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/facility/checkhl7link")
async def check_hl7_link(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCIRN CHECKLINK")
    return await session.s_query(vq)


@router.get("/api/facility/vistawebaddress")
async def vista_web_address(value: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCIRN WEBADDR")
    vq.add_parameter(VistaQuery.LITERAL, value)
    return await session.s_query(vq)


@router.get("/api/facility/jlvlabel")
async def jlv_label(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWCIRN JLV LABEL")
    return await session.s_query(vq)


@router.get("/api/facility/checkremotepatient")
async def check_remote_patient(patient: str = Query(...), site: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("XWB DIRECT RPC")
    vq.add_parameter(VistaQuery.LITERAL, site)
    vq.add_parameter(VistaQuery.LITERAL, "ORWCIRN RESTRICT")
    vq.add_parameter(VistaQuery.LITERAL, "0")
    vq.add_parameter(VistaQuery.LITERAL, patient)
    return await session.t_query(vq)
