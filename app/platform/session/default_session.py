# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Default session — mirrors Session.cs.

Wraps a VistaConnection and exposes query/heartbeat/disconnect.
"""

from __future__ import annotations

import logging
from datetime import datetime
from typing import Optional

from app.platform.session.base import ISession
from app.platform.vista.constants import CPRS_CONTEXT
from app.platform.vista.models import User, VistaCredentials, VistaOption
from app.platform.vista.vista_connection import VistaConnection
from app.platform.vista.vista_query import VistaQuery

logger = logging.getLogger(__name__)

# JWT claim URIs (match the C# constants)
CLAIM_CONTEXT_ID = "http://schemas.token.engineerecare.org/claims/ContextId"
CLAIM_SURNAME = "surname"
CLAIM_NAME = "name"
CLAIM_SID = "sid"
CLAIM_PRIMARY_SID = "primary_sid"
CLAIM_LOCALITY = "locality"


class Session(ISession):
    """Live VistA session backed by a TCP VistaConnection."""

    def __init__(
        self,
        site_id: str,
        hostname: str,
        port: str,
        access_code: str,
        verify_code: str,
        sso_token: str = "",
    ) -> None:
        self._connection = VistaConnection(hostname, port, site_id)
        creds = VistaCredentials(
            account_name=access_code,
            account_password=verify_code,
            sso_token=sso_token,
        )
        permission = VistaOption(name=CPRS_CONTEXT)

        # NOTE: The C# code does .Result (blocking) in the constructor.
        # We can't await in __init__, so we defer to an async factory.
        self._creds = creds
        self._permission = permission
        self._user: Optional[User] = None
        self._is_context_set = False

    async def initialize(self) -> None:
        """Async initialization — must be called after construction."""
        if self._creds.sso_token:
            self._user = await self._connection.authorized_connect_sso(
                self._creds, self._permission, None
            )
        else:
            self._user = await self._connection.authorized_connect(
                self._creds, self._permission, None
            )

    def user_to_claims(self, session_id: str) -> dict[str, str]:
        claims: dict[str, str] = {}
        if self._user:
            if self._user.name:
                claims[CLAIM_SURNAME] = self._user.name.lastname
                claims[CLAIM_NAME] = self._user.name.firstname
            claims[CLAIM_SID] = self._user.uid
            claims[CLAIM_PRIMARY_SID] = self._user.uid
            if self._user.logon_site_id:
                claims[CLAIM_LOCALITY] = self._user.logon_site_id.id
        claims[CLAIM_CONTEXT_ID] = session_id
        return claims

    @property
    def cached_division_lines(self) -> list[str] | None:
        return self._user.division_lines if self._user else None

    @property
    def is_context_set(self) -> bool:
        return self._is_context_set

    async def select_division_and_set_context(self, station_number: str) -> None:
        await self._connection.account.select_division_and_set_context(station_number)
        self._is_context_set = True

    @property
    def connection_site_id(self) -> str:
        return self._connection.site_id

    @property
    def connection_last_used(self) -> datetime:
        return self._connection.last_used

    @property
    def session_timeout_seconds(self) -> int:
        return 900

    async def heartbeat(self, update_session_time: bool = False) -> None:
        await self._connection.heartbeat(update_session_time)

    async def disconnect(self) -> None:
        await self._connection.disconnect()

    async def query(self, vq: VistaQuery) -> str:
        return await self._connection.query(vq)

    async def t_query(self, vq: VistaQuery) -> list[str]:
        result = await self.query(vq)
        return result.split("\r\n")

    async def s_query(self, vq: VistaQuery) -> str:
        result = await self.query(vq)
        return result.split("\r\n")[0] if result else ""
