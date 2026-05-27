# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Consult models — mirrors ConsultModels.cs (Consult only for list parsing)."""

from __future__ import annotations

from pydantic import BaseModel

from app.models.vista_string_parser import parse_fm_datetime, split


# Status abbreviation mapping — parity with cprs/Consults/uConsults.pas
_STATUS_TO_ABBREV: dict[str, str] = {
    "PENDING": "p", "ACTIVE": "a", "SCHEDULED": "sch", "COMPLETE": "c",
    "CANCELLED": "x", "DISCONTINUED": "dc", "HOLD": "h", "FLAGGED": "f",
    "EXPIRED": "e", "PARTIAL RESULTS": "pr", "DELAYED": "d",
    "UNRELEASED": "u", "CHANGED": "ch", "LAPSED": "l", "RENEWED": "rn",
}
_ABBREV_TO_STATUS: dict[str, str] = {v: k for k, v in _STATUS_TO_ABBREV.items()}


class Consult(BaseModel):
    ien: str = ""
    status: str = ""
    service: str = ""
    procedure: str = ""
    consult_type: str = ""
    request_date: str | None = None
    urgency: str = ""
    requesting_provider: str = ""
    display_date: str = ""
    status_abbrev: str = ""
    consult_number: str = ""
    order_ifn: str = ""
    has_children: str = ""
    parent_node: str = ""
    type_text: str = ""
    type_code: str = ""
    to_service: str = ""

    @classmethod
    def parse(cls, line: str) -> "Consult":
        p = line.split("^")

        def pc(idx: int) -> str:
            return p[idx] if 0 <= idx < len(p) else ""

        # Heuristic: real VistA lines have a numeric FM date in piece 11 (index 10)
        p11 = pc(10)
        is_wire = len(p) >= 11 and p11.strip() != ""
        try:
            float(p11)
        except (ValueError, TypeError):
            is_wire = False

        if is_wire:
            status_abbrev = pc(2).strip()
            type_text = pc(8)
            return cls(
                ien=pc(0),
                display_date=pc(1).strip(),
                status_abbrev=status_abbrev,
                status=_ABBREV_TO_STATUS.get(status_abbrev, ""),
                service=pc(3),
                consult_number=pc(4),
                order_ifn=pc(5),
                has_children=pc(6),
                parent_node=pc(7),
                type_text=type_text,
                consult_type=type_text,
                procedure=pc(3) if type_text.lower() in ("procedure", "clinical procedure") else "",
                type_code=pc(11),
                to_service=pc(9),
                request_date=str(parse_fm_datetime(p11)) if parse_fm_datetime(p11) else None,
            )

        # Legacy 7-piece stub fallback
        status = pc(3)
        return cls(
            ien=pc(0),
            service=pc(1),
            procedure=pc(2),
            status=status,
            status_abbrev=_STATUS_TO_ABBREV.get(status.upper(), status.lower()),
            request_date=str(parse_fm_datetime(pc(4))) if parse_fm_datetime(pc(4)) else None,
            urgency=pc(5),
            requesting_provider=pc(6),
        )

    @classmethod
    def parse_list(cls, lines: list[str]) -> list["Consult"]:
        return [cls.parse(line) for line in lines if line.strip()]
