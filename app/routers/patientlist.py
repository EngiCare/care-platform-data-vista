# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""PatientList router — mirrors PatientListController.cs.

Patient list sources and queries — specialties, teams, wards, clinics, providers.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── List Sources ────────────────────────────────────────────────────

@router.get("/api/patientlist/defaultsource")
async def default_source(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQPT DEFAULT LIST SOURCE")
    return await session.s_query(vq)


@router.get("/api/patientlist/defaultclinicdaterange")
async def default_clinic_date_range(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQPT DEFAULT CLINIC DATE RANG")
    return await session.s_query(vq)


@router.get("/api/patientlist/defaultsort")
async def default_sort(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQPT DEFAULT LIST SORT")
    return await session.s_query(vq)


@router.get("/api/patientlist/specialties")
async def specialties(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT SPECIALTIES")
    return await session.t_query(vq)


@router.get("/api/patientlist/teams")
async def teams(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT TEAMS")
    return await session.t_query(vq)


@router.get("/api/patientlist/pcmmteams")
async def pcmm_teams(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT PTEAMPR")
    return await session.t_query(vq)


@router.get("/api/patientlist/wards")
@router.get("/api/patient/wards")
async def wards(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT WARDS")
    return await session.t_query(vq)


@router.get("/api/patientlist/clinics")
@router.get("/api/patient/clinics")
async def clinics(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT CLINICS")
    return await session.t_query(vq)


@router.get("/api/patientlist/defaultlist")
async def default_list(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT DEFAULT PATIENT LIST")
    return await session.t_query(vq)


# ── Patient Lists by Source ─────────────────────────────────────────

@router.get("/api/patientlist/byprovider")
async def by_provider(providerIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT PROVIDER PATIENTS")
    vq.add_parameter(VistaQuery.LITERAL, str(providerIen))
    return await session.t_query(vq)


@router.get("/api/patientlist/byteam")
async def by_team(teamIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT TEAM PATIENTS")
    vq.add_parameter(VistaQuery.LITERAL, str(teamIen))
    return await session.t_query(vq)


@router.get("/api/patientlist/bypcmmteam")
async def by_pcmm_team(teamIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT PTEAM PATIENTS")
    vq.add_parameter(VistaQuery.LITERAL, str(teamIen))
    return await session.t_query(vq)


@router.get("/api/patientlist/byspecialty")
async def by_specialty(specialtyIen: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQPT SPECIALTY PATIENTS")
    vq.add_parameter(VistaQuery.LITERAL, str(specialtyIen))
    return await session.t_query(vq)


@router.get("/api/patientlist/byclinic")
async def by_clinic(
    clinicIen: int = Query(...),
    firstDate: str = Query(""),
    lastDate: str = Query(""),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQPT CLINIC PATIENTS")
    vq.add_parameter(VistaQuery.LITERAL, str(clinicIen))
    vq.add_parameter(VistaQuery.LITERAL, firstDate)
    vq.add_parameter(VistaQuery.LITERAL, lastDate)
    return await session.t_query(vq)


# ── Restricted Patient List (RPL) ──────────────────────────────────

@router.post("/api/patientlist/makerpl")
async def make_rpl(rplList: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQPT MAKE RPL")
    vq.add_parameter(VistaQuery.LITERAL, rplList)
    return await session.s_query(vq)


@router.get("/api/patientlist/readrpl")
async def read_rpl(
    rplJobNumber: str = Query(...),
    startFrom: str = Query(...),
    direction: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORQPT READ RPL")
    vq.add_parameter(VistaQuery.LITERAL, rplJobNumber)
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.post("/api/patientlist/killrpl")
async def kill_rpl(rplJobNumber: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORQPT KILL RPL")
    vq.add_parameter(VistaQuery.LITERAL, rplJobNumber)
    return await session.s_query(vq)
