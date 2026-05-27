# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""TIU document model — mirrors TiuDocument.cs.

Parsed from TIU DOCUMENTS BY CONTEXT RPC (15 caret-delimited pieces).
Used by Notes (class=3), DCSumm (class=244), and Consults TIU sub-documents.
"""

from __future__ import annotations

from datetime import datetime
from typing import Optional

from pydantic import BaseModel

from app.models.vista_string_parser import parse_fm_datetime, parse_int, split


class TiuDocument(BaseModel):
    ien: str = ""
    title: str = ""
    fm_date: str = ""
    reference_date: Optional[datetime] = None
    patient_name: str = ""
    author_raw: str = ""
    author_name: str = ""
    author_duz: int = 0
    location_name: str = ""
    status: str = ""
    visit_string: str = ""
    discharge_date_string: str = ""
    discharge_date: Optional[datetime] = None
    package_ref: str = ""
    image_count: int = 0
    subject: str = ""
    has_children_flag: str = ""
    has_children: bool = False
    parent_document: str = ""
    order_by_title: bool = False

    @classmethod
    def parse(cls, vista_line: str) -> "TiuDocument":
        """Parse a single caret-delimited line from TIU DOCUMENTS BY CONTEXT."""
        p = split(vista_line)
        author_raw = p[4] if len(p) > 4 else ""
        author_parts = author_raw.split(";")

        has_children_raw = p[12] if len(p) > 12 else ""
        if has_children_raw.startswith("*"):
            has_children_raw = has_children_raw[1:]

        discharge_date_str = p[8] if len(p) > 8 else ""
        discharge_fm = (
            discharge_date_str.split(";")[1]
            if ";" in discharge_date_str
            else discharge_date_str
        )

        author_duz = 0
        if author_parts and author_parts[0]:
            try:
                author_duz = int(author_parts[0])
            except ValueError:
                pass

        if len(author_parts) > 2:
            author_name = author_parts[2]
        elif len(author_parts) > 1:
            author_name = author_parts[1]
        else:
            author_name = author_raw

        return cls(
            ien=p[0] if len(p) > 0 else "",
            title=p[1] if len(p) > 1 else "",
            fm_date=p[2] if len(p) > 2 else "",
            reference_date=parse_fm_datetime(p[2]) if len(p) > 2 else None,
            patient_name=p[3] if len(p) > 3 else "",
            author_raw=author_raw,
            author_duz=author_duz,
            author_name=author_name,
            location_name=p[5] if len(p) > 5 else "",
            status=p[6] if len(p) > 6 else "",
            visit_string=p[7] if len(p) > 7 else "",
            discharge_date_string=discharge_date_str,
            discharge_date=parse_fm_datetime(discharge_fm),
            package_ref=p[9] if len(p) > 9 else "",
            image_count=parse_int(p[10]) if len(p) > 10 else 0,
            subject=p[11] if len(p) > 11 else "",
            has_children_flag=has_children_raw,
            has_children=bool(has_children_raw),
            parent_document=p[13] if len(p) > 13 else "",
            order_by_title=(p[14] == "1") if len(p) > 14 else False,
        )

    @classmethod
    def parse_list(cls, vista_lines: list[str]) -> list["TiuDocument"]:
        """Parse a list of lines, skipping blanks and IEN-less entries."""
        results = []
        for line in vista_lines:
            if not line or not line.strip():
                continue
            doc = cls.parse(line)
            if doc.ien:
                results.append(doc)
        return results
