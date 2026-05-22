# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""User router — mirrors UserController.cs.

User info, security keys, permissions, person/location lookups, divisions.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.models.user import UserInfo
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── User Info & Permissions ─────────────────────────────────────────

@router.get("/api/user/info")
async def info(session: ISession = Depends(get_current_session)) -> UserInfo:
    vq = VistaQuery("ORWU USERINFO")
    result = await session.s_query(vq)
    return UserInfo.parse(result)


@router.get("/api/user/param")
async def param(paramName: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU PARAM")
    vq.add_parameter(VistaQuery.LITERAL, paramName)
    return await session.s_query(vq)


@router.get("/api/user/sysparam")
async def sys_param(duz: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU SYSPARAM")
    vq.add_parameter(VistaQuery.LITERAL, str(duz))
    return await session.s_query(vq)


@router.get("/api/user/haskey")
async def has_key(keyName: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU HASKEY")
    vq.add_parameter(VistaQuery.LITERAL, keyName)
    return await session.s_query(vq)


@router.get("/api/user/personhaskey")
async def person_has_key(personIen: int = Query(...), keyName: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU NPHASKEY")
    vq.add_parameter(VistaQuery.LITERAL, str(personIen))
    vq.add_parameter(VistaQuery.LITERAL, keyName)
    return await session.s_query(vq)


@router.get("/api/user/hasoptionaccess")
async def has_option_access(optionName: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU HAS OPTION ACCESS")
    vq.add_parameter(VistaQuery.LITERAL, optionName)
    return await session.s_query(vq)


@router.post("/api/user/validatesignature")
async def validate_signature(esCode: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU VALIDSIG")
    vq.add_encrypted_parameter(VistaQuery.LITERAL, esCode)
    return await session.s_query(vq)


# ── General Lookups ─────────────────────────────────────────────────

@router.get("/api/user/externalname")
async def external_name(ien: int = Query(...), fileNumber: float = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU EXTNAME")
    vq.add_parameter(VistaQuery.LITERAL, str(ien))
    vq.add_parameter(VistaQuery.LITERAL, str(fileNumber))
    return await session.s_query(vq)


@router.get("/api/user/globalref")
async def global_ref(fileId: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU GBLREF")
    vq.add_parameter(VistaQuery.LITERAL, fileId)
    return await session.s_query(vq)


@router.get("/api/user/generic")
async def generic(startFrom: str = Query(...), direction: int = Query(...), globalRef: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU GENERIC")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    vq.add_parameter(VistaQuery.LITERAL, globalRef)
    return await session.t_query(vq)


@router.get("/api/user/fmdate")
async def fm_date(dateString: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU DT")
    vq.add_parameter(VistaQuery.LITERAL, dateString)
    return await session.s_query(vq)


@router.get("/api/user/validatedate")
async def validate_date(dateString: str = Query(...), flags: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU VALDT")
    vq.add_parameter(VistaQuery.LITERAL, dateString)
    vq.add_parameter(VistaQuery.LITERAL, flags)
    return await session.s_query(vq)


# ── Person & Location Subsets ───────────────────────────────────────

@router.get("/api/user/persons")
async def persons(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU NEWPERS")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/user/providers")
async def providers(startFrom: str = Query(...), direction: int = Query(...), dateTime: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU NEWPERS")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    vq.add_parameter(VistaQuery.LITERAL, "PROVIDER")
    if dateTime:
        vq.add_parameter(VistaQuery.LITERAL, dateTime)
    return await session.t_query(vq)


@router.get("/api/user/userswithclass")
async def users_with_class(startFrom: str = Query(...), direction: int = Query(...), dateTime: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU NEWPERS")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, dateTime)
    return await session.t_query(vq)


@router.get("/api/user/allpersons")
async def all_persons(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU NEWPERS")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, "")
    vq.add_parameter(VistaQuery.LITERAL, "1")
    return await session.t_query(vq)


@router.get("/api/user/cosigners")
async def cosigners(startFrom: str = Query(...), direction: int = Query(...), fmDate: str = Query(...), docType: int = Query(0), title: int = Query(0), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU2 COSIGNER")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    vq.add_parameter(VistaQuery.LITERAL, fmDate)
    vq.add_parameter(VistaQuery.LITERAL, str(docType))
    vq.add_parameter(VistaQuery.LITERAL, str(title))
    return await session.t_query(vq)


@router.get("/api/user/devices")
async def devices(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU DEVICE")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/user/hospitallocations")
async def hospital_locations(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU HOSPLOC")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/user/cliniclocations")
async def clinic_locations(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU CLINLOC")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/user/inpatientlocations")
async def inpatient_locations(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU INPLOC")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/user/newlocations")
async def new_locations(startFrom: str = Query(...), direction: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU1 NEWLOC")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/user/nameconvert")
async def name_convert(providerIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU1 NAMECVT")
    vq.add_parameter(VistaQuery.LITERAL, str(providerIen))
    return await session.s_query(vq)


@router.get("/api/user/defaultprinter")
async def default_printer(duz: int = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWRP GET DEFAULT PRINTER")
    vq.add_parameter(VistaQuery.LITERAL, str(duz))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


# ── Simplified Aliases ──────────────────────────────────────────────

@router.get("/api/user/search")
async def search_users(search: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWU NEWPERS")
    vq.add_parameter(VistaQuery.LITERAL, search)
    vq.add_parameter(VistaQuery.LITERAL, "1")
    return await session.t_query(vq)


@router.get("/api/location/clinics")
async def location_clinics(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT CLINICS")
    return await session.t_query(vq)


# ── Division Selection ──────────────────────────────────────────────

@router.get("/api/user/divisions")
async def divisions(session: ISession = Depends(get_current_session)) -> list[str]:
    return session.cached_division_lines or ["0"]


@router.post("/api/user/division/set")
async def set_division(stationNumber: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    await session.select_division_and_set_context(stationNumber)
    return "1"
