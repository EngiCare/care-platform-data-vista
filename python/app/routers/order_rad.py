# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Order rad router — mirrors OrderRadController.cs.

Radiology order dialogs — ORWDRA32 RPCs.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/orderrad/defaults")
async def od_for_rad(dfn: str = Query(...), eventDiv: str = Query(...), imagingType: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDRA32 DEF")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, eventDiv)
    vq.add_parameter(VistaQuery.LITERAL, str(imagingType))
    return await session.t_query(vq)


@router.get("/api/orderrad/islonglist")
async def is_rad_procs_long_list(imagingType: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDRA32 RADLONG")
    vq.add_parameter(VistaQuery.LITERAL, str(imagingType))
    return await session.s_query(vq)


@router.get("/api/orderrad/procedures")
async def subset_of_rad_procs(imagingType: int = Query(...), startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDRA32 RAORDITM")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    vq.add_parameter(VistaQuery.LITERAL, str(imagingType))
    return await session.t_query(vq)


@router.get("/api/orderrad/procmessage")
async def imaging_message(ien: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDRA32 PROCMSG")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    return await session.s_query(vq)


@router.get("/api/orderrad/isolation")
async def patient_on_isolation(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDRA32 ISOLATN")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/orderrad/radiologists")
async def subset_of_radiologists(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDRA32 APPROVAL")
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.t_query(vq)


@router.get("/api/orderrad/imagingtypes")
async def subset_of_imaging_types(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDRA32 IMTYPSEL")
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.t_query(vq)


@router.get("/api/orderrad/sources")
async def subset_of_rad_sources(srcType: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWDRA32 RADSRC")
    vq.add_parameter(VistaQuery.LITERAL, srcType)
    return await session.t_query(vq)


@router.get("/api/orderrad/locationtype")
async def location_type(location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWDRA32 LOCTYPE")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)
