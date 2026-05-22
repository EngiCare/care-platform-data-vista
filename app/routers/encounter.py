# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Encounter router — mirrors EncounterController.cs.

PCE visit types, diagnoses, procedures, immunizations, skin tests, patient education,
health factors, exams, lexicon, modifiers, service-connected eligibility, GAF, MH tests.
"""

from __future__ import annotations

from fastapi import APIRouter, Body, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


# ── Encounter List ──────────────────────────────────────────────────

@router.get("/api/encounter/list")
async def encounter_list(dfn: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE PCE4NOTE")
    vq.add_parameter(VistaQuery.LITERAL, "0")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, "")
    return await session.t_query(vq)


# ── Encounter Form & Visit Types ───────────────────────────────────

@router.get("/api/encounter/visit")
async def visit(dfn: str = Query(...), location: int = Query(...), visitDate: str = Query(...), serviceCategory: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE VISIT")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, visitDate)
    if serviceCategory:
        vq.add_parameter(VistaQuery.LITERAL, serviceCategory)
    return await session.t_query(vq)


@router.get("/api/encounter/autovisittypeselect")
async def auto_visit_type_select(location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE AUTO VISIT TYPE SELECT")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


@router.get("/api/encounter/getsvc")
async def get_svc(location: int = Query(...), provider: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE GETSVC")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, str(provider))
    return await session.s_query(vq)


@router.get("/api/encounter/isclinic")
async def is_clinic(location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE ISCLINIC")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


@router.get("/api/encounter/noncount")
async def non_count(location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE1 NONCOUNT")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


@router.get("/api/encounter/hasvisit")
async def has_visit(noteIen: int = Query(...), dfn: str = Query(...), location: int = Query(...), visitDate: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE HASVISIT")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, visitDate)
    return await session.s_query(vq)


@router.get("/api/encounter/getvisit")
async def get_visit(noteIen: int = Query(...), dfn: str = Query(""), visitStr: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE GET VISIT")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    if dfn:
        vq.add_parameter(VistaQuery.LITERAL, dfn)
        vq.add_parameter(VistaQuery.LITERAL, visitStr)
    return await session.t_query(vq)


@router.get("/api/encounter/cxnoshow")
async def cx_no_show(noteIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE CXNOSHOW")
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.s_query(vq)


@router.get("/api/encounter/anytime")
async def anytime(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE ANYTIME")
    return await session.s_query(vq)


@router.get("/api/encounter/alwayscheckout")
async def always_checkout(location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE ALWAYS CHECKOUT")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


@router.get("/api/encounter/askpce")
async def ask_pce(userDuz: int = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE ASKPCE")
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


@router.get("/api/encounter/force")
async def force_pce(userDuz: int = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE FORCE")
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


# ── Diagnoses ───────────────────────────────────────────────────────

@router.get("/api/encounter/dxtext")
async def dx_text(dxIen: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE GET DX TEXT")
    vq.add_parameter(VistaQuery.LITERAL, dxIen)
    return await session.s_query(vq)


@router.get("/api/encounter/icdver")
async def icd_version(date: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE ICDVER")
    vq.add_parameter(VistaQuery.LITERAL, date)
    return await session.s_query(vq)


@router.get("/api/encounter/activecode")
async def active_code(code: str = Query(...), date: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE ACTIVE CODE")
    vq.add_parameter(VistaQuery.LITERAL, code)
    vq.add_parameter(VistaQuery.LITERAL, date)
    return await session.s_query(vq)


@router.get("/api/encounter/activeproblems")
async def active_problems(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE ACTPROB")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


@router.get("/api/encounter/diag")
async def diag(location: int = Query(...), visitDate: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE DIAG")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, visitDate)
    return await session.t_query(vq)


# ── Lexicon ─────────────────────────────────────────────────────────

@router.get("/api/encounter/lexcode")
async def lex_code(code: str = Query(...), codeSystem: int = Query(...), date: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE LEXCODE")
    vq.add_parameter(VistaQuery.LITERAL, code)
    vq.add_parameter(VistaQuery.LITERAL, str(codeSystem))
    if date:
        vq.add_parameter(VistaQuery.LITERAL, date)
    return await session.s_query(vq)


@router.get("/api/encounter/lex")
async def lex(x: str = Query(...), app: str = Query(""), date: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE LEX")
    vq.add_parameter(VistaQuery.LITERAL, x)
    vq.add_parameter(VistaQuery.LITERAL, app)
    if date:
        vq.add_parameter(VistaQuery.LITERAL, date)
    return await session.t_query(vq)


@router.get("/api/encounter/lexfreq")
async def lex_freq(code: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWLEX GETFREQ")
    vq.add_parameter(VistaQuery.LITERAL, code)
    return await session.s_query(vq)


@router.get("/api/encounter/i10dx")
async def get_i10_dx(code: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWLEX GETI10DX")
    vq.add_parameter(VistaQuery.LITERAL, code)
    return await session.t_query(vq)


# ── Procedures ──────────────────────────────────────────────────────

@router.get("/api/encounter/proc")
async def proc(location: int = Query(...), visitDate: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE PROC")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, visitDate)
    return await session.t_query(vq)


@router.get("/api/encounter/cptmods")
async def cpt_modifiers(cptCode: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE CPTMODS")
    vq.add_parameter(VistaQuery.LITERAL, cptCode)
    return await session.t_query(vq)


@router.get("/api/encounter/getmod")
async def get_modifiers(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE GETMOD")
    return await session.t_query(vq)


@router.post("/api/encounter/hascpt")
async def has_cpt(items: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE HASCPT")
    dhl = DictionaryHashList()
    for i, item in enumerate(items):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


# ── Immunizations ───────────────────────────────────────────────────

@router.get("/api/encounter/imm")
async def imm(location: int = Query(...), visitDate: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE IMM")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, visitDate)
    return await session.t_query(vq)


@router.get("/api/encounter/immtypes")
async def imm_types(visitDate: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE GET IMMUNIZATION TYPE")
    vq.add_parameter(VistaQuery.LITERAL, visitDate)
    return await session.t_query(vq)


# ── Skin Tests ──────────────────────────────────────────────────────

@router.get("/api/encounter/sk")
async def skin_tests(location: int = Query(...), visitDate: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE SK")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, visitDate)
    return await session.t_query(vq)


@router.get("/api/encounter/sktypes")
async def skin_test_types(visitDate: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE GET SKIN TEST TYPE")
    vq.add_parameter(VistaQuery.LITERAL, visitDate)
    return await session.t_query(vq)


# ── Patient Education ──────────────────────────────────────────────

@router.get("/api/encounter/ped")
async def patient_education(location: int = Query(...), visitDate: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE PED")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, visitDate)
    return await session.t_query(vq)


@router.get("/api/encounter/edtopics")
async def education_topics(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE GET EDUCATION TOPICS")
    return await session.t_query(vq)


# ── Health Factors ──────────────────────────────────────────────────

@router.get("/api/encounter/hf")
async def health_factors(location: int = Query(...), visitDate: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE HF")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, visitDate)
    return await session.t_query(vq)


@router.get("/api/encounter/hftypes")
async def health_factor_types(idx: int = Query(1), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE GET HEALTH FACTORS TY")
    vq.add_parameter(VistaQuery.LITERAL, str(idx))
    return await session.t_query(vq)


# ── Exams ───────────────────────────────────────────────────────────

@router.get("/api/encounter/xam")
async def exams(location: int = Query(...), visitDate: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE XAM")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, visitDate)
    return await session.t_query(vq)


@router.get("/api/encounter/examtypes")
async def exam_types(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE GET EXAM TYPE")
    return await session.t_query(vq)


# ── Set-of-Codes ────────────────────────────────────────────────────

@router.get("/api/encounter/setofcodes")
async def set_of_codes(fileNumber: str = Query(...), fieldNumber: str = Query(...), ien: str = Query("1"), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE GET SET OF CODES")
    vq.add_parameter(VistaQuery.LITERAL, fileNumber)
    vq.add_parameter(VistaQuery.LITERAL, fieldNumber)
    vq.add_parameter(VistaQuery.LITERAL, ien)
    return await session.t_query(vq)


@router.get("/api/encounter/excluded")
async def get_excluded(location: int = Query(...), pceType: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE GET EXCLUDED")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, str(pceType))
    return await session.t_query(vq)


@router.get("/api/encounter/histlocations")
async def hist_locations(session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORQQPX GET HIST LOCATIONS")
    return await session.t_query(vq)


# ── Service-Connected / Eligibility ────────────────────────────────

@router.get("/api/encounter/scsel")
async def sc_sel(dfn: str = Query(...), encounterDate: str = Query(""), location: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE SCSEL")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, encounterDate)
    vq.add_parameter(VistaQuery.LITERAL, location)
    return await session.s_query(vq)


@router.get("/api/encounter/scdis")
async def sc_disabilities(dfn: str = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE SCDIS")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    return await session.t_query(vq)


# ── PCE Data Load / Save / Delete ──────────────────────────────────

@router.get("/api/encounter/pce4note")
async def pce4note(noteIen: int = Query(...), dfn: str = Query(""), visitStr: str = Query(""), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE PCE4NOTE")
    if noteIen < 1:
        vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
        vq.add_parameter(VistaQuery.LITERAL, dfn)
        vq.add_parameter(VistaQuery.LITERAL, visitStr)
    else:
        vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.t_query(vq)


@router.post("/api/encounter/save")
async def save(pceList: list[str] = Body(...), noteIen: int = Query(...), location: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE SAVE")
    dhl = DictionaryHashList()
    for i, item in enumerate(pceList):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.t_query(vq)


@router.post("/api/encounter/delete")
async def delete_encounter(pceList: list[str] = Body(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE DELETE")
    dhl = DictionaryHashList()
    for i, item in enumerate(pceList):
        dhl.add(str(i + 1), item)
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.s_query(vq)


@router.get("/api/encounter/current")
async def current_encounter(noteIen: str = Query("0"), dfn: str = Query(""), visitStr: str = Query(""), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE PCE4NOTE")
    vq.add_parameter(VistaQuery.LITERAL, noteIen)
    try:
        ien = int(noteIen)
    except ValueError:
        ien = 0
    if ien < 1:
        vq.add_parameter(VistaQuery.LITERAL, dfn)
        vq.add_parameter(VistaQuery.LITERAL, visitStr)
    return await session.s_query(vq)


# ── Provider / User ─────────────────────────────────────────────────

@router.get("/api/encounter/activeprov")
async def active_provider(provider: str = Query(...), dateTime: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE ACTIVE PROV")
    vq.add_parameter(VistaQuery.LITERAL, provider)
    vq.add_parameter(VistaQuery.LITERAL, dateTime)
    return await session.s_query(vq)


@router.get("/api/encounter/defaultprovider")
async def default_provider(location: int = Query(...), userDuz: int = Query(...), date: str = Query(...), noteIen: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU GET DEFAULT PROVIDER")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    vq.add_parameter(VistaQuery.LITERAL, date)
    vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.s_query(vq)


@router.get("/api/encounter/isuseranprovider")
async def is_user_a_provider(userDuz: int = Query(...), date: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU IS USER A PROVIDER?")
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    vq.add_parameter(VistaQuery.LITERAL, date)
    return await session.s_query(vq)


@router.get("/api/encounter/isusrprovider")
async def is_usr_provider(userDuz: int = Query(...), date: str = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU IS USER A USR PROVIDER")
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    vq.add_parameter(VistaQuery.LITERAL, date)
    return await session.s_query(vq)


# ── Document Parameters ────────────────────────────────────────────

@router.get("/api/encounter/docparams")
async def doc_params(noteIen: int = Query(...), titleIen: int = Query(0), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("TIU GET DOCUMENT PARAMETERS")
    if noteIen <= 0:
        vq.add_parameter(VistaQuery.LITERAL, "0")
        vq.add_parameter(VistaQuery.LITERAL, str(titleIen))
    else:
        vq.add_parameter(VistaQuery.LITERAL, str(noteIen))
    return await session.s_query(vq)


# ── GAF ─────────────────────────────────────────────────────────────

@router.get("/api/encounter/gafok")
async def gaf_ok(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE GAFOK")
    return await session.s_query(vq)


@router.get("/api/encounter/mhclinic")
async def mh_clinic(location: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE MHCLINIC")
    vq.add_parameter(VistaQuery.LITERAL, str(location))
    return await session.s_query(vq)


@router.get("/api/encounter/loadgaf")
async def load_gaf(dfn: str = Query(...), limit: int = Query(10), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE LOADGAF")
    dhl = DictionaryHashList()
    dhl.add('"DFN"', dfn)
    dhl.add('"LIMIT"', str(limit))
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.post("/api/encounter/savegaf")
async def save_gaf(dfn: str = Query(...), score: int = Query(...), gafDate: str = Query(...), staff: int = Query(...), session: ISession = Depends(get_current_session)) -> list[str]:
    vq = VistaQuery("ORWPCE SAVEGAF")
    dhl = DictionaryHashList()
    dhl.add('"DFN"', dfn)
    dhl.add('"GAF"', str(score))
    dhl.add('"DATE"', gafDate)
    dhl.add('"STAFF"', str(staff))
    vq.add_parameter(VistaQuery.LIST, dhl)
    return await session.t_query(vq)


@router.get("/api/encounter/gafurl")
async def gaf_url(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE GAFURL")
    return await session.s_query(vq)


# ── Mental Health Tests ─────────────────────────────────────────────

@router.get("/api/encounter/mhtestok")
async def mh_test_ok(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE MHTESTOK")
    return await session.s_query(vq)


@router.get("/api/encounter/mhtestauth")
async def mh_test_auth(test: str = Query(...), userDuz: int = Query(...), session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWPCE MH TEST AUTHORIZED")
    vq.add_parameter(VistaQuery.LITERAL, test)
    vq.add_parameter(VistaQuery.LITERAL, str(userDuz))
    return await session.s_query(vq)
