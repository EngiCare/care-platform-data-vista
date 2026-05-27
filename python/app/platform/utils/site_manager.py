# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Site config lookup — mirrors SiteManager.cs (SiteConfigManager)."""

from __future__ import annotations

import logging
import os
import xml.etree.ElementTree as ET
from pathlib import Path

from app import config as app_config

logger = logging.getLogger(__name__)

NS = "http://med.va.gov/vistaweb/sitesTable"


def _resolve_site_config_path() -> str:
    """Resolve the site config path (may be relative to project root)."""
    path = app_config.get_settings().site_config_file_path
    if not os.path.isabs(path):
        base = Path(__file__).resolve().parent.parent.parent.parent  # project root
        path = str(base / path)
    return path


def _find_site_datasource(site_id: str) -> ET.Element:
    """Find the VISTA DataSource element for a given site ID."""
    path = _resolve_site_config_path()
    tree = ET.parse(path)
    root = tree.getroot()

    visns = root.findall(f"{{{NS}}}VhaVisn")
    if not visns:
        visns = root.findall("VhaVisn")

    for visn in visns:
        sites = visn.findall(f"{{{NS}}}VhaSite")
        if not sites:
            sites = visn.findall("VhaSite")

        for site in sites:
            if site.get("ID") == site_id:
                sources = site.findall(f"{{{NS}}}DataSource")
                if not sources:
                    sources = site.findall("DataSource")
                for ds in sources:
                    if ds.get("protocol") == "VISTA":
                        return ds

    raise LookupError(f"Site not found in configuration: {site_id}")


def lookup_hostname(site_id: str) -> str:
    ds = _find_site_datasource(site_id)
    return ds.get("source", "")


def lookup_port(site_id: str) -> str:
    ds = _find_site_datasource(site_id)
    return ds.get("port", "")
