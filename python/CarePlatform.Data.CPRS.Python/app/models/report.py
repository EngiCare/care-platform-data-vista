# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""Report models — mirrors ReportModels.cs.

ReportDefinition with parse/tree-building from ORWRP3 EXPAND COLUMNS.
"""

from __future__ import annotations

from pydantic import BaseModel, Field


class ReportDefinition(BaseModel):
    id: str = ""
    name: str = ""
    qualifier: str = ""
    hs_tag: str = ""
    remote: str = ""
    rpt_type: str = ""
    category: str = ""
    rpc_name: str = ""
    ifn: str = ""
    sort_order: str = ""
    max_days_back: str = ""
    direct: str = ""
    hdr: str = ""
    fhie: str = ""
    fhie_only: str = ""
    supports_date_range: bool = False
    supports_remote_data: bool = False
    qualifier_type: int = 0
    children: list[ReportDefinition] = Field(default_factory=list)

    @staticmethod
    def parse_line(line: str) -> ReportDefinition:
        p = line.split("^")

        def _g(idx: int) -> str:
            return p[idx] if idx < len(p) else ""

        remote = _g(6)
        qualifier = _g(2)
        return ReportDefinition(
            id=_g(0),
            name=_g(1),
            qualifier=qualifier,
            hs_tag=_g(3),
            remote=remote,
            rpt_type=_g(7),
            category=_g(8),
            rpc_name=_g(9),
            ifn=_g(10),
            sort_order=_g(11),
            max_days_back=_g(12),
            direct=_g(13),
            hdr=_g(14),
            fhie=_g(15),
            fhie_only=_g(16),
            supports_remote_data=remote in ("1", "2"),
            supports_date_range=len(qualifier) > 0 and qualifier[0] in ("T", "d"),
        )

    @staticmethod
    def parse_tree_from_expand_columns(all_lines: list[str]) -> list[ReportDefinition]:
        # Extract [REPORT LIST] section
        lines: list[str] = []
        in_section = False
        for line in all_lines:
            if line == "[REPORT LIST]":
                in_section = True
                continue
            if line == "$$END":
                if in_section:
                    break
                continue
            if in_section and line.strip():
                lines.append(line)

        roots: list[ReportDefinition] = []
        parent_stack: list[ReportDefinition] = []

        for line in lines:
            piece0 = line.split("^")[0].upper()

            if piece0 == "[PARENT END]":
                if parent_stack:
                    parent_stack.pop()
                continue

            if piece0 == "[PARENT START]":
                parts = line.split("^")
                def_line = "^".join(parts[1:])
                node = ReportDefinition.parse_line(def_line)
                if parent_stack:
                    parent_stack[-1].children.append(node)
                else:
                    roots.append(node)
                parent_stack.append(node)
                continue

            report = ReportDefinition.parse_line(line)
            if parent_stack:
                parent_stack[-1].children.append(report)
            else:
                roots.append(report)

        return roots
