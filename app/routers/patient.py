# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Patient router — mirrors PatientController.cs.

Patient selection, identification, demographics, lookup, admissions,
sensitive-record access, cross-reference, and cover-sheet widgets.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.models.patient import (
    PatientDemographics,
    PatientIdInfo,
    PatientPrimaryCare,
    PatientSearchResult,
    SensitiveRecordResult,
)
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Patient Selection & Demographics ────────────────────────────────

@router.get("/api/patient/select")
async def select(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> PatientDemographics:
    vq = VistaQuery("ORWPT SELECT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    result = await session.s_query(vq)
    return PatientDemographics.parse(dfn, result)


@router.get("/api/patient/primarycare")
async def primary_care(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> PatientPrimaryCare:
    vq = VistaQuery("ORWPT1 PRCARE")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    result = await session.s_query(vq)
    return PatientPrimaryCare.parse(result)


@router.get("/api/patient/idinfo")
async def id_info(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> PatientIdInfo:
    vq = VistaQuery("ORWPT ID INFO")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    result = await session.s_query(vq)
    return PatientIdInfo.parse(result)


@router.get("/api/patient/dateofdeath")
async def date_of_death(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWPT DIEDON")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/patient/inpatientlocation")
async def inpatient_location(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWPT INPLOC")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/patient/restrictedcheck")
async def restricted_check(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWPT SELCHK")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/patient/encountertext")
async def encounter_text(
    dfn: str = Query(...),
    location: int = Query(0),
    provider: int = Query(0),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWPT ENCTITL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    return await session.s_query(vq)


@router.get("/api/patient/legacy")
async def legacy(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWPT LEGACY")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/patient/otherinformation")
async def other_information(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWPT2 COVID")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/patient/otherinformationdetail")
async def other_information_detail(
    dfn: str = Query(...),
    valueType: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWOTHER DETAIL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, valueType)
    return await session.t_query(vq)


# ── Patient Lookup / Search ─────────────────────────────────────────

@router.get("/api/patient/top")
async def top(
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWPT TOP")
    return await session.t_query(vq)


@router.get("/api/patient/listall")
async def list_all(
    startFrom: str = Query(...),
    direction: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWPT LIST ALL")
    vq.add_parameter(VistaQuery.LITERAL, startFrom)
    vq.add_parameter(VistaQuery.LITERAL, str(direction))
    return await session.t_query(vq)


@router.get("/api/patient/searchlast5")
async def search_last5(
    last5: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[PatientSearchResult]:
    vq = VistaQuery("ORWPT LAST5")
    vq.add_parameter(VistaQuery.LITERAL, last5.upper())
    results = await session.t_query(vq)
    return PatientSearchResult.parse_list(results)


@router.get("/api/patient/searchlast5rpl")
async def search_last5_rpl(
    last5: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWPT LAST5 RPL")
    vq.add_parameter(VistaQuery.LITERAL, last5.upper())
    return await session.t_query(vq)


@router.get("/api/patient/searchfullssn")
async def search_full_ssn(
    fullSsn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[PatientSearchResult]:
    ssn = fullSsn.replace("-", "").upper()
    vq = VistaQuery("ORWPT FULLSSN")
    vq.add_parameter(VistaQuery.LITERAL, ssn)
    results = await session.t_query(vq)
    return PatientSearchResult.parse_list(results)


@router.get("/api/patient/searchfullssnrpl")
async def search_full_ssn_rpl(
    fullSsn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    ssn = fullSsn.replace("-", "").upper()
    vq = VistaQuery("ORWPT FULLSSN RPL")
    vq.add_parameter(VistaQuery.LITERAL, ssn)
    return await session.t_query(vq)


@router.get("/api/patient/byward")
async def by_ward(
    wardIen: int = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[PatientSearchResult]:
    vq = VistaQuery("ORWPT BYWARD")
    vq.add_parameter(VistaQuery.LITERAL, str(wardIen))
    results = await session.t_query(vq)
    return PatientSearchResult.parse_list(results)


# ── Admissions & Appointments ───────────────────────────────────────

@router.get("/api/patient/admissions")
async def admissions(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWPT ADMITLST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/patient/visits")
async def visits(
    dfn: str = Query(...),
    _from: str = Query("0", alias="from"),
    thru: str = Query("0"),
    skipAdmits: int = Query(0),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWCV VST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, _from)
    vq.add_parameter(VistaQuery.LITERAL, thru)
    vq.add_parameter(VistaQuery.LITERAL, str(skipAdmits))
    return await session.t_query(vq)


# ── Patient List Defaults ───────────────────────────────────────────

@router.get("/api/patient/defaultlistsource")
async def default_list_source(
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWPT DFLTSRC")
    return await session.s_query(vq)


@router.post("/api/patient/savedefault")
async def save_default(
    listDefault: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("ORWPT SAVDFLT")
    vq.add_parameter(VistaQuery.LITERAL, listDefault)
    return await session.s_query(vq)


@router.get("/api/patient/clinicdateranges")
async def clinic_date_ranges(
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWPT CLINRNG")
    return await session.t_query(vq)


# ── Sensitive Record Access (DG RPCs) ───────────────────────────────

@router.get("/api/patient/sensitiverecordaccess")
async def sensitive_record_access(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> SensitiveRecordResult:
    vq = VistaQuery("DG SENSITIVE RECORD ACCESS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    results = await session.t_query(vq)
    return SensitiveRecordResult.parse(results)


@router.post("/api/patient/logsensitiveaccess")
async def log_sensitive_access(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("DG SENSITIVE RECORD BULLETIN")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.s_query(vq)


@router.get("/api/patient/meanstest")
async def means_test_required(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("DG CHK PAT/DIV MEANS TEST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/patient/similarrecords")
async def similar_records(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("DG CHK BS5 XREF Y/N")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/patient/duplicaterecords")
async def duplicate_records(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("DG CHK BS5 XREF ARRAY")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


# ── Cross-Reference & External Data ─────────────────────────────────

@router.get("/api/patient/icntodfn")
async def icn_to_dfn(
    icn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> str:
    vq = VistaQuery("VAFCTFU CONVERT ICN TO DFN")
    vq.add_parameter(VistaQuery.LITERAL, icn)
    return await session.s_query(vq)


@router.get("/api/patient/vaadata")
async def vaa_data(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORVAA VAA")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/patient/mhvdata")
async def mhv_data(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWMHV MHV")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


# ── Cover Sheet — Appointments & Flags ──────────────────────────────

@router.get("/api/appointment/list")
async def appointment_list(
    dfn: str = Query(...),
    _from: str = Query("0", alias="from"),
    thru: str = Query("0"),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWCV VST")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, _from)
    vq.add_parameter(VistaQuery.LITERAL, thru)
    vq.add_parameter(VistaQuery.LITERAL, "0")
    return await session.t_query(vq)


@router.get("/api/patient/flags")
async def flags(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORPRF HASFLG")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


# ── Expanded Demographics ───────────────────────────────────────────

@router.get("/api/patient/expandeddemographics")
async def expanded_demographics(
    dfn: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("ORWPT PTINQ")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)
