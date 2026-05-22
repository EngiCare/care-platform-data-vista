# Care Platform Data VistA

A Python FastAPI service that provides REST API access to VA VistA clinical data via the XWB broker protocol. Covers the full breadth of CPRS (Computerized Patient Record System) functionality: patient records, clinical notes, orders, medications, labs, vitals, problems, allergies, consults, reminders, reports, and more.

**License:** MIT — Copyright (c) 2026 Engineered Care, Inc.

---

## Prerequisites

- **Python 3.11+** (3.12 recommended)
- **A VistA RPC server** listening on XWB broker protocol (e.g., [care-platform-core-mock-vista](https://engicare.visualstudio.com/Care%20Platform%20CORE/_git/care-platform-core-mock-vista) for local development)

## Quick Start

### 1. Install dependencies

```bash
# Create a virtual environment (recommended)
python -m venv .venv

# Activate it
# Windows:
.venv\Scripts\activate
# macOS/Linux:
source .venv/bin/activate

# Install
pip install -e ".[dev]"
```

### 2. Configure site connections

Edit `resources/xml/LocalVEHUSites.xml` to point at your VistA instance:

```xml
<VhaSite name="VistA RPC Server (Test)" ID="128" moniker="TST">
  <DataSource modality="HIS" type="VISTA" protocol="VISTA"
              source="127.0.0.1" status="active" port="9200" />
</VhaSite>
```

- `source` — hostname or IP of the VistA RPC broker
- `port` — XWB broker port (typically 9200 for dev, 9430 for production)
- `ID` — the Site ID used in login requests

### 3. Configure application settings

`appsettings.json` controls logging, session management, and JWT signing:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "SiteConfig": {
    "FilePath": "resources/xml/LocalVEHUSites.xml"
  },
  "TokenServiceConfig": {
    "CertificateName": "CN=CarePlatform",
    "IssuerName": "CarePlatform",
    "Audience": "urn:careplatform"
  }
}
```

For production, set `CertificateFileName` and `CertificatePassword` to use a real certificate for JWT signing. In development, the service auto-generates an RSA key.

### 4. Launch the server

```bash
python -m uvicorn app.main:app --host 0.0.0.0 --port 5200
```

The API is now available at `http://localhost:5200`. View interactive docs at `http://localhost:5200/docs`.

### 5. Verify it's running

```bash
curl http://localhost:5200/api/connection/sites
```

Expected output:
```json
[{"site_id": "128", "name": "VistA RPC Server (Test)", "visn_name": "Care Platform"}]
```

---

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `CPRS_SITE_CONFIG` | Override path to sites XML | `resources/xml/LocalVEHUSites.xml` |
| `CPRS_LOG_LEVEL` | Logging level | `DEBUG` |
| `CPRS_JWT_ISSUER` | JWT issuer claim | `CarePlatform` |
| `CPRS_JWT_AUDIENCE` | JWT audience claim | `urn:careplatform` |

---

## API Overview

### Authentication Flow

1. **Get available sites:** `GET /api/connection/sites`
2. **Login:** `GET /api/connectbysite?SiteId=128&AccessCode=...&VerifyCode=...` → returns JWT
3. **Use JWT:** `Authorization: Bearer <token>` on all subsequent requests
4. **Keep alive:** `GET /api/session/pulse`
5. **Disconnect:** `POST /api/session/disconnect`

### Key Endpoints

| Domain | Example Endpoint | Description |
|--------|-----------------|-------------|
| Patients | `GET /api/patient/select?dfn=3` | Select patient, get demographics |
| Notes | `GET /api/note/list?dfn=3&context=1` | List clinical notes |
| Orders | `GET /api/order/list?dfn=3&filterTS=2&dGroup=ALL` | List active orders |
| Meds | `GET /api/medication/active?dfn=3&duz=83` | Active medications |
| Problems | `GET /api/problem/list?dfn=3&status=A` | Active problem list |
| Labs | `GET /api/lab/recent?dfn=3` | Recent lab results |
| Vitals | `GET /api/vital/latest?dfn=3` | Latest vital signs |
| Allergies | `GET /api/coversheet/allergies?dfn=3` | Allergy list |
| Consults | `GET /api/consult/list?dfn=3&early=...&late=...` | Consult requests |
| Reminders | `GET /api/reminder/applicable?dfn=3` | Clinical reminders |
| Reports | `GET /api/report/lists` | Available report types |
| Alerts | `GET /api/alert/list` | User notifications |

See [AI_CONTEXT.md](AI_CONTEXT.md) for comprehensive endpoint documentation, or visit `/docs` for the auto-generated OpenAPI spec.

### Response Formats

- **Structured JSON** — Patient demographics, orders, problems, notes, consults (Pydantic models with `snake_case` fields)
- **Raw strings** — Note text, lab reports, order details (VistA wire format passed through as `string` or `array of string`)

---

## Example Scripts

The `examples/` directory contains ready-to-run Python scripts demonstrating common workflows:

```bash
# Login and get user info
python examples/login.py

# Search for a patient and read their notes
python examples/read_notes.py

# Get a patient's active orders, problems, and vitals
python examples/patient_summary.py
```

See the [examples/](examples/) directory for details. Each script is self-contained and only requires the `requests` library (`pip install requests`).

---

## AI Integration

Two files are provided for AI/LLM tool integration:

- **[AI_CONTEXT.md](AI_CONTEXT.md)** — Human-readable context guide with auth flow, workflows, domain glossary, and model schemas. Suitable for LLM system prompts or RAG retrieval.
- **[ai_tools.json](ai_tools.json)** — Structured JSON tool definitions (50+ tools) for OpenAI function calling, LangChain, MCP, or similar frameworks.

---

## Project Structure

```
app/
├── main.py                 # FastAPI application entry point
├── config.py               # Site configuration loader
├── dependencies.py         # Auth dependency (JWT verification)
├── filemandate.py          # FileMan date utilities
├── models/                 # Pydantic response models
│   ├── patient.py          #   Patient demographics, search
│   ├── user.py             #   User info
│   ├── order.py            #   Orders
│   ├── consult.py          #   Consults
│   ├── problem.py          #   Problems
│   ├── tiu_document.py     #   Clinical notes (TIU)
│   ├── report.py           #   Report definitions
│   └── ...
├── platform/
│   ├── vista/              # XWB broker client
│   │   ├── vista_connection.py  # TCP connection management
│   │   ├── vista_query.py       # RPC call builder
│   │   └── ...
│   ├── session/            # Session management
│   └── security/           # JWT signing/verification
└── routers/                # 37 API routers (one per clinical domain)
    ├── connection.py       #   Login/auth
    ├── patient.py          #   Patient search/select
    ├── note.py             #   Clinical notes
    ├── order.py            #   Orders
    └── ...
```

---

## Running Tests

```bash
pytest
```

---

## Development

```bash
# Install with dev dependencies
pip install -e ".[dev]"

# Run with auto-reload
python -m uvicorn app.main:app --host 0.0.0.0 --port 5200 --reload
```
