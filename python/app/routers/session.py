# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Session router — mirrors SessionController.cs."""

from __future__ import annotations

import logging
from datetime import datetime

from fastapi import APIRouter, Depends

from app import config as app_config
from app.dependencies import get_context_id, get_current_session
from app.platform.session.base import ISession
from app.platform.vista.dict_hash_list import VistaTimestamp
from app.platform.vista.vista_query import VistaQuery

logger = logging.getLogger(__name__)

router = APIRouter()


@router.get("/api/session/timeremaining")
async def session_status(session: ISession = Depends(get_current_session)) -> int:
    vq = VistaQuery("XWB GET BROKER INFO")
    result = await session.s_query(vq)
    try:
        return int(result)
    except (ValueError, TypeError):
        pass

    # Fallback to local calculation
    deadline = session.connection_last_used.timestamp() + session.session_timeout_seconds
    if datetime.now().timestamp() >= deadline:
        return -1
    return int(deadline - datetime.now().timestamp())


@router.get("/api/session/pulse")
async def session_heartbeat(session: ISession = Depends(get_current_session)) -> bool:
    await session.heartbeat(update_session_time=True)
    logger.debug("Pulse received")
    return True


@router.get("/api/session/currenttime")
async def current_time(session: ISession = Depends(get_current_session)) -> str:
    vq = VistaQuery("ORWU DT")
    vq.add_parameter(VistaQuery.LITERAL, "NOW")
    result = await session.query(vq)
    return VistaTimestamp.to_utc_string(result)


@router.post("/api/session/disconnect")
async def disconnect(
    session: ISession = Depends(get_current_session),
    context_id: str = Depends(get_context_id),
):
    try:
        logger.info("Disconnect requested for session %s", context_id)
        await session.disconnect()
        if context_id:
            app_config.get_session_manager().remove_session(context_id)
        logger.info("Session %s disconnected and removed", context_id)
    except Exception as exc:
        logger.error("Error disconnecting session: %s", exc)
    return {"status": "ok"}
