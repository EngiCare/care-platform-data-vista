# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""VistA string parser — mirrors VistaStringParser.cs.

Caret/pipe-delimited string parsing utilities matching CPRS Piece() patterns.
"""

from __future__ import annotations

from datetime import datetime
from typing import Optional


def piece(source: str, delimiter: str, piece_num: int) -> str:
    """Extract a single piece from a delimited string (1-based index)."""
    if not source or piece_num < 1:
        return ""
    parts = source.split(delimiter)
    return parts[piece_num - 1] if piece_num <= len(parts) else ""


def split(source: str, delimiter: str = "^") -> list[str]:
    """Split a VistA delimited string into parts."""
    return source.split(delimiter) if source else []


def parse_fm_datetime(fm_date: Optional[str]) -> Optional[datetime]:
    """Parse a VistA FM date/time string to datetime.

    FM format: YYYMMDD.HHMMSS where YYY = year - 1700.
    """
    if not fm_date or not fm_date.strip():
        return None
    try:
        clean = fm_date.strip().split(" ")[0]
        dot_parts = clean.split(".")
        date_part = dot_parts[0]

        if len(date_part) < 7:
            return None

        year = 1700 + int(date_part[:3])
        month = int(date_part[3:5])
        day = int(date_part[5:7])

        hour = minute = second = 0
        if len(dot_parts) > 1 and len(dot_parts[1]) >= 2:
            time_part = dot_parts[1].ljust(6, "0")
            hour = int(time_part[0:2])
            minute = int(time_part[2:4])
            second = int(time_part[4:6])

        return datetime(year, month, day, hour, minute, second)
    except Exception:
        return None


def parse_bool(value: Optional[str]) -> bool:
    """Parse a boolean-like VistA value ('1', 'Y', 'YES', 'TRUE' → True)."""
    if not value or not value.strip():
        return False
    v = value.strip().upper()
    return v in ("1", "Y", "YES", "TRUE")


def parse_int(value: Optional[str], default: int = 0) -> int:
    """Safely parse an integer from a VistA string piece."""
    if not value or not value.strip():
        return default
    try:
        return int(value.strip())
    except ValueError:
        return default
