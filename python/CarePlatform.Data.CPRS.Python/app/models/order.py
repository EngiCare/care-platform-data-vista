# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Order models — mirrors OrderModels.cs.

Order with parse/parse_list from ORWORR GET caret-delimited lines.
"""

from __future__ import annotations

from pydantic import BaseModel
from typing import Optional

from app.models.vista_string_parser import parse_fm_datetime, parse_int


class Order(BaseModel):
    order_id: str = ""
    display_group: str = ""
    text: str = ""
    status: str = ""
    status_name: str = ""
    start_date: Optional[str] = None
    stop_date: Optional[str] = None
    provider: str = ""
    is_flagged: bool = False

    @staticmethod
    def parse(line: str) -> Order:
        p = line.split("^")

        def _g(idx: int) -> str:
            return p[idx] if idx < len(p) else ""

        return Order(
            order_id=_g(0),
            display_group=_g(1),
            status=_g(2),
            status_name=_g(3),
            text=_g(4),
            start_date=_g(5) or None,
            stop_date=_g(6) or None,
            provider=_g(7),
            is_flagged=_g(8) == "1",
        )

    @staticmethod
    def parse_list(lines: list[str]) -> list[Order]:
        return [Order.parse(line) for line in lines if line.strip()]
