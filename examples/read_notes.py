# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""
Read Notes Example
==================
Demonstrates how to:
1. Login to VistA
2. Search for a patient by last name initial + last 4 SSN
3. Select the patient
4. List their clinical notes
5. Read the full text of a note

Usage:
    python examples/read_notes.py
    python examples/read_notes.py --last5 C1234
    python examples/read_notes.py --dfn 3
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


def main():
    parser = argparse.ArgumentParser(description="Read clinical notes from VistA")
    parser.add_argument("--last5", help="Patient search: last initial + last 4 SSN (e.g., C1234)")
    parser.add_argument("--dfn", help="Patient DFN (skip search, use directly)")
    parser.add_argument("--max-notes", type=int, default=5, help="Max notes to list (default: 5)")
    args = parser.parse_args()

    # ── Login ────────────────────────────────────────────────────
    print("Logging in...")
    token, headers = login()
    print("  Authenticated.\n")

    # ── Find patient ─────────────────────────────────────────────
    dfn = args.dfn

    if not dfn:
        last5 = args.last5
        if not last5:
            # Default: list a known test patient or prompt
            print("No --last5 or --dfn specified. Searching for first available patient...")
            resp = requests.get(f"{API_URL}/api/patient/list/all", headers=headers,
                                params={"startFrom": "", "direction": 1})
            resp.raise_for_status()
            patients = resp.json()
            if not patients:
                print("No patients found.")
                sys.exit(1)
            # First line is often a header; find one with a DFN
            for line in patients:
                if "^" in str(line):
                    dfn = str(line).split("^")[0]
                    name = str(line).split("^")[1] if len(str(line).split("^")) > 1 else "?"
                    print(f"  Using patient: {name} (DFN={dfn})\n")
                    break
            if not dfn:
                print("Could not find a patient.")
                sys.exit(1)
        else:
            print(f"Searching for patient: {last5}...")
            resp = requests.get(f"{API_URL}/api/patient/search/last5",
                                headers=headers, params={"last5": last5})
            resp.raise_for_status()
            results = resp.json()
            if not results:
                print("  No patients found for that search.")
                sys.exit(1)
            # Pick the first result
            patient = results[0]
            dfn = patient.get("dfn", "")
            name = patient.get("name", "?")
            print(f"  Found: {name} (DFN={dfn})\n")

    # ── Select patient ───────────────────────────────────────────
    print(f"Selecting patient DFN={dfn}...")
    resp = requests.get(f"{API_URL}/api/patient/select", headers=headers,
                        params={"dfn": dfn})
    resp.raise_for_status()
    demo = resp.json()
    print(f"  Name: {demo.get('name', '?')}")
    print(f"  DOB:  {demo.get('date_of_birth', '?')}")
    print(f"  Sex:  {demo.get('sex', '?')}")
    print(f"  CWAD: {demo.get('cwad', 'none')}\n")

    # ── List notes ───────────────────────────────────────────────
    print(f"Listing notes (max {args.max_notes})...")
    resp = requests.get(f"{API_URL}/api/note/list", headers=headers, params={
        "dfn": dfn,
        "context": 1,       # 1 = All signed notes
        "early": "",         # No date filter
        "late": "",
        "occLim": args.max_notes,
    })
    resp.raise_for_status()
    notes = resp.json()

    if not notes:
        print("  No notes found for this patient.")
        sys.exit(0)

    print(f"  Found {len(notes)} note(s):\n")
    for i, note in enumerate(notes):
        print(f"  [{i+1}] IEN={note['ien']}  {note.get('title', '?')}")
        print(f"      Date:   {note.get('reference_date', '?')}")
        print(f"      Author: {note.get('author_name', '?')}")
        print(f"      Status: {note.get('status', '?')}")
        print()

    # ── Read first note's text ───────────────────────────────────
    first_note = notes[0]
    ien = first_note["ien"]
    print(f"Reading text of note IEN={ien} ({first_note.get('title', '?')})...")
    print("=" * 60)

    resp = requests.get(f"{API_URL}/api/note/text", headers=headers,
                        params={"ien": int(ien)})
    resp.raise_for_status()
    text_lines = resp.json()

    if isinstance(text_lines, list):
        for line in text_lines:
            print(line)
    else:
        print(text_lines)

    print("=" * 60)

    # ── Cleanup ──────────────────────────────────────────────────
    print("\nDisconnecting...")
    requests.post(f"{API_URL}/api/session/disconnect", headers=headers)
    print("Done!")


if __name__ == "__main__":
    main()
