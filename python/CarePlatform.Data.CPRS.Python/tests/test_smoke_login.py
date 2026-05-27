# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Smoke test — connect to Mock VistA on localhost:9200 and log in."""

import asyncio
import sys

# Allow running from the project root
sys.path.insert(0, ".")

from app.platform.vista.models import VistaCredentials, VistaOption
from app.platform.vista.vista_connection import VistaConnection


async def main() -> None:
    cxn = VistaConnection("127.0.0.1", 9200, "128")

    print("1. Connecting to Mock VistA on 127.0.0.1:9200 ...")
    await cxn.connect()
    print("   OK — connected")

    print("2. Authenticating (cprs / cprs1234) ...")
    creds = VistaCredentials(account_name="cprs", account_password="cprs1234")
    permission = VistaOption(name="OR CPRS GUI CHART")
    user = await cxn.account.authenticate_and_authorize(creds, permission)
    print(f"   OK — DUZ={user.uid}, Name={user.name}")

    print("3. Setting division + context ...")
    await cxn.account.select_division_and_set_context("")
    print("   OK — context set")

    print("4. Heartbeat ...")
    await cxn.heartbeat()
    print("   OK")

    print("5. Disconnecting ...")
    await cxn.disconnect()
    print("   OK — disconnected")

    print("\n=== ALL PASSED ===")


if __name__ == "__main__":
    asyncio.run(main())
