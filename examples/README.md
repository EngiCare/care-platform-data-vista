# Example Scripts

Ready-to-run Python scripts demonstrating common API workflows.

## Prerequisites

```bash
pip install requests
```

All scripts default to `http://localhost:5200` and Site 128 with test credentials.
Override with environment variables:

```bash
export CPRS_API_URL=http://localhost:5200
export CPRS_SITE_ID=128
export CPRS_ACCESS_CODE=cprs
export CPRS_VERIFY_CODE=cprs1234
```

## Scripts

| Script | Description |
|--------|-------------|
| `login.py` | Authenticate, get JWT, display user info |
| `read_notes.py` | Search for a patient, list their notes, read note text |
| `patient_summary.py` | Get a patient's active orders, problems, vitals, and allergies |
