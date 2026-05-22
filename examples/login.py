# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""
Login Example
=============
Demonstrates how to:
1. Fetch available VistA sites
2. Authenticate and receive a JWT token
3. Retrieve current user information
4. Check session status
5. Disconnect cleanly

Usage:
    python examples/login.py
"""

import os
import requests

API_URL = os.environ.get("CPRS_API_URL", "http://localhost:5200")
SITE_ID = os.environ.get("CPRS_SITE_ID", "128")
ACCESS_CODE = os.environ.get("CPRS_ACCESS_CODE", "cprs")
VERIFY_CODE = os.environ.get("CPRS_VERIFY_CODE", "cprs1234")


def main():
    # ── Step 1: Get available sites ──────────────────────────────
    print("1. Fetching available sites...")
    resp = requests.get(f"{API_URL}/api/connection/sites")
    resp.raise_for_status()
    sites = resp.json()
    for site in sites:
        print(f"   Site {site['site_id']}: {site['name']} ({site['visn_name']})")

    # ── Step 2: Login ────────────────────────────────────────────
    print(f"\n2. Logging in to site {SITE_ID}...")
    resp = requests.get(f"{API_URL}/api/connectbysite", params={
        "SiteId": SITE_ID,
        "AccessCode": ACCESS_CODE,
        "VerifyCode": VERIFY_CODE,
    })
    resp.raise_for_status()
    token = resp.text.strip('"')  # JWT returned as plain string
    print(f"   Token received: {token[:50]}...")

    # All subsequent requests use this header
    headers = {"Authorization": f"Bearer {token}"}

    # ── Step 3: Get user info ────────────────────────────────────
    print("\n3. Fetching user info...")
    resp = requests.get(f"{API_URL}/api/user/info", headers=headers)
    resp.raise_for_status()
    user = resp.json()
    print(f"   Name:          {user['name']}")
    print(f"   DUZ:           {user['duz']}")
    print(f"   Class:         {user['user_class']}")
    print(f"   Can sign:      {user['can_sign']}")
    print(f"   Is provider:   {user['is_provider']}")
    print(f"   Station:       {user['station_number']}")

    # ── Step 4: Check session status ─────────────────────────────
    print("\n4. Session status...")
    resp = requests.get(f"{API_URL}/api/session/timeremaining", headers=headers)
    resp.raise_for_status()
    print(f"   Time remaining: {resp.text} seconds")

    # ── Step 5: Disconnect ───────────────────────────────────────
    print("\n5. Disconnecting...")
    resp = requests.post(f"{API_URL}/api/session/disconnect", headers=headers)
    resp.raise_for_status()
    print(f"   {resp.json()}")
    print("\nDone!")


if __name__ == "__main__":
    main()
