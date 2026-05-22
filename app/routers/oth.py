# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""OTH router — mirrors OTHController.cs.

Other Than Honorable discharge status — OTH eligibility clock for a patient.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Query

from app.dependencies import get_current_session
from app.platform.session.base import ISession
from app.platform.vista.vista_query import VistaQuery

router = APIRouter()


@router.get("/api/oth/status")
async def get_oth_status(
    dfn: str = Query(...),
    dateOfService: str = Query(...),
    session: ISession = Depends(get_current_session),
) -> list[str]:
    vq = VistaQuery("OROTHCL GET")
    vq.add_parameter(VistaQuery.LITERAL, dfn)
    vq.add_parameter(VistaQuery.LITERAL, dateOfService)
    return await session.t_query(vq)
