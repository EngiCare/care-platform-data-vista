# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Default session manager — mirrors SessionManager.cs.

In-memory dict of sessions with a background reaper task.
"""

from __future__ import annotations

import asyncio
import logging
from datetime import datetime
from typing import Optional

from app.platform.session.base import ISession, ISessionManager

logger = logging.getLogger(__name__)


class SessionManager(ISessionManager):
    """Thread-safe in-memory session store with periodic cleanup."""

    def __init__(self) -> None:
        self._sessions: dict[str, ISession] = {}
        self._lock = asyncio.Lock()
        self._reaper_task: Optional[asyncio.Task] = None

    def start_reaper(self) -> None:
        """Start the background session maintenance loop.

        Must be called once the event loop is running (e.g. on app startup).
        """
        if self._reaper_task is None:
            self._reaper_task = asyncio.create_task(self._maintain_sessions_loop())

    async def _maintain_sessions_loop(self) -> None:
        while True:
            await asyncio.sleep(30)
            try:
                await self._maintain_sessions()
            except Exception as exc:
                logger.error("Error during session maintenance: %s", exc)

    async def _maintain_sessions(self) -> None:
        expired: list[str] = []
        async with self._lock:
            for sid, session in list(self._sessions.items()):
                try:
                    deadline = session.connection_last_used.timestamp() + session.session_timeout_seconds
                    if datetime.now().timestamp() >= deadline:
                        logger.debug("Session %s expired, disconnecting", sid)
                        await session.disconnect()
                        expired.append(sid)
                    else:
                        await session.heartbeat()
                except Exception as exc:
                    logger.error("Error maintaining session %s: %s", sid, exc)

            for sid in expired:
                self._sessions.pop(sid, None)

    def add_session(self, session_id: str, session: ISession) -> None:
        self._sessions[session_id] = session

    def get_session(self, session_id: str) -> ISession:
        return self._sessions[session_id]

    def remove_session(self, session_id: str) -> bool:
        return self._sessions.pop(session_id, None) is not None

    def session_exists(self, session_id: str) -> bool:
        return session_id in self._sessions

    def dump_debug_info(self) -> None:
        logger.debug("Session count: %d", len(self._sessions))
        for sid in self._sessions:
            logger.debug("  Session: %s", sid)
