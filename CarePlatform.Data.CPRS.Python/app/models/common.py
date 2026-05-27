# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Common API response models — mirrors ApiResponse.cs."""

from __future__ import annotations

from pydantic import BaseModel
from typing import Generic, Optional, TypeVar

T = TypeVar("T")


class ApiResponse(BaseModel):
    success: bool = False
    error_message: Optional[str] = None
    message: Optional[str] = None

    @classmethod
    def ok(cls, message: str | None = None) -> "ApiResponse":
        return cls(success=True, message=message)

    @classmethod
    def fail(cls, error: str) -> "ApiResponse":
        return cls(success=False, error_message=error)
