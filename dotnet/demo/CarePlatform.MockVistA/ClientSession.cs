// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System.Net.Sockets;
using System.Text;

namespace CarePlatform.MockVistA;

/// <summary>
/// Handles one broker client connection — reads XWB-framed requests,
/// dispatches to hardcoded RPC handlers, and writes XWB-framed responses.
/// </summary>
internal sealed class ClientSession(NetworkStream stream, string endpoint)
{
    private const byte EOT = 0x04;

    // ── Hardcoded test user ──────────────────────────────────────────────
    private const string AccessCode = "cprs";
    private const string VerifyCode = "cprs1234";
    private const string Duz = "10000000237";
    private const string UserName = "PROVIDER,WILLIAM MD";
    private const string StationNumber = "500";
    private const string StationName = "DEMO MEDICAL CENTER";
    private const string Title = "PHYSICIAN";
    private const string Service = "MEDICINE";

    /// <summary>
    /// XUS GET USER INFO response — CRLF-delimited fields:
    ///   [0] DUZ, [1] Name, [2] (empty), [3] Station^Name^Port, [4] Title, [5] Service, [6] Language, [7] DTIME
    /// </summary>
    private static readonly string UserInfoString =
        $"{Duz}\r\n{UserName}\r\n\r\n{StationNumber}^{StationName}^127\r\n{Title}\r\n{Service}\r\nENGLISH\r\n300\r\n";

    /// <summary>
    /// ORWU USERINFO response — caret-delimited 27-piece string:
    ///   DUZ^NAME^USRCLS^CANSIGN^ISPROVIDER^ORDERROLE^NOORDER^DTIME^CNTDN^VERORD^
    ///   NOTIFYAPPS^MSGHANG^DOMAIN^SERVICE^AUTOSAVE^INITTAB^LASTTAB^WEBACCESS^
    ///   ALLOWHOLD^ISRPL^RPLLIST^CORTABS^RPTTAB^STATION#^GECStatus^Production^EnableActOneStep
    /// </summary>
    private static readonly string OrwuUserInfoString =
        $"{Duz}^{UserName}^1^1^1^1^0^300^10^0^0^0^DEMO.DOMAIN.GOV^{Service}^180^1^1^0^0^0^^1^1^{StationNumber}^0^0^0";

    public async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var data = await ReadMessageAsync(ct);
            if (data == null) break;

            var raw = Encoding.ASCII.GetString(data);

            // ── Old-style messages: TCPConnect and #BYE# ────────────
            if (raw.Contains("#BYE#"))
            {
                await WriteSuccessAsync("GOODBYE", ct);
                break;
            }

            if (raw.Contains("TCPConnect"))
            {
                await WriteSuccessAsync("accept", ct);
                continue;
            }

            // ── New-style RPC messages ──────────────────────────────
            var rpcName = ParseRpcName(data);
            if (rpcName == null)
            {
                await WriteSuccessAsync("", ct);
                continue;
            }

