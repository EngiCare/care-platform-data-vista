# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Session factory — mirrors SessionFactory.cs.

Config-driven factory (no reflection).  Returns default Session/SessionManager
unless overridden in settings.
"""

from __future__ import annotations

import logging

from app.platform.session.base import ISession, ISessionManager
from app.platform.session.default_session import Session
from app.platform.session.manager import SessionManager

logger = logging.getLogger(__name__)

# Registry of known session/manager types (avoids reflection)
_SESSION_TYPES: dict[str, type] = {
    "": None,  # default
}
_MANAGER_TYPES: dict[str, type] = {
    "": None,  # default
}


def register_session_type(fqn: str, cls: type) -> None:
    _SESSION_TYPES[fqn] = cls


def register_manager_type(fqn: str, cls: type) -> None:
    _MANAGER_TYPES[fqn] = cls


def get_default_session_manager(session_manager_class: str = "") -> ISessionManager:
    if not session_manager_class or session_manager_class not in _MANAGER_TYPES:
        return SessionManager()
    cls = _MANAGER_TYPES[session_manager_class]
    if cls is None:
        return SessionManager()
    return cls()


async def get_default_session(
    session_class: str,
    site_id: str,
    hostname: str,
    port: str,
    access_code: str,
    verify_code: str,
    sso_token: str = "",
) -> ISession:
    if not session_class or session_class not in _SESSION_TYPES:
        sess = Session(site_id, hostname, port, access_code, verify_code, sso_token)
        await sess.initialize()
        return sess
    cls = _SESSION_TYPES[session_class]
    if cls is None:
        sess = Session(site_id, hostname, port, access_code, verify_code, sso_token)
        await sess.initialize()
        return sess
    return cls()
