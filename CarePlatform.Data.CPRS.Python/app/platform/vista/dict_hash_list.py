# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Ordered key/value list for VistA RPC LIST parameters.

Mirrors DictionaryHashList.cs.  Also contains VistaTimestamp helper.
"""

from __future__ import annotations

from datetime import datetime, timezone


class DictionaryHashList:
    """Ordered list of (key, value) pairs used to build LIST-type RPC params."""

    def __init__(self) -> None:
        self._items: list[tuple[str, str]] = []
        self._index: dict[str, int] = {}

    @property
    def count(self) -> int:
        return len(self._items)

    def add(self, key: str, value: str) -> None:
        if key in self._index:
            idx = self._index[key]
            self._items[idx] = (key, value)
        else:
            self._index[key] = len(self._items)
            self._items.append((key, value))

    def __getitem__(self, index: int) -> tuple[str, str]:
        return self._items[index]

    def __len__(self) -> int:
        return len(self._items)

    def __iter__(self):
        return iter(self._items)


class VistaTimestamp:
    """Convert a FileMan date string to UTC ISO string.

    FileMan dates are YYYMMDD.HHMMSS where YYY = year - 1700.
    """

    @staticmethod
    def to_utc_string(fm_date: str) -> str:
        if not fm_date or fm_date.strip() == "":
            return ""
        fm_date = fm_date.strip()
        parts = fm_date.split(".")
        date_part = parts[0]
        time_part = parts[1] if len(parts) > 1 else ""

        # pad date_part to 7 digits
        date_part = date_part.ljust(7, "0")
        yyy = int(date_part[0:3])
        year = yyy + 1700
        month = int(date_part[3:5])
        day = int(date_part[5:7])

        hour = minute = second = 0
        if time_part:
            time_part = time_part.ljust(6, "0")
            hour = int(time_part[0:2])
            minute = int(time_part[2:4])
            second = int(time_part[4:6])

        dt = datetime(year, month, day, hour, minute, second, tzinfo=timezone.utc)
        return dt.isoformat()
