# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""String / pack helpers — mirrors Utils.cs and StringUtils.cs."""

from __future__ import annotations


def str_pack(s: str, n: int) -> str:
    """Length-prefixed string — LPack(s, n) in C#.

    The length (in characters) is zero-padded to *n* digits and prepended.
    """
    return str(len(s)).zfill(n) + s


def s_pack(s: str) -> str:
    """Single-byte length-prefix — SPack(s) in C#.

    First byte is chr(len(s)) (max 255).
    """
    length = len(s)
    if length > 255:
        raise ValueError(f"SPack: string too long ({length})")
    return chr(length) + s


def piece(s: str, delimiter: str, piece_num: int) -> str:
    """1-based field extraction — mirrors Utils.piece()."""
    parts = s.split(delimiter)
    idx = piece_num - 1
    if 0 <= idx < len(parts):
        return parts[idx]
    return ""