            Log($"RPC: {rpcName}");
            var response = HandleRpc(rpcName, data);
            await WriteSuccessAsync(response, ct);
        }
    }

    private string HandleRpc(string rpcName, byte[] data) => rpcName switch
    {
        "XUS INTRO MSG" => $"Welcome to Mock VistA\r\n{StationName}\r\nDemo System",

        "XUS SIGNON SETUP" => string.Join("\r\n", [
            StationName, "VOL", "UCI", "5", "0",
            "DEMO.DOMAIN.GOV", "0", ""
        ]),

        "XUS AV CODE" => HandleAvCode(data),

        "XUS DIVISION GET" => string.Join("\r\n", [
            "1",
            $"1^{StationName}^{StationNumber}^1"
        ]),

        "XUS DIVISION SET" => "1",

        "XUS GET USER INFO" => UserInfoString,

        "XWB CREATE CONTEXT" => "1",

        "ORWU USERINFO" => OrwuUserInfoString,

        "XWB IM HERE" => "1",

        "XWB GET BROKER INFO" => "300",

        _ => UnhandledRpc(rpcName),
    };

    private string HandleAvCode(byte[] data)
    {
        // Extract the encrypted parameter from the new-style message
        var encrypted = ExtractFirstLiteralParam(data);
        var decrypted = CipherDecrypt(encrypted);

        var parts = decrypted.Split(';', 2);
        var ac = parts.Length > 0 ? parts[0] : "";
        var vc = parts.Length > 1 ? parts[1] : "";

        if (ac.Equals(AccessCode, StringComparison.OrdinalIgnoreCase) &&
            vc.Equals(VerifyCode, StringComparison.OrdinalIgnoreCase))
        {
            Log("Login SUCCESS");
            // Success: DUZ on line 0, lines 1-6 status, line 7 greeting
            return string.Join("\r\n", [
                Duz, "0", "0", "", "0", "0", "0",
                $"Good morning, {UserName}"
            ]);
        }

        Log("Login FAILED");
        // Failure: "0" on line 0, error on line 3
        return string.Join("\r\n", [
            "0", "0", "0",
            "Not a valid ACCESS CODE/VERIFY CODE pair.",
            "0", "0", "0", ""
        ]);
    }

    private string UnhandledRpc(string rpcName)
    {
        Console.WriteLine($"  [{endpoint}] UNHANDLED RPC: {rpcName} — returning empty");
        return "";
    }

    // ── XWB Protocol Helpers ────────────────────────────────────────────

    /// <summary>Read bytes from the stream until EOT (0x04).</summary>
    private async Task<byte[]?> ReadMessageAsync(CancellationToken ct)
    {
        using var ms = new MemoryStream();
        var buffer = new byte[4096];

        while (!ct.IsCancellationRequested)
        {
            int n;
            try { n = await stream.ReadAsync(buffer, ct); }
            catch { return null; }

            if (n == 0) return null;

            for (int i = 0; i < n; i++)
            {
                if (buffer[i] == EOT)
                {
                    ms.Write(buffer, 0, i);
                    return ms.ToArray();
                }
            }
            ms.Write(buffer, 0, n);
        }
        return null;
    }

    /// <summary>Write a success response: 0x00 0x00 {data} 0x04.</summary>
    private async Task WriteSuccessAsync(string data, CancellationToken ct)
    {
        var payload = Encoding.ASCII.GetBytes(data);
        var frame = new byte[2 + payload.Length + 1];
        // frame[0] = 0x00, frame[1] = 0x00 already (success)
        Array.Copy(payload, 0, frame, 2, payload.Length);
        frame[^1] = EOT;
        await stream.WriteAsync(frame, ct);
        await stream.FlushAsync(ct);
    }

    /// <summary>
    /// Parse the RPC name from a new-style message.
    /// Format: [XWB]11{countWidth}02{SPack(version)}{SPack(rpcName)}...
    /// SPack = 1-byte length + string.
    /// </summary>
    private static string? ParseRpcName(byte[] data)
    {
        // Minimum: [XWB]11X02 = 10 bytes header + version SPack + name SPack
        if (data.Length < 12) return null;

        var prefix = Encoding.ASCII.GetString(data, 0, 5);
        if (prefix != "[XWB]") return null;
        if (data[5] != (byte)'1' || data[6] != (byte)'1') return null;

        // offset 10: SPack(version)
        int offset = 10;
        if (offset >= data.Length) return null;
        int verLen = data[offset]; offset++;
        offset += verLen; // skip version string

        // SPack(rpcName)
        if (offset >= data.Length) return null;
        int nameLen = data[offset]; offset++;
        if (offset + nameLen > data.Length) return null;
        return Encoding.ASCII.GetString(data, offset, nameLen);
    }

    /// <summary>
    /// Extract the first LITERAL parameter value from a new-style RPC message.
    /// After the RPC name, params section: '5' + type('0') + LPack(value, countWidth) + 'f'
    /// </summary>
    private static string ExtractFirstLiteralParam(byte[] data)
    {
        // Find params section: skip header (10) + SPack(ver) + SPack(name)
        int offset = 10;
        if (offset >= data.Length) return "";
        int verLen = data[offset]; offset++; offset += verLen;
        if (offset >= data.Length) return "";
        int nameLen = data[offset]; offset++; offset += nameLen;

        // Expect '5' sentinel
        if (offset >= data.Length || data[offset] != 0x35) return "";
        offset++;

        // Expect '0' (LITERAL type)
        if (offset >= data.Length || data[offset] != (byte)'0') return "";
        offset++;

        // countWidth is at data[7]
        int countWidth = data[7] - (byte)'0';
        if (countWidth < 1 || countWidth > 9) countWidth = 4;

        // LPack: countWidth-digit length + value
        if (offset + countWidth > data.Length) return "";
        var lenStr = Encoding.ASCII.GetString(data, offset, countWidth);
        if (!int.TryParse(lenStr, out int len)) return "";
        offset += countWidth;

        if (offset + len > data.Length) return "";
        return Encoding.ASCII.GetString(data, offset, len);
    }

    // ── VistA Cipher Pad ────────────────────────────────────────────────

    private static readonly string[] CipherPad =
    [
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
    ];

    /// <summary>Decrypt a VistA cipher-pad encrypted string.</summary>
    private static string CipherDecrypt(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted) || encrypted.Length < 2)
            return encrypted;

        var assocIdx = encrypted[0] - 32;
        var identIdx = encrypted[^1] - 32;

        if (assocIdx < 0 || assocIdx > 19 || identIdx < 0 || identIdx > 19)
            return encrypted;

        var assocPad = CipherPad[assocIdx];
        var identPad = CipherPad[identIdx];

        var sb = new StringBuilder(encrypted.Length - 2);
        for (int i = 1; i < encrypted.Length - 1; i++)
        {
            var pos = identPad.IndexOf(encrypted[i]);
            sb.Append(pos >= 0 && pos < assocPad.Length ? assocPad[pos] : encrypted[i]);
        }
        return sb.ToString();
    }

    private void Log(string msg) => Console.WriteLine($"  [{endpoint}] {msg}");
}
