# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Note models — mirrors NoteModels.cs (CreateNoteResult only; TiuDocument already in tiu_document.py)."""

from __future__ import annotations

from pydantic import BaseModel

from app.models.vista_string_parser import parse_int, split


class CreateNoteResult(BaseModel):
    success: bool = False
    note_ien: str = ""
    error_text: str = ""

    @classmethod
    def parse(cls, vista_string: str) -> "CreateNoteResult":
        p = split(vista_string)
        ien = p[0] if len(p) > 0 else ""
        error = p[1] if len(p) > 1 else ""
        ien_val = parse_int(ien)
        return cls(
            note_ien=ien,
            success=ien_val > 0 and (not error or error == "0" or error.startswith("Record created")),
            error_text="" if (error == "0" or error.startswith("Record created")) else error,
        )
