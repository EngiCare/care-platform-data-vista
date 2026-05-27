# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""VistA account — authentication/authorization logic.

Mirrors VistaAccount.cs.  Two-phase login:
  Phase 1: authenticate(credentials) → authorize(credentials, permission)
  Phase 2: selectDivisionAndSetContext(stationNumber)
"""

from __future__ import annotations

import logging
from typing import TYPE_CHECKING

from app.platform.vista.constants import CPRS_CONTEXT
from app.platform.vista.models import (
    AbstractPermission,
    DataSource,
    PersonName,
    Service,
    SiteId,
    SocSecNum,
    User,
    VistaCredentials,
    VistaOption,
)
from app.platform.vista.vista_query import VistaQuery, encrypt

if TYPE_CHECKING:
    from app.platform.vista.vista_connection import VistaConnection

logger = logging.getLogger(__name__)


class VistaAccount:
    """Manages VistA login, context switching, and permission checks."""

    def __init__(self, connection: VistaConnection) -> None:
        self._cxn = connection
        self.primary_permission: AbstractPermission = VistaOption(name=CPRS_CONTEXT)
        self.permissions: dict[str, AbstractPermission] = {}
        self.is_authenticated = False
        self.is_authorized = False

    # ------------------------------------------------------------------
    # Phase 1 — authenticate + authorize (still under XUS SIGNON context)
    # ------------------------------------------------------------------

    async def authenticate(
        self,
        credentials: VistaCredentials,
        validation_data_source: DataSource | None = None,
    ) -> str:
        """Access/verify code login.  Returns the greeting string."""
        vq = VistaQuery("XUS SIGNON SETUP")
        await self._cxn.query_raw(vq.build_message())

        # Encrypt and send access;verify
        av = f"{credentials.account_name};{credentials.account_password}"
        login_vq = VistaQuery("XUS AV CODE")
        login_vq.add_encrypted_parameter(VistaQuery.LITERAL, av)
        response = await self._cxn.query_raw(login_vq.build_message())

        flds = response.split("\r\n")
        if len(flds) < 1 or flds[0] == "" or flds[0] == "0":
            err = flds[3] if len(flds) > 3 else "Login failed"
            raise PermissionError(err)

        self.is_authenticated = True
        greeting = flds[7] if len(flds) > 7 else ""
        return greeting

    async def authenticate_sso(
        self,
        credentials: VistaCredentials,
        validation_data_source: DataSource | None = None,
    ) -> str:
        """SSO token login using ESSO VALIDATE.  Returns the greeting string."""
        vq = VistaQuery("XUS SIGNON SETUP")
        await self._cxn.query_raw(vq.build_message())

        from app.platform.vista.dict_hash_list import DictionaryHashList

        login_vq = VistaQuery("XUS ESSO VALIDATE")
        # Split SSO token into 200-char chunks as GLOBAL param
        token = credentials.sso_token
        lst = DictionaryHashList()
        idx = 1
        for i in range(0, len(token), 200):
            lst.add(str(idx), token[i : i + 200])
            idx += 1
        login_vq.add_parameter(VistaQuery.GLOBAL, lst)
        response = await self._cxn.query_raw(login_vq.build_message())

        flds = response.split("\r\n")
        if len(flds) < 1 or flds[0] == "" or flds[0] == "0":
            err = flds[3] if len(flds) > 3 else "SSO Login failed"
            raise PermissionError(err)

        self.is_authenticated = True
        greeting = flds[7] if len(flds) > 7 else ""
        return greeting

    async def authorize(
        self,
        credentials: VistaCredentials,
        permission: AbstractPermission,
    ) -> User:
        """Fetch user info + divisions (still under XUS SIGNON context)."""
        self.primary_permission = permission

        # Get user info
        vq = VistaQuery("XUS GET USER INFO")
        response = await self._cxn.query_raw(vq.build_message())
        flds = response.split("\r\n")

        user = User()
        if len(flds) > 0:
            user.uid = flds[0]
        if len(flds) > 1:
            user.name = PersonName(flds[1])
        if len(flds) > 2 and flds[2]:
            # flds[2] format: "LAST FOUR" of SSN
            pass
        if len(flds) > 4:
            user.title = flds[4]
        if len(flds) > 5 and flds[5]:
            user.service = Service(name=flds[5])

        user.logon_site_id = SiteId(id=self._cxn.site_id)

        # Fetch divisions while still in XUS SIGNON context
        div_vq = VistaQuery("XUS DIVISION GET")
        div_response = await self._cxn.query_raw(div_vq.build_message())
        user.division_lines = div_response.split("\r\n") if div_response else []

        self._cxn.uid = user.uid
        self.is_authorized = True
        return user

    # ------------------------------------------------------------------
    # Phase 2 — select division + set OR CPRS GUI CHART context
    # ------------------------------------------------------------------

    async def select_division_and_set_context(self, station_number: str) -> None:
        """Phase 2: set division then switch to CPRS context."""
        if station_number:
            div_vq = VistaQuery("XUS DIVISION SET")
            div_vq.add_parameter(VistaQuery.LITERAL, station_number)
            await self._cxn.query_raw(div_vq.build_message())

        await self.set_context(self.primary_permission)

    async def set_context(self, permission: AbstractPermission) -> None:
        """Switch broker context (encrypts the context name)."""
        vq = VistaQuery("XWB CREATE CONTEXT")
        vq.add_encrypted_parameter(VistaQuery.LITERAL, permission.name)
        response = await self._cxn.query_raw(vq.build_message())
        if response and response.strip() == "1":
            self.permissions[permission.name] = permission
        elif response and "M  ERROR" in response:
            raise PermissionError(f"Failed to set context: {response}")

    # ------------------------------------------------------------------
    # Combined helpers
    # ------------------------------------------------------------------

    async def authenticate_and_authorize(
        self,
        credentials: VistaCredentials,
        permission: AbstractPermission,
        validation_data_source: DataSource | None = None,
    ) -> User:
        msg = await self.authenticate(credentials, validation_data_source)
        user = await self.authorize(credentials, permission)
        user.greeting = msg
        return user

    async def authenticate_and_authorize_sso(
        self,
        credentials: VistaCredentials,
        permission: AbstractPermission,
        validation_data_source: DataSource | None = None,
    ) -> User:
        msg = await self.authenticate_sso(credentials, validation_data_source)
        user = await self.authorize(credentials, permission)
        user.greeting = msg
        return user
