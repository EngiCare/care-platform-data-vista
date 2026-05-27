# Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
"""VistA query builder — mirrors VistaQuery.cs.

Constructs XWB protocol envelopes for RPC calls to VistA.
"""

from __future__ import annotations

import random
from typing import Any

from app.platform.vista.dict_hash_list import DictionaryHashList
from app.platform.vista.utils import str_pack, s_pack

# Parameter types
LITERAL = 1
REFERENCE = 2
LIST = 3
GLOBAL = 4
EMPTY = 5
STREAM = 6
UNDEFINED = 7

# Protocol constants — match desktop CPRSChart.exe (fFrame.pas line 938)
PREFIX = "[XWB]"
COUNT_WIDTH = 4
RPC_VERSION = "0"

# XWB cipher pad — 20 entries, exact copy from VistaQuery.Parameter.encrypt()
_CIPHER_PAD = [
    "wkEo-ZJt!dG)49K{nX1BS$vH<&:Myf*>Ae0jQW=;|#PsO`'%+rmb[gpqN,l6/hFC@DcUa ]z~R}\"V\\iIxu?872.(TYL5_3",
    "rKv`R;M/9BqAF%&tSs#Vh)dO1DZP> *fX'u[.4lY=-mg_ci802N7LTG<]!CWo:3?{+,5Q}(@jaExn$~p\\IyHwzU\"|k6Jeb",
    "\\pV(ZJk\"WQmCn!Y,y@1d+~8s?[lNMxgHEt=uw|X:qSLjAI*}6zoF{T3#;ca)/h5%`P4$r]G'9e2if_>UDKb7<v0&- RBO.",
    "depjt3g4W)qD0V~NJar\\B \"?OYhcu[<Ms%Z`RIL_6:]AX-zG.#}$@vk7/5x&*m;(yb2Fn+l'PwUof1K{9,|EQi>H=CT8S!",
    "NZW:1}K$byP;jk)7'`x90B|cq@iSsEnu,(l-hf.&Y_?J#R]+voQXU8mrV[!p4tg~OMez CAaGFD6H53%L/dT2<*>\"{\\wI=",
    "vCiJ<oZ9|phXVNn)m K`t/SI%]A5qOWe\\&?;jT~M!fz1l>[D_0xR32c*4.P\"G{r7}E8wUgyudF+6-:B=$(sY,LkbHa#'@Q",
    "hvMX,'4Ty;[a8/{6l~F_V\"}qLI\\!@x(D7bRmUH]W15J%N0BYPkrs&9:$)Zj>u|zwQ=ieC-oGA.#?tfdcO3gp`S+En K2*<",
    "jd!W5[];4'<C$/&x|rZ(k{>?ghBzIFN}fAK\"#`p_TqtD*1E37XGVs@0nmSe+Y6Qyo-aUu%i8c=H2vJ\\) R:MLb.9,wlO~P",
    "2ThtjEM+!=xXb)7,ZV{*ci3\"8@_l-HS69L>]\\AUF/Q%:qD?1~m(yvO0e'<#o$p4dnIzKP|`NrkaGg.ufCRB[; sJYwW}5&",
    "vB\\5/zl-9y:Pj|=(R'7QJI *&CTX\"p0]_3.idcuOefVU#omwNZ`$Fs?L+1Sk<,b)hM4A6[Y%aDrg@~KqEW8t>H};n!2xG{",
    "sFz0Bo@_HfnK>LR}qWXV+D6`Y28=4Cm~G/7-5A\\b9!a#rP.l&M$hc3ijQk;),TvUd<[:I\"u1'NZSOw]*gxtE{eJp|y (?%",
    "M@,D}|LJyGO8`$*ZqH .j>c~h<d=fimszv[#-53F!+a;NC'6T91IV?(0x&/{B)w\"]Q\\YUWprk4:ol%g2nE7teRKbAPuS_X",
    ".mjY#_0*H<B=Q+FML6]s;r2:e8R}[ic&KA 1w{)vV5d,$u\"~xD/Pg?IyfthO@CzWp%!`N4Z'3-(o|J9XUE7k\\TlqSb>anG",
    "xVa1']_GU<X`|\\NgM?LS9{\"jT%s$}y[nvtlefB2RKJW~(/cIDCPow4,>#zm+:5b@06O3Ap8=*7ZFY!H-uEQk; .q)i&rhd",
    "I]Jz7AG@QX.\"%3Lq>METUo{Pp_ |a6<0dYVSv8:b)~W9NK`(r'4fs&wim\\kReC2hg=HOj$1B*/nxt,;c#y+![?lFuZ-5D}",
    "Rr(Ge6F Hx>q$m&C%M~Tn,:\"o'tX/*yP.{lZ!YkiVhuw_<KE5a[;}W0gjsz3]@7cI2\\QN?f#4p|vb1OUBD9)=-LJA+d`S8",
    "I~k>y|m};d)-7DZ\"Fe/Y<B:xwojR,Vh]O0Sc[`$sg8GXE!1&Qrzp._W%TNK(=J 3i*2abuHA4C'?Mv\\Pq{n#56LftUl@9+",
    "~A*>9 WidFN,1KsmwQ)GJM{I4:C%}#Ep(?HB/r;t.&U8o|l['Lg\"2hRDyZ5`nbf]qjc0!zS-TkYO<_=76a\\X@$Pe3+xVvu",
    "yYgjf\"5VdHc#uA,W1i+v'6|@pr{n;DJ!8(btPGaQM.LT3oe?NB/&9>Z`-}02*%x<7lsqz4OS ~E$\\R]KI[:UwC_=h)kXmF",
    "5:iar.{YU7mBZR@-K|2 \"+~`M%8sq4JhPo<_X\\Sg3WC;Tuxz,fvEQ1p9=w}FAI&j/keD0c?)LN6OHV]lGy'$*>nd[(tb!#",
]
_MAXKEY = 19


