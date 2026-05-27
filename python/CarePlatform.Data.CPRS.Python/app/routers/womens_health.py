# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Women's Health router — mirrors WomensHealthController.cs.

Pregnancy, lactation, conception status tracking.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/womenshealth/isvalid")
async def is_valid_patient(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("WV IS PATIENT FEMALE")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/womenshealth/pregnancystatus")
async def get_pregnancy_status(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("WV PREGNANCY STATUS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/womenshealth/lactationstatus")
async def get_lactation_status(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("WV LACTATION STATUS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/womenshealth/abletoconceive")
async def get_able_to_conceive(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("WV ABLE TO CONCEIVE")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/womenshealth/patientstatus")
async def get_patient_status(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("WV PATIENT STATUS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.post("/api/womenshealth/savepreglac")
async def save_preg_lac_data(dfn: str = Query(...), data: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("WV SAVE PREGLAC DATA")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    dhl = DictionaryHashList()
    for i, item in enumerate(data):
        dhl.add(str(i), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.post("/api/womenshealth/enteredinerror")
async def mark_entered_in_error(itemId: str = Query(...), reason: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("WV ENTERED IN ERROR")
    vq.add_parameter(VistaQuery.LITERAL, itemId)
    vq.add_parameter(VistaQuery.LITERAL, reason or "")
    return await session.s_query(vq)


@router.get("/api/womenshealth/askfordata")
async def ask_for_data(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("WV ASK FOR DATA")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/womenshealth/websites")
async def get_websites(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("WV GET WEBSITES")
    return await session.t_query(vq)
