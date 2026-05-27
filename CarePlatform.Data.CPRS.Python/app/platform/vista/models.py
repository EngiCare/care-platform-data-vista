# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""VistA data types — mirrors DataTypes.cs, VistaCredentials.cs, SecurityKey.cs, VistaMenuOption.cs."""

from __future__ import annotations

from dataclasses import dataclass, field
from enum import Enum
from typing import Optional


class PermissionType(Enum):
    MENU_OPTION = "MenuOption"
    SECURITY_KEY = "SecurityKey"
    DELEGATED_OPTION = "DelegatedOption"
    OTHER = "Other"


@dataclass
class AbstractPermission:
    permission_id: str = ""
    name: str = ""
    record_number: str = ""
    is_primary: bool = False

    @property
    def ptype(self) -> PermissionType:
        raise NotImplementedError


@dataclass
class PersonName:
    lastname: str = ""
    firstname: str = ""
    full_name: str = ""

    def __init__(self, full_name: str = ""):
        self.full_name = full_name or ""
        if not full_name:
            self.lastname = ""
            self.firstname = ""
            return
        parts = full_name.split(",")
        self.lastname = parts[0].strip()
        self.firstname = parts[1].strip().split(" ")[0] if len(parts) > 1 else ""

    def __str__(self) -> str:
        return self.full_name or f"{self.lastname},{self.firstname}"


@dataclass
class SocSecNum:
    value: str = ""

    def __str__(self) -> str:
        return self.value or ""


@dataclass
class SiteId:
    id: str = ""
    name: str = ""

    def __str__(self) -> str:
        return self.id or ""


@dataclass
class Service:
    name: str = ""
    id: str = ""

    def __str__(self) -> str:
        return self.name or ""


@dataclass
class DataSource:
    provider: str = ""
    port: int = 0
    protocol: str = ""
    site_id: Optional[SiteId] = None


@dataclass
class User:
    uid: str = ""
    name: Optional[PersonName] = None
    ssn: Optional[SocSecNum] = None
    logon_site_id: Optional[SiteId] = None
    title: str = ""
    service: Optional[Service] = None
    greeting: str = ""
    division_lines: list[str] = field(default_factory=list)


@dataclass
class VistaCredentials:
    account_name: str = ""
    account_password: str = ""
    sso_token: str = ""
    authentication_source: Optional[DataSource] = None
    federated_uid: str = ""
    local_uid: str = ""
    subject_name: str = ""
    subject_phone: str = ""
    authentication_token: str = ""
    security_phrase: str = ""


@dataclass
class SecurityKey:
    key_id: str = ""
    name: str = ""
    record_id: str = ""

    @property
    def ptype(self) -> PermissionType:
        return PermissionType.SECURITY_KEY


@dataclass
class VistaOption(AbstractPermission):
    display_name: str = ""
    key: Optional[SecurityKey] = None
    reverse_key: Optional[SecurityKey] = None
    option_type: str = ""

    @property
    def ptype(self) -> PermissionType:
        return PermissionType.MENU_OPTION
