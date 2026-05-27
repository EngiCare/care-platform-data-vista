# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""FastAPI application entry point — mirrors Program.cs."""

from __future__ import annotations

import logging
import os
from contextlib import asynccontextmanager
from pathlib import Path

from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse

from app import config as app_config
from app.config import AppSettings
from app.platform.security.token_config import TokenServiceConfiguration
from app.platform.session.factory import get_default_session_manager
from app.platform.session.manager import SessionManager
from app.routers import (
    alert,
    allergy,
    connection,
    consult,
    coversheet,
    cwad,
    dcsumm,
    encounter,
    event_capture,
    facility,
    graph,
    lab,
    medication,
    note,
    options,
    order,
    order_allergy,
    order_billing,
    order_dialog,
    order_diet,
    order_lab,
    order_med,
    order_rad,
    oth,
    patient,
    patientlist,
    problem,
    reminder,
    report,
    session,
    surgery,
    template,
    user,
    utility,
    vitals,
    womens_health,
)

logger = logging.getLogger(__name__)


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Startup / shutdown lifecycle."""
    # --- Startup ---
    logging.basicConfig(
        level=logging.DEBUG,
        format="%(asctime)s %(levelname)-5s [%(name)s] %(message)s",
    )
    logger.info("CarePlatform.Data.CPRS (Python) starting")

    # Load settings
    base_dir = Path(__file__).resolve().parent.parent
    appsettings_path = base_dir / "appsettings.json"
    if appsettings_path.exists():
        settings = AppSettings.from_json(appsettings_path)
    else:
        settings = AppSettings()
    app_config.set_settings(settings)

    # Token service config
    TokenServiceConfiguration.load(settings.token_service)

    # Session manager
    mgr = get_default_session_manager(settings.session_manager.session_manager_object)
    app_config.set_session_manager(mgr)

    # Start reaper task if real SessionManager
    if isinstance(mgr, SessionManager):
        mgr.start_reaper()

    # Load site config
    site_path = settings.site_config_file_path
    if not os.path.isabs(site_path):
        site_path = str(base_dir / site_path)
    if os.path.exists(site_path):
        app_config.load_site_config(site_path)
    else:
        logger.warning("Site config not found: %s", site_path)

    logger.info("CarePlatform.Data.CPRS (Python) started")
    yield
    # --- Shutdown ---
    logger.info("CarePlatform.Data.CPRS (Python) shutting down")


app = FastAPI(
    title="CarePlatform Data CPRS API",
    version="1.0.0",
    lifespan=lifespan,
)

# Exception handlers — mirror ECWebApiExceptionFilterAttribute
@app.exception_handler(PermissionError)
async def unauthorized_handler(request: Request, exc: PermissionError):
    return JSONResponse(status_code=401, content="Please log in again.")


@app.exception_handler(Exception)
async def generic_error_handler(request: Request, exc: Exception):
    logger.error("Unhandled exception: %s", exc, exc_info=True)
    return JSONResponse(
        status_code=500,
        content="An error occurred, please try again or contact the administrator.",
    )


# Routers
app.include_router(connection.router)
app.include_router(session.router)
app.include_router(alert.router)
app.include_router(allergy.router)
app.include_router(consult.router)
app.include_router(coversheet.router)
app.include_router(cwad.router)
app.include_router(dcsumm.router)
app.include_router(encounter.router)
app.include_router(event_capture.router)
app.include_router(facility.router)
app.include_router(graph.router)
app.include_router(lab.router)
app.include_router(medication.router)
app.include_router(note.router)
app.include_router(options.router)
app.include_router(order.router)
app.include_router(order_allergy.router)
app.include_router(order_billing.router)
app.include_router(order_dialog.router)
app.include_router(order_diet.router)
app.include_router(order_lab.router)
app.include_router(order_med.router)
app.include_router(order_rad.router)
app.include_router(oth.router)
app.include_router(patient.router)
app.include_router(patientlist.router)
app.include_router(problem.router)
app.include_router(reminder.router)
app.include_router(report.router)
app.include_router(surgery.router)
app.include_router(template.router)
app.include_router(user.router)
app.include_router(utility.router)
app.include_router(vitals.router)
app.include_router(womens_health.router)


# Health check
@app.get("/health")
async def health():
    return {"status": "healthy", "service": "Data.CPRS API (Python)"}
