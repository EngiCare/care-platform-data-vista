# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Session-related Pydantic models — mirrors SessionModels.cs."""

from __future__ import annotations

from pydantic import BaseModel
from typing import Optional


class LoginRequest(BaseModel):
    site_id: str = ""
    access_code: str = ""
    verify_code: str = ""
    sso_token: Optional[str] = None
    host_name: Optional[str] = None
    port: Optional[int] = None


class LoginResponse(BaseModel):
    success: bool = False
    token: Optional[str] = None
    error_message: Optional[str] = None
    user_name: Optional[str] = None
    duz: int = 0
    site_id: Optional[str] = None
    station_number: Optional[str] = None


class UserDivision(BaseModel):
    ien: str = ""
    name: str = ""
    station_number: str = ""
    is_default: bool = False

    @classmethod
    def parse_line(cls, line: str) -> "UserDivision":
        pieces = line.split("^")
        return cls(
            ien=pieces[0] if len(pieces) > 0 else "",
            name=pieces[1] if len(pieces) > 1 else "",
            station_number=pieces[2] if len(pieces) > 2 else "",
            is_default=pieces[3] == "1" if len(pieces) > 3 else False,
        )

    @classmethod
    def parse_list(cls, lines: list[str]) -> list["UserDivision"]:
        if len(lines) <= 1:
            return []
        return [cls.parse_line(line) for line in lines[1:]]


class SessionStatus(BaseModel):
    seconds_remaining: int = 0
    is_active: bool = True
    current_time: Optional[str] = None


class SiteEntry(BaseModel):
    site_id: str = ""
    name: str = ""
    visn_name: str = ""


class SsoSiteOption(BaseModel):
    site_id: str = ""
    duz: str = ""
    name: str = ""
