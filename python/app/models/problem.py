# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Problem models — mirrors ProblemModels.cs."""

from __future__ import annotations

from pydantic import BaseModel

from app.models.vista_string_parser import piece, split, parse_fm_datetime


class Problem(BaseModel):
    ifn: str = ""
    dfn: str = ""
    description: str = ""
    icd_code: str = ""
    snomed_code: str = ""
    status: str = ""
    priority: str = ""
    onset_date: str | None = None
    date_recorded: str | None = None
    date_resolved: str | None = None
    date_modified: str | None = None
    responsible_provider: str = ""
    responsible_provider_ien: int = 0
    recording_provider: str = ""
    location: str = ""
    service: str = ""
    condition: str = ""
    inactive_icd_flag: bool = False
    service_connected: bool = False
    icd_code_system: str = ""

    @staticmethod
    def _extract_name(ien_name_pair: str) -> str:
        if not ien_name_pair:
            return ""
        idx = ien_name_pair.find(";")
        if idx >= 0 and idx < len(ien_name_pair) - 1:
            return ien_name_pair[idx + 1:]
        return ien_name_pair

    @staticmethod
    def parse(vista_line: str) -> Problem:
        p = split(vista_line)
        return Problem(
            ifn=p[0] if len(p) > 0 else "",
            status=p[1] if len(p) > 1 else "",
            description=p[2] if len(p) > 2 else "",
            icd_code=p[3] if len(p) > 3 else "",
            onset_date=p[4] if len(p) > 4 and p[4] else None,
            date_modified=p[5] if len(p) > 5 and p[5] else None,
            service_connected=len(p) > 6 and p[6] != "0" and bool(p[6]),
            condition=p[8] if len(p) > 8 else "",
            location=Problem._extract_name(p[9]) if len(p) > 9 else "",
            responsible_provider=Problem._extract_name(p[11]) if len(p) > 11 else "",
            service=Problem._extract_name(p[12]) if len(p) > 12 else "",
            priority=p[13] if len(p) > 13 else "",
            inactive_icd_flag=len(p) > 17 and p[17] == "#",
            icd_code_system=p[19] if len(p) > 19 else "",
        )

    @staticmethod
    def parse_list(vista_lines: list[str]) -> list[Problem]:
        return [
            Problem.parse(line)
            for line in vista_lines
            if line.strip() and Problem.parse(line).ifn
        ]


class ProblemLexiconResult(BaseModel):
    ien: str = ""
    text: str = ""
    icd_code: str = ""
    snomed_ct: str = ""

    @staticmethod
    def parse(vista_line: str) -> ProblemLexiconResult:
        p = split(vista_line)
        return ProblemLexiconResult(
            ien=p[0] if len(p) > 0 else "",
            text=p[1] if len(p) > 1 else "",
            icd_code=p[2] if len(p) > 2 else "",
            snomed_ct=p[3] if len(p) > 3 else "",
        )

    @staticmethod
    def parse_list(vista_lines: list[str]) -> list[ProblemLexiconResult]:
        return [
            ProblemLexiconResult.parse(line)
            for line in vista_lines
            if line.strip()
        ]
