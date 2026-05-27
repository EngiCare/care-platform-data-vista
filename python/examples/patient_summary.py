# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""
Patient Summary Example
=======================
Demonstrates how to pull a quick clinical summary for a patient:
1. Login
2. Select a patient
3. Fetch active orders, problems, vitals, and allergies
4. Print a consolidated summary

Usage:
    python examples/patient_summary.py --dfn 3
    python examples/patient_summary.py --last5 C1234
"""

import argparse
import os
import sys
import requests

API_URL = os.environ.get("CPRS_API_URL", "http://localhost:5200")
SITE_ID = os.environ.get("CPRS_SITE_ID", "128")
ACCESS_CODE = os.environ.get("CPRS_ACCESS_CODE", "cprs")
VERIFY_CODE = os.environ.get("CPRS_VERIFY_CODE", "cprs1234")


def login() -> tuple[str, dict]:
    """Login and return (token, headers)."""
    resp = requests.get(f"{API_URL}/api/connectbysite", params={
        "SiteId": SITE_ID,
        "AccessCode": ACCESS_CODE,
        "VerifyCode": VERIFY_CODE,
    })
    resp.raise_for_status()
    token = resp.text.strip('"')
    return token, {"Authorization": f"Bearer {token}"}


def print_section(title: str, items: list, indent: str = "  "):
    """Print a section with a header and items."""
    print(f"\n{'─' * 50}")
    print(f"  {title}")
    print(f"{'─' * 50}")
    if not items:
        print(f"{indent}(none)")
        return
    for item in items:
        if isinstance(item, dict):
            # Structured model — pick the most useful fields
            display = item.get("text") or item.get("description") or item.get("name") or str(item)
            print(f"{indent}• {display}")
        else:
            print(f"{indent}{item}")


def main():
    parser = argparse.ArgumentParser(description="Get a patient clinical summary")
    parser.add_argument("--dfn", help="Patient DFN")
    parser.add_argument("--last5", help="Patient search: last initial + last 4 SSN")
    args = parser.parse_args()

    if not args.dfn and not args.last5:
        print("Specify --dfn or --last5. Example: python examples/patient_summary.py --dfn 3")
        sys.exit(1)

    # ── Login ────────────────────────────────────────────────────
    print("Logging in...")
    token, headers = login()
    user_resp = requests.get(f"{API_URL}/api/user/info", headers=headers)
    user_resp.raise_for_status()
    user = user_resp.json()
    print(f"  Logged in as {user['name']} (DUZ={user['duz']})\n")

    # ── Find patient ─────────────────────────────────────────────
    dfn = args.dfn
    if not dfn:
        resp = requests.get(f"{API_URL}/api/patient/search/last5",
                            headers=headers, params={"last5": args.last5})
        resp.raise_for_status()
        results = resp.json()
        if not results:
            print("No patients found.")
            sys.exit(1)
        dfn = results[0].get("dfn", "")
        print(f"  Found patient DFN={dfn}: {results[0].get('name', '?')}")

    # ── Select patient ───────────────────────────────────────────
    resp = requests.get(f"{API_URL}/api/patient/select", headers=headers,
                        params={"dfn": dfn})
    resp.raise_for_status()
    demo = resp.json()

    print(f"\n{'═' * 50}")
    print(f"  PATIENT SUMMARY")
    print(f"{'═' * 50}")
    print(f"  Name:     {demo.get('name', '?')}")
    print(f"  DOB:      {demo.get('date_of_birth', '?')}")
    print(f"  Age:      {demo.get('age', '?')}")
    print(f"  Sex:      {demo.get('sex', '?')}")
    print(f"  SSN:      {demo.get('ssn', '?')}")
    print(f"  Location: {demo.get('location_name', 'N/A')}")
    print(f"  CWAD:     {demo.get('cwad', 'none')}")

    # ── Active Problems ──────────────────────────────────────────
    resp = requests.get(f"{API_URL}/api/problem/list", headers=headers,
                        params={"dfn": dfn, "status": "A"})
    resp.raise_for_status()
    problems = resp.json()
    print_section("ACTIVE PROBLEMS", problems)

    # ── Allergies ────────────────────────────────────────────────
    resp = requests.get(f"{API_URL}/api/coversheet/allergies", headers=headers,
                        params={"dfn": dfn})
    resp.raise_for_status()
    allergies = resp.json()
    print_section("ALLERGIES", allergies)

    # ── Active Orders ────────────────────────────────────────────
    resp = requests.get(f"{API_URL}/api/order/list", headers=headers,
                        params={"dfn": dfn, "filterTS": "2", "dGroup": "ALL"})
    resp.raise_for_status()
    orders = resp.json()
    print_section(f"ACTIVE ORDERS ({len(orders)})", orders[:10])
    if len(orders) > 10:
        print(f"  ... and {len(orders) - 10} more")

    # ── Latest Vitals ────────────────────────────────────────────
    resp = requests.get(f"{API_URL}/api/vital/latest", headers=headers,
                        params={"dfn": dfn})
    resp.raise_for_status()
    vitals = resp.json()
    print_section("LATEST VITALS", vitals)

    # ── Recent Labs ──────────────────────────────────────────────
    resp = requests.get(f"{API_URL}/api/lab/recent", headers=headers,
                        params={"dfn": dfn})
    resp.raise_for_status()
    labs = resp.json()
    print_section("RECENT LABS", labs[:10] if isinstance(labs, list) else [labs])
    if isinstance(labs, list) and len(labs) > 10:
        print(f"  ... and {len(labs) - 10} more")

    # ── Active Medications ───────────────────────────────────────
    resp = requests.get(f"{API_URL}/api/medication/active", headers=headers,
                        params={"dfn": dfn, "duz": user["duz"]})
    resp.raise_for_status()
    meds = resp.json()
    print_section("ACTIVE MEDICATIONS", meds)

    print(f"\n{'═' * 50}\n")

    # ── Cleanup ──────────────────────────────────────────────────
    requests.post(f"{API_URL}/api/session/disconnect", headers=headers)
    print("Done!")


if __name__ == "__main__":
    main()
