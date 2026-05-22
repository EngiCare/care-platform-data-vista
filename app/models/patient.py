# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Patient models — mirrors PatientModels.cs.

Parsed from VistA RPC responses for patient demographics, search, and sensitivity.
"""

from __future__ import annotations

from datetime import datetime
from typing import Optional

from pydantic import BaseModel

from app.models.vista_string_parser import parse_bool, parse_fm_datetime, parse_int, split


class PatientDemographics(BaseModel):
    dfn: str = ""
    name: str = ""
    sex: str = ""
    date_of_birth: Optional[datetime] = None
    ssn: str = ""
    location_ien: str = ""
    location_name: str = ""
    room_bed: str = ""
    cwad: str = ""
    is_sensitive: bool = False
    admit_time: Optional[datetime] = None
    icn: str = ""
    age: int = 0
    treating_specialty: str = ""
    service_connected: str = ""
    service_connected_percent: int = 0

    @classmethod
    def parse(cls, dfn: str, vista_string: str) -> "PatientDemographics":
        p = split(vista_string)
        return cls(
            dfn=dfn,
            name=p[0] if len(p) > 0 else "",
            sex=p[1] if len(p) > 1 else "",
            date_of_birth=parse_fm_datetime(p[2]) if len(p) > 2 else None,
            ssn=p[3] if len(p) > 3 else "",
            location_ien=p[4] if len(p) > 4 else "",
            location_name=p[5] if len(p) > 5 else "",
            room_bed=p[6] if len(p) > 6 else "",
            cwad=p[7] if len(p) > 7 else "",
            is_sensitive=parse_bool(p[8]) if len(p) > 8 else False,
            admit_time=parse_fm_datetime(p[9]) if len(p) > 9 else None,
            icn=p[13] if len(p) > 13 else "",
            age=parse_int(p[14]) if len(p) > 14 else 0,
            treating_specialty=p[15] if len(p) > 15 else "",
            service_connected=p[11] if len(p) > 11 else "",
            service_connected_percent=parse_int(p[12]) if len(p) > 12 else 0,
        )


class PatientPrimaryCare(BaseModel):
    primary_team: str = ""
    primary_provider: str = ""
    attending: str = ""
    associate: str = ""
    mental_health_treatment_coordinator: str = ""
    inpatient_provider: str = ""

    @classmethod
    def parse(cls, vista_string: str) -> "PatientPrimaryCare":
        p = split(vista_string)
        return cls(
            primary_team=p[0] if len(p) > 0 else "",
            primary_provider=p[1] if len(p) > 1 else "",
            attending=p[2] if len(p) > 2 else "",
            associate=p[3] if len(p) > 3 else "",
            mental_health_treatment_coordinator=p[4] if len(p) > 4 else "",
            inpatient_provider=p[5] if len(p) > 5 else "",
        )


class PatientIdInfo(BaseModel):
    ssn: str = ""
    date_of_birth: Optional[datetime] = None
    sex: str = ""
    is_veteran: bool = False
    service_connected_percent: int = 0
    ward: str = ""
    room_bed: str = ""
    name: str = ""

    @classmethod
    def parse(cls, vista_string: str) -> "PatientIdInfo":
        p = split(vista_string)
        return cls(
            ssn=p[0] if len(p) > 0 else "",
            date_of_birth=parse_fm_datetime(p[1]) if len(p) > 1 else None,
            sex=p[2] if len(p) > 2 else "",
            is_veteran=parse_bool(p[3]) if len(p) > 3 else False,
            service_connected_percent=parse_int(p[4]) if len(p) > 4 else 0,
            ward=p[5] if len(p) > 5 else "",
            room_bed=p[6] if len(p) > 6 else "",
            name=p[7] if len(p) > 7 else "",
        )


class PatientSearchResult(BaseModel):
    dfn: str = ""
    name: str = ""
    ssn: str = ""
    date_of_birth: Optional[datetime] = None
    room_bed: str = ""
    appointment_fm_date_time: str = ""

    @classmethod
    def parse(cls, vista_line: str) -> "PatientSearchResult":
        p = split(vista_line)
        return cls(
            dfn=p[0] if len(p) > 0 else "",
            name=p[1] if len(p) > 1 else "",
            ssn=p[3] if len(p) > 3 else "",
            date_of_birth=parse_fm_datetime(p[2]) if len(p) > 2 else None,
            room_bed=p[4] if len(p) > 4 else "",
        )

    @classmethod
    def parse_list(cls, vista_lines: list[str]) -> list["PatientSearchResult"]:
        results = []
        for line in vista_lines:
            if not line or not line.strip():
                continue
            r = cls.parse(line)
            if r.dfn:
                results.append(r)
        return results

    @classmethod
    def parse_clinic(cls, vista_line: str) -> "PatientSearchResult":
        p = split(vista_line)
        return cls(
            dfn=p[0] if len(p) > 0 else "",
            name=p[1] if len(p) > 1 else "",
            room_bed=p[2] if len(p) > 2 else "",
            appointment_fm_date_time=p[3] if len(p) > 3 else "",
        )

    @classmethod
    def parse_clinic_list(cls, vista_lines: list[str]) -> list["PatientSearchResult"]:
        results = []
        for line in vista_lines:
            if not line or not line.strip():
                continue
            r = cls.parse_clinic(line)
            if r.dfn:
                results.append(r)
        return results


class SensitiveRecordResult(BaseModel):
    status: int = 0
    message: str = ""
    requires_warning: bool = False
    access_denied: bool = False
    has_error: bool = False

    @classmethod
    def parse(cls, vista_lines: list[str]) -> "SensitiveRecordResult":
        status = parse_int(vista_lines[0]) if vista_lines else 0
        message = "\n".join(vista_lines[1:]) if len(vista_lines) > 1 else ""
        return cls(
            status=status,
            message=message,
            requires_warning=status in (1, 2),
            access_denied=status == 3,
            has_error=status == -1,
        )
