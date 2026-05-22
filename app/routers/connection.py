# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Connection router — mirrors ConnectionController.cs.

Handles login/connect endpoints and JWT token creation.
"""

from __future__ import annotations

import logging
import uuid
from datetime import datetime, timedelta, timezone
from typing import Optional

from fastapi import APIRouter, HTTPException, Query
from jose import jwt

from app import config as app_config
from app.models.session import SiteEntry
from app.platform.security.certificate import get_private_key_pem
from app.platform.security.token_config import TokenServiceConfiguration
from app.platform.session.factory import get_default_session
from app.platform.utils.site_manager import lookup_hostname, lookup_port

logger = logging.getLogger(__name__)

router = APIRouter()


def _build_jwt(claims: dict[str, str]) -> str:
    """Build a signed JWT from session claims."""
    private_key = get_private_key_pem()
    now = datetime.now(timezone.utc)
    payload = {
        **claims,
        "iss": TokenServiceConfiguration.issuer_name,
        "aud": TokenServiceConfiguration.realm,
        "nbf": now,
        "exp": now + timedelta(minutes=20),
        "iat": now,
    }
    return jwt.encode(payload, private_key, algorithm="RS256")


async def _initialize_session(
    site_id: str,
    hostname: str,
    port: str,
    access_code: str,
    verify_code: str,
    sso_token: str = "",
) -> dict[str, str]:
    """Create a Session, register it, return claims dict."""
    settings = app_config.get_settings()
    session = await get_default_session(
        session_class=settings.session_manager.session_object,
        site_id=site_id,
        hostname=hostname,
        port=port,
        access_code=access_code,
        verify_code=verify_code,
        sso_token=sso_token,
    )
    session_id = str(uuid.uuid4())
    app_config.get_session_manager().add_session(session_id, session)
    return session.user_to_claims(session_id)


@router.get("/api/connect")
async def connect_to_vista(
    SiteId: str = Query(""),
    HostName: str = Query(""),
    Port: str = Query(""),
    AccessCode: str = Query(""),
    VerifyCode: str = Query(""),
    SSOToken: str = Query(""),
):
    try:
        claims = await _initialize_session(SiteId, HostName, Port, AccessCode, VerifyCode, SSOToken)
    except PermissionError as exc:
        raise HTTPException(status_code=401, detail=str(exc))
    except Exception as exc:
        logger.error("Connect error: %s", exc)
        raise HTTPException(status_code=401, detail=str(exc))
    return _build_jwt(claims)


@router.get("/api/login")
async def login_to_vista(
    username: str = Query(""),
    password: str = Query(""),
    siteId: str = Query("640"),
    SSOToken: str = Query(""),
):
    return await connect_by_site(siteId, username, password, SSOToken)


@router.get("/api/connection/sites")
async def get_sites() -> list[dict[str, str]]:
    return app_config.get_site_list()


@router.get("/api/connectbysite")
async def connect_by_site_get(
    SiteId: str = Query(""),
    AccessCode: str = Query(""),
    VerifyCode: str = Query(""),
    SSOToken: str = Query(""),
):
    return await connect_by_site(SiteId, AccessCode, VerifyCode, SSOToken)


@router.post("/api/connectbysite")
async def connect_by_site_post(
    SiteId: str = Query(""),
    AccessCode: str = Query(""),
    VerifyCode: str = Query(""),
    SSOToken: str = Query(""),
):
    return await connect_by_site(SiteId, AccessCode, VerifyCode, SSOToken)


async def connect_by_site(
    site_id: str,
    access_code: str,
    verify_code: str,
    sso_token: str = "",
) -> str:
    if not site_id:
        raise HTTPException(status_code=400, detail="SiteId is required.")
    if not sso_token and (not access_code or not verify_code):
        raise HTTPException(
            status_code=400,
            detail="AccessCode and VerifyCode are required when SSOToken is not provided.",
        )

    hostname = lookup_hostname(site_id)
    port = lookup_port(site_id)

    try:
        claims = await _initialize_session(site_id, hostname, port, access_code, verify_code, sso_token)
    except PermissionError as exc:
        raise HTTPException(status_code=401, detail=str(exc))
    except Exception as exc:
        logger.error("ConnectBySite error: %s", exc)
        raise HTTPException(status_code=401, detail=str(exc))
    return _build_jwt(claims)
