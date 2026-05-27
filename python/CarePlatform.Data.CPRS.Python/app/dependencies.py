# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""FastAPI dependencies — mirrors SessionFilter + JWT auth from C#."""

from __future__ import annotations

import logging
from typing import Optional

from fastapi import Depends, HTTPException, Request, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer

from jose import JWTError, jwt

from app import config as app_config
from app.platform.security.certificate import get_public_key_pem
from app.platform.security.token_config import TokenServiceConfiguration
from app.platform.session.base import ISession

logger = logging.getLogger(__name__)

_bearer_scheme = HTTPBearer(auto_error=False)

CLAIM_CONTEXT_ID = "http://schemas.token.engineerecare.org/claims/ContextId"


async def _get_token(
    credentials: Optional[HTTPAuthorizationCredentials] = Depends(_bearer_scheme),
) -> str:
    if credentials is None or not credentials.credentials:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Missing authorization token",
        )
    return credentials.credentials


async def get_current_session(token: str = Depends(_get_token)) -> ISession:
    """Resolve the ISession from the JWT's ContextId claim.

    This is the FastAPI equivalent of the C# SessionFilter.
    """
    settings = app_config.get_settings()

    # Decode the JWT — validation params match C# JwtValidation section
    try:
        public_key = get_public_key_pem()
        payload = jwt.decode(
            token,
            public_key,
            algorithms=["RS256"],
            options={
                "verify_aud": settings.jwt_validation.validate_audience,
                "verify_iss": settings.jwt_validation.validate_issuer,
                "verify_exp": settings.jwt_validation.validate_lifetime,
            },
            audience=TokenServiceConfiguration.realm if settings.jwt_validation.validate_audience else None,
            issuer=TokenServiceConfiguration.issuer_name if settings.jwt_validation.validate_issuer else None,
        )
    except JWTError as exc:
        logger.warning("JWT validation failed: %s", exc)
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid token")

    context_id = payload.get(CLAIM_CONTEXT_ID) or payload.get("ContextId")
    if not context_id:
        logger.warning("No ContextId claim in JWT")
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="No session context")

    mgr = app_config.get_session_manager()
    if not mgr.session_exists(context_id):
        logger.warning("Session %s not found (expired?)", context_id)
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Session expired")

    return mgr.get_session(context_id)


async def get_context_id(token: str = Depends(_get_token)) -> str:
    """Extract just the ContextId from the JWT (for disconnect etc.)."""
    settings = app_config.get_settings()
    try:
        public_key = get_public_key_pem()
        payload = jwt.decode(
            token,
            public_key,
            algorithms=["RS256"],
            options={
                "verify_aud": settings.jwt_validation.validate_audience,
                "verify_iss": settings.jwt_validation.validate_issuer,
                "verify_exp": settings.jwt_validation.validate_lifetime,
            },
            audience=TokenServiceConfiguration.realm if settings.jwt_validation.validate_audience else None,
            issuer=TokenServiceConfiguration.issuer_name if settings.jwt_validation.validate_issuer else None,
        )
    except JWTError:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid token")

    return payload.get(CLAIM_CONTEXT_ID) or payload.get("ContextId") or ""
