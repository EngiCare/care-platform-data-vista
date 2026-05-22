# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Session abstractions — mirrors ISession.cs and ISessionManager.cs."""

from __future__ import annotations

import abc
from datetime import datetime
from typing import Any, Optional


class ISession(abc.ABC):
    """Abstract session interface."""

    @property
    @abc.abstractmethod
    def connection_last_used(self) -> datetime: ...

    @property
    @abc.abstractmethod
    def connection_site_id(self) -> str: ...

    @property
    @abc.abstractmethod
    def session_timeout_seconds(self) -> int: ...

    @property
    @abc.abstractmethod
    def cached_division_lines(self) -> list[str] | None: ...

    @property
    @abc.abstractmethod
    def is_context_set(self) -> bool: ...

    @abc.abstractmethod
    async def select_division_and_set_context(self, station_number: str) -> None: ...

    @abc.abstractmethod
    def user_to_claims(self, session_id: str) -> dict[str, str]: ...

    @abc.abstractmethod
    async def disconnect(self) -> None: ...

    @abc.abstractmethod
    async def heartbeat(self, update_session_time: bool = False) -> None: ...

    @abc.abstractmethod
    async def query(self, vq: Any) -> str: ...

    @abc.abstractmethod
    async def t_query(self, vq: Any) -> list[str]: ...

    @abc.abstractmethod
    async def s_query(self, vq: Any) -> str: ...


class ISessionManager(abc.ABC):
    """Abstract session manager."""

    @abc.abstractmethod
    def add_session(self, session_id: str, session: ISession) -> None: ...

    @abc.abstractmethod
    def get_session(self, session_id: str) -> ISession: ...

    @abc.abstractmethod
    def remove_session(self, session_id: str) -> bool: ...

    @abc.abstractmethod
    def session_exists(self, session_id: str) -> bool: ...
