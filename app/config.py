# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Application configuration — maps appsettings.json to pydantic-settings."""

from __future__ import annotations

import json
import logging
from pathlib import Path
from typing import Any

from pydantic_settings import BaseSettings
from pydantic import Field

logger = logging.getLogger(__name__)


class TokenServiceSettings(BaseSettings):
    certificate_name: str = "CN=CarePlatform"
    certificate_file_name: str = ""
    certificate_password: str = ""
    issuer_name: str = "CarePlatform"
    audience: str = "urn:careplatform"
    sts_base_address: str = "https://localhost:5001/"


class JwtValidationSettings(BaseSettings):
    validate_issuer: bool = True
    validate_audience: bool = True
    validate_issuer_signing_key: bool = False
    validate_lifetime: bool = False


class SessionManagerSettings(BaseSettings):
    session_manager_object: str = ""
    session_object: str = ""


class DemoSettings(BaseSettings):
    test_files_path: str = "resources/demo-data"
    user_name: str = "DEMO,USER"
    duz: str = "12345"
    site_id: str = "500"


class AppSettings(BaseSettings):
    site_config_file_path: str = "resources/xml/LocalVEHUSites.xml"
    token_service: TokenServiceSettings = Field(default_factory=TokenServiceSettings)
    jwt_validation: JwtValidationSettings = Field(default_factory=JwtValidationSettings)
    session_manager: SessionManagerSettings = Field(default_factory=SessionManagerSettings)
    demo: DemoSettings = Field(default_factory=DemoSettings)

    @classmethod
    def from_json(cls, path: str | Path) -> "AppSettings":
        """Load settings from an appsettings.json file."""
        p = Path(path)
        if not p.exists():
            logger.warning("Config file not found: %s — using defaults", p)
            return cls()
        data = json.loads(p.read_text(encoding="utf-8"))
        return cls(
            site_config_file_path=_get(data, "SiteConfig:FilePath", "resources/xml/LocalVEHUSites.xml"),
            token_service=TokenServiceSettings(
                certificate_name=_get(data, "TokenServiceConfig:CertificateName", "CN=CarePlatform"),
                certificate_file_name=_get(data, "TokenServiceConfig:CertificateFileName", ""),
                certificate_password=_get(data, "TokenServiceConfig:CertificatePassword", ""),
                issuer_name=_get(data, "TokenServiceConfig:IssuerName", "CarePlatform"),
                audience=_get(data, "TokenServiceConfig:Audience", "urn:careplatform"),
                sts_base_address=_get(data, "TokenServiceConfig:STSBaseAddress", "https://localhost:5001/"),
            ),
            jwt_validation=JwtValidationSettings(
                validate_issuer=_get_bool(data, "JwtValidation:ValidateIssuer", True),
                validate_audience=_get_bool(data, "JwtValidation:ValidateAudience", True),
                validate_issuer_signing_key=_get_bool(data, "JwtValidation:ValidateIssuerSigningKey", False),
                validate_lifetime=_get_bool(data, "JwtValidation:ValidateLifetime", False),
            ),
            session_manager=SessionManagerSettings(
                session_manager_object=_get(data, "SessionManager:SessionManagerObject", ""),
                session_object=_get(data, "SessionManager:SessionObject", ""),
            ),
            demo=DemoSettings(
                test_files_path=_get(data, "Demo:TestFilesPath", "resources/demo-data"),
                user_name=_get(data, "Demo:UserName", "DEMO,USER"),
                duz=_get(data, "Demo:Duz", "12345"),
                site_id=_get(data, "Demo:SiteId", "500"),
            ),
        )


def _get(data: dict, key: str, default: str) -> str:
    """Traverse a nested dict by colon-separated key path."""
    parts = key.split(":")
    obj: Any = data
    for p in parts:
        if not isinstance(obj, dict):
            return default
        obj = obj.get(p)
        if obj is None:
            return default
    return str(obj) if obj is not None else default


def _get_bool(data: dict, key: str, default: bool) -> bool:
    val = _get(data, key, str(default))
    return val.lower() in ("true", "1", "yes")


# ---------------------------------------------------------------------------
# Global application state (mirrors C# ECConfiguration static class)
# ---------------------------------------------------------------------------
from app.platform.session.base import ISessionManager  # noqa: E402

_settings: AppSettings | None = None
_session_manager: ISessionManager | None = None
_auth_sites: dict[str, dict[str, str]] = {}
_facility_lookup: dict[str, str] = {}


def get_settings() -> AppSettings:
    global _settings
    if _settings is None:
        _settings = AppSettings()
    return _settings


def set_settings(s: AppSettings) -> None:
    global _settings
    _settings = s


def get_session_manager() -> ISessionManager:
    if _session_manager is None:
        raise RuntimeError("Session manager not initialized")
    return _session_manager


def set_session_manager(mgr: ISessionManager) -> None:
    global _session_manager
    _session_manager = mgr


def get_auth_sites() -> dict[str, dict[str, str]]:
    return _auth_sites


def get_facility_lookup() -> dict[str, str]:
    return _facility_lookup


def load_site_config(site_config_path: str) -> None:
    """Load site XML and populate auth_sites / facility_lookup dicts."""
    import xml.etree.ElementTree as ET

    global _auth_sites, _facility_lookup
    _auth_sites = {}
    _facility_lookup = {}

    ns = "http://med.va.gov/vistaweb/sitesTable"
    tree = ET.parse(site_config_path)
    root = tree.getroot()

    # Support both namespaced and non-namespaced XML
    visns = root.findall(f"{{{ns}}}VhaVisn")
    if not visns:
        visns = root.findall("VhaVisn")

    for visn in sorted(visns, key=lambda v: int(v.get("ID", "0"))):
        visn_label = f"VISN {visn.get('ID')} - {visn.get('name', '')}"
        site_dict: dict[str, str] = {}

        sites = visn.findall(f"{{{ns}}}VhaSite")
        if not sites:
            sites = visn.findall("VhaSite")

        for site in sites:
            site_id = site.get("ID", "")
            site_name = site.get("name", "")
            site_dict[site_id] = site_name
            _facility_lookup[site_id] = site_name

        _auth_sites[visn_label] = site_dict

    logger.info("Loaded %d site(s) from %s", len(_facility_lookup), site_config_path)


def get_site_list() -> list[dict[str, str]]:
    """Return a flat list of all configured sites for the login UI."""
    result: list[dict[str, str]] = []
    for visn_name, sites in _auth_sites.items():
        for site_id, site_name in sites.items():
            result.append({"site_id": site_id, "name": site_name, "visn_name": visn_name})
    return result
