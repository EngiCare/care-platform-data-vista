# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""User models — mirrors UserModels.cs."""

from __future__ import annotations

from pydantic import BaseModel

from app.models.vista_string_parser import parse_bool, parse_int, split


class UserInfo(BaseModel):
    duz: int = 0
    name: str = ""
    user_class: str = ""
    can_sign: bool = False
    is_provider: bool = False
    order_role: str = ""
    no_order_entry: bool = False
    d_time: int = 300
    count_down: int = 10
    verify_orders: bool = False
    notify_apps: int = 0
    message_hang: int = 0
    domain: str = ""
    service: str = ""
    auto_save_interval: int = 180
    initial_tab: int = 1
    last_tab: int = 0
    web_access: bool = False
    allow_hold: bool = False
    is_rpl: bool = False
    rpl_list: str = ""
    cor_tabs: str = ""
    rpt_tab: str = ""
    station_number: str = ""
    gec_status: str = ""
    is_production: bool = False
    enable_action_one_step: bool = False

    @classmethod
    def parse(cls, vista_string: str) -> "UserInfo":
        p = split(vista_string)
        return cls(
            duz=parse_int(p[0]) if len(p) > 0 else 0,
            name=p[1] if len(p) > 1 else "",
            user_class=p[2] if len(p) > 2 else "",
            can_sign=parse_bool(p[3]) if len(p) > 3 else False,
            is_provider=parse_bool(p[4]) if len(p) > 4 else False,
            order_role=p[5] if len(p) > 5 else "",
            no_order_entry=parse_bool(p[6]) if len(p) > 6 else False,
            d_time=parse_int(p[7], 300) if len(p) > 7 else 300,
            count_down=parse_int(p[8], 10) if len(p) > 8 else 10,
            verify_orders=parse_bool(p[9]) if len(p) > 9 else False,
            notify_apps=parse_int(p[10]) if len(p) > 10 else 0,
            message_hang=parse_int(p[11]) if len(p) > 11 else 0,
            domain=p[12] if len(p) > 12 else "",
            service=p[13] if len(p) > 13 else "",
            auto_save_interval=parse_int(p[14], 180) if len(p) > 14 else 180,
            initial_tab=parse_int(p[15], 1) if len(p) > 15 else 1,
            last_tab=parse_int(p[16]) if len(p) > 16 else 0,
            web_access=parse_bool(p[17]) if len(p) > 17 else False,
            allow_hold=parse_bool(p[18]) if len(p) > 18 else False,
            is_rpl=parse_bool(p[19]) if len(p) > 19 else False,
            rpl_list=p[20] if len(p) > 20 else "",
            cor_tabs=p[21] if len(p) > 21 else "",
            rpt_tab=p[22] if len(p) > 22 else "",
            station_number=p[23] if len(p) > 23 else "",
            gec_status=p[24] if len(p) > 24 else "",
            is_production=parse_bool(p[25]) if len(p) > 25 else False,
            enable_action_one_step=parse_bool(p[26]) if len(p) > 26 else False,
        )
