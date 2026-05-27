# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""FileMan date helpers — mirrors FileManDate.cs."""

from __future__ import annotations

import re
from datetime import datetime, timedelta
from typing import Optional


_FM_SHAPE = re.compile(r"^\d{5,7}(\.\d+)?$")
_T_RELATIVE = re.compile(
    r"^T(?P<sign>[+\-])?(?P<days>\d+)?(@(?P<time>\d{1,6}))?$", re.IGNORECASE
)


def to_fm(input_str: Optional[str]) -> str:
    """Convert a user-supplied date string to a FileMan date string."""
    if not input_str or not input_str.strip():
        return ""
    s = input_str.strip()

    if _FM_SHAPE.match(s):
        return s  # pass-through

    t = _T_RELATIVE.match(s)
    if t:
        sign = -1 if t.group("sign") == "-" else 1
        days = int(t.group("days")) if t.group("days") else 0
        dt = datetime.today() + timedelta(days=sign * days)
        if t.group("time"):
            hms = int(t.group("time"))
            hh = (hms // 10000) % 24
            mm = (hms // 100) % 100
            ss = hms % 100
            dt = dt.replace(hour=hh, minute=mm, second=ss, microsecond=0)
        return format_fm(dt)

    # Try standard date parse
    for fmt in ("%Y-%m-%d", "%Y-%m-%dT%H:%M:%S", "%Y-%m-%dT%H:%M", "%m/%d/%Y"):
        try:
            parsed = datetime.strptime(s, fmt)
            return format_fm(parsed)
        except ValueError:
            continue

    return ""


def format_fm(dt: datetime) -> str:
    """Format a datetime as a FileMan date string."""
    yyy = dt.year - 1700
    date_part = f"{yyy:03d}{dt.month:02d}{dt.day:02d}"
    if dt.hour == 0 and dt.minute == 0 and dt.second == 0:
        return date_part
    time_part = f"{dt.hour:02d}{dt.minute:02d}{dt.second:02d}".rstrip("0")
    return f"{date_part}.{time_part}" if time_part else date_part


def to_fm_range(alpha: Optional[str], omega: Optional[str]) -> tuple[str, str]:
    """Convert both ends of a date range, swap if alpha > omega."""
    a = to_fm(alpha)
    o = to_fm(omega)
    if a and o:
        try:
            if float(a) > float(o):
                return o, a
        except ValueError:
            pass
    return a, o