def encrypt(in_string: str) -> str:
    """Encrypt a string using the VistA XWB cipher pad (login credentials)."""
    associator = random.randint(0, _MAXKEY)
    identifier = random.randint(0, _MAXKEY)
    while associator == identifier:
        identifier = random.randint(0, _MAXKEY)

    xlated = []
    for ch in in_string:
        pos = _CIPHER_PAD[associator].find(ch)
        if pos == -1:
            xlated.append(ch)
        else:
            xlated.append(_CIPHER_PAD[identifier][pos])

    return chr(associator + 32) + "".join(xlated) + chr(identifier + 32)


def _lpack(s: str | None, ndigits: int) -> str:
    """LPack — length-prefixed with zero-padded digit count."""
    length = 0 if not s else len(s)
    s_len = str(length)
    if ndigits < len(s_len):
        raise ValueError("Too few digits")
    padded = ("0" * 9 + s_len)[-ndigits:]
    return padded + (s or "")


class Parameter:
    """A single RPC parameter."""

    __slots__ = ("type", "value", "text", "list", "encrypted", "original")

    def __init__(
        self,
        ptype: int,
        value: str = "",
        *,
        text: str = "",
        lst: DictionaryHashList | None = None,
        do_encrypt: bool = False,
    ):
        self.type = ptype
        self.text = text
        self.list = lst
        self.encrypted = do_encrypt
        self.original = value
        if do_encrypt:
            self.value = encrypt(value)
        else:
            self.value = value
        if lst is not None:
            self.value = ".x"


class VistaQuery:
    """Build an XWB protocol message for a VistA RPC call."""

    # Re-export parameter type constants on the class for convenience
    LITERAL = LITERAL
    REFERENCE = REFERENCE
    LIST = LIST
    GLOBAL = GLOBAL
    EMPTY = EMPTY
    STREAM = STREAM

    def __init__(self, rpc_name: str = ""):
        self.rpc_name = rpc_name
        self.parameters: list[Parameter] = []

    def add_parameter(self, ptype: int, value: Any = "", *, text: str = "") -> None:
        if isinstance(value, DictionaryHashList):
            self.parameters.append(Parameter(ptype, "", lst=value))
        else:
            self.parameters.append(Parameter(ptype, str(value), text=text))

    def add_encrypted_parameter(self, ptype: int, value: str) -> None:
        self.parameters.append(Parameter(ptype, value, do_encrypt=True))

    def build_message(self) -> str:
        s_params = "5"  # Don't ask why — from the broker code

        for vp in self.parameters:
            if vp.type == LITERAL:
                s_params += "0" + _lpack(vp.value, COUNT_WIDTH) + "f"
            elif vp.type == REFERENCE:
                s_params += "1" + _lpack(vp.value, COUNT_WIDTH) + "f"
            elif vp.type == EMPTY:
                s_params += "4f"
            elif vp.type == LIST:
                s_params += "2" + self._list2string(vp.list, COUNT_WIDTH)
            elif vp.type == GLOBAL:
                s_params += "3" + self._list2string(vp.list, COUNT_WIDTH)
            elif vp.type == STREAM:
                s_params += "5" + _lpack(vp.value, COUNT_WIDTH) + "f"

        # If no params were added, add an empty one
        if s_params == "5":
            s_params += "4f"

        msg = (
            PREFIX
            + "11"
            + str(COUNT_WIDTH)
            + "02"
            + s_pack(RPC_VERSION)
            + s_pack(self.rpc_name)
            + s_params
            + "\x04"
        )
        return msg

    @staticmethod
    def _list2string(lst: DictionaryHashList | None, count_width: int = 4) -> str:
        if lst is None or lst.count == 0:
            return _lpack("", count_width) + "f"

        result = ""
        for i, (key, value) in enumerate(lst):
            v = value if value else "\u0001"
            result += _lpack(key, count_width) + _lpack(v, count_width) + "t"
        # Replace last 't' with 'f'
        result = result[:-1] + "f"
        return result

    @staticmethod
    def adjust_for_name_search(target: str) -> str:
        if not target:
            return ""
        rtn = target[:-1]
        c = chr(ord(target[-1]) - 1)
        return rtn + c + "~"

    @staticmethod
    def adjust_for_numeric_search(target: str) -> str:
        return str(int(target) - 1)
