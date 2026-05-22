# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""VistA TCP connection — mirrors VistaConnection.cs.

Uses asyncio streams (non-blocking) instead of the C# Socket + ManualResetEvent
pattern.  The protocol is identical: XWB envelope, \x04 terminator.
"""

from __future__ import annotations

import asyncio
import logging
import socket
from datetime import datetime
from typing import TYPE_CHECKING, Optional

from app.platform.vista.account import VistaAccount
from app.platform.vista.models import (
    AbstractPermission,
    DataSource,
    User,
    VistaCredentials,
    VistaOption,
)
from app.platform.vista.utils import str_pack
from app.platform.vista.vista_query import VistaQuery

logger = logging.getLogger(__name__)
rpc_trace = logging.getLogger("VistaRpcTrace")

CONNECTION_TIMEOUT = 30  # seconds
READ_TIMEOUT = 60  # seconds
DEFAULT_PORT = 9200
BUFFER_SIZE = 4096


class VistaConnection:
    """Async TCP connection to a VistA broker (or mock RPC server)."""

    def __init__(self, hostname: str, port: str | int, site_id: str) -> None:
        self.hostname = hostname
        self.port = int(port) if port else DEFAULT_PORT
        self.site_id = site_id
        self.uid: str = ""
        self.last_used = datetime.now()
        self.is_connected = False
        self.account = VistaAccount(self)

        self._reader: Optional[asyncio.StreamReader] = None
        self._writer: Optional[asyncio.StreamWriter] = None
        self._lock = asyncio.Lock()

    # ------------------------------------------------------------------
    # Connect
    # ------------------------------------------------------------------

    async def connect(self) -> None:
        if not self.hostname:
            raise ValueError("No provider (hostname)")

        logger.debug("Connecting to VistA %s:%s", self.hostname, self.port)

        self._reader, self._writer = await asyncio.wait_for(
            asyncio.open_connection(self.hostname, self.port),
            timeout=CONNECTION_TIMEOUT,
        )

        # Build the initial connect request — COUNT_WIDTH=3 for TCPConnect handshake
        my_ip = socket.gethostbyname(socket.gethostname())
        my_hostname = socket.gethostname()
        count_w = 3
        request = (
            "[XWB]10"
            + str(count_w)
            + "04\nTCPConnect50"
            + str_pack(my_ip, count_w)
            + "f0"
            + str_pack("0", count_w)
            + "f0"
            + str_pack(my_hostname, count_w)
            + "f\x04"
        )

        reply = await self._send_and_receive(request)
        if reply != "accept":
            await self._close_writer()
            raise ConnectionError(f"Unaccepted by {self.hostname}")

        # Intro message
        intro_request = "[XWB]11302\x00010\rXUS INTRO MSG54f\x04"
        await self._send_and_receive(intro_request)

        self.is_connected = True
        self.last_used = datetime.now()
        logger.info("Connected to VistA %s:%s (site %s)", self.hostname, self.port, self.site_id)

    # ------------------------------------------------------------------
    # Query — public API
    # ------------------------------------------------------------------

    async def query(self, vq: VistaQuery, context: AbstractPermission | None = None, update_time: bool = True) -> str:
        """Execute an RPC via VistaQuery object."""
        request = vq.build_message()

        if rpc_trace.isEnabledFor(logging.DEBUG):
            rpc_trace.debug("[data] %s %s", self.site_id, vq.rpc_name)

        reply = await self.query_raw(request, context, update_time)

        if rpc_trace.isEnabledFor(logging.DEBUG):
            rpc_trace.debug(
                "[data] %s %s -- response (%d bytes) --\n%s",
                self.site_id,
                vq.rpc_name,
                len(reply) if reply else 0,
                reply or "",
            )
        return reply

    async def query_raw(
        self,
        request: str,
        context: AbstractPermission | None = None,
        update_time: bool = True,
    ) -> str:
        """Send a raw XWB-encoded request string and return the response."""
        if not self.is_connected and not self._writer:
            raise RuntimeError("Not connected")

        # Context switching (if needed)
        current_context: AbstractPermission | None = None
        if context and context.name != self.account.primary_permission.name:
            current_context = self.account.primary_permission
            await self.account.set_context(context)

        try:
            reply = await self._send_and_receive(request)

            if current_context:
                await self.account.set_context(current_context)

            if reply and "M  ERROR" in reply:
                raise RuntimeError(f"M ERROR: {reply}")

            if update_time:
                self.last_used = datetime.now()

            return reply
        except Exception:
            raise

    # ------------------------------------------------------------------
    # Authorized connect (authenticate during connect)
    # ------------------------------------------------------------------

    async def authorized_connect(
        self,
        credentials: VistaCredentials,
        permission: AbstractPermission,
        validation_data_source: DataSource | None = None,
    ) -> User:
        await self.connect()
        return await self.account.authenticate_and_authorize(credentials, permission, validation_data_source)

    async def authorized_connect_sso(
        self,
        credentials: VistaCredentials,
        permission: AbstractPermission,
        validation_data_source: DataSource | None = None,
    ) -> User:
        await self.connect()
        return await self.account.authenticate_and_authorize_sso(credentials, permission, validation_data_source)

    # ------------------------------------------------------------------
    # Heartbeat / disconnect
    # ------------------------------------------------------------------

    async def heartbeat(self, update_session_time: bool = False) -> None:
        vq = VistaQuery("XWB IM HERE")
        await self.query(vq, update_time=update_session_time)

    async def disconnect(self) -> None:
        if not self.is_connected:
            return
        try:
            msg = "[XWB]10304\x05#BYE#\x04"
            await self._send_and_receive(msg)
        except Exception as e:
            logger.warning("Exception disconnecting: %s", e)
        finally:
            await self._close_writer()
            self.is_connected = False

    # ------------------------------------------------------------------
    # Low-level socket I/O
    # ------------------------------------------------------------------

    async def _send_and_receive(self, request: str) -> str:
        """Thread-safe send + receive with \x04 termination."""
        async with self._lock:
            if not self._writer or not self._reader:
                raise RuntimeError("No active connection")

            self._writer.write(request.encode("ascii"))
            await self._writer.drain()

            # Read until \x04 terminator
            data = bytearray()
            while True:
                chunk = await asyncio.wait_for(
                    self._reader.read(BUFFER_SIZE),
                    timeout=READ_TIMEOUT,
                )
                if not chunk:
                    raise ConnectionError("Connection closed by VistA")
                data.extend(chunk)
                end_idx = data.find(b"\x04")
                if end_idx != -1:
                    data = data[:end_idx]
                    break

            return self._parse_response(bytes(data))

    @staticmethod
    def _parse_response(raw: bytes) -> str:
        """Parse the XWB response envelope.

        The first two bytes are error/security flags:
        - If byte[0] != 0: error message starting at byte[1] for byte[0] chars.
        - If byte[1] != 0: error, payload starts at byte[2].
        - Otherwise: normal response starts at byte[2].
        """
        if len(raw) == 0:
            return ""

        is_error = False
        if raw[0] != 0:
            # Error message — byte[0] is length
            msg_len = raw[0]
            text = raw[1 : 1 + msg_len].decode("ascii", errors="replace")
            is_error = True
            return text
        elif len(raw) > 1 and raw[1] != 0:
            is_error = True
            return raw[2:].decode("ascii", errors="replace")
        else:
            return raw[2:].decode("ascii", errors="replace")

    async def _close_writer(self) -> None:
        if self._writer:
            try:
                self._writer.close()
                await self._writer.wait_closed()
            except Exception:
                pass
            self._writer = None
            self._reader = None
