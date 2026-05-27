// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
//
// XWB protocol helpers (SPack, LPack, buildMessage, encrypt) are copied
// verbatim from the production broker client in
//   care-platform-core-vista-webservice / CarePlatform.Data.CPRS /
//   Platform / Vista / Models / {VistaQuery.cs, Utils.cs, VistaConnection.cs, VistaAccount.cs}
// so that the wire format exactly matches what desktop CPRSChart.exe sends.
using System.Net.Sockets;
using System.Text;

namespace CarePlatform.Web.Example.Services;

/// <summary>
/// Lightweight XWB broker client that speaks just enough of the VistA RPC
/// protocol to authenticate and retrieve user info.  Each call to
/// <see cref="LoginAsync"/> opens a fresh TCP connection, runs the login
/// sequence, fetches user info, and disconnects.
/// </summary>
public sealed class VistaRpcClient(ILogger<VistaRpcClient> logger)
{
    // ── Public API ──────────────────────────────────────────────────────

    public record LoginResult(bool Success, string? Error, UserInfo? User);

    public record UserInfo(
        string Duz,
        string Name,
        string Title,
        string ServiceSection,
        string Division,
        string Language,
        string Dtime);

    /// <summary>
    /// Connect to a VistA RPC broker, authenticate with the given
    /// access/verify codes, retrieve user info, and disconnect.
    /// </summary>
    public async Task<LoginResult> LoginAsync(
        string host, int port,
        string accessCode, string verifyCode,
        CancellationToken ct = default)
    {
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(host, port, ct);
        var stream = tcp.GetStream();

        // 1. TCPConnect handshake — copied from VistaConnection.connect()
        var myHostname = System.Net.Dns.GetHostName();
        var myIp = "127.0.0.1";
        string connectMsg = "[XWB]10304\nTCPConnect50"
            + strPack(myIp, 3)
            + "f0" + strPack("0", 3)
            + "f0" + strPack(myHostname, 3)
            + "f\x0004";
        var reply = await QueryAsync(stream, connectMsg, ct);
        if (reply != "accept")
            return new(false, $"TCPConnect rejected: {reply}", null);

        // 2. XUS INTRO MSG — hardcoded exactly as VistaConnection.connect() does
        string introMsg = "[XWB]11302\x00010\rXUS INTRO MSG54f\x0004";
        await QueryAsync(stream, introMsg, ct);

        // 3. XUS SIGNON SETUP
        reply = await QueryAsync(stream, BuildRpcMessage("XUS SIGNON SETUP"), ct);
        if (reply == null)
            return new(false, "Unable to setup authentication", null);

        // 4. XUS AV CODE (encrypted) — uses Parameter.encrypt() from VistaQuery.cs
        var encrypted = Encrypt(accessCode + ";" + verifyCode);
        reply = await QueryAsync(stream, BuildRpcMessage("XUS AV CODE", encrypted), ct);

        var flds = reply.Split(new[] { "\r\n" }, StringSplitOptions.None);
        if (flds[0] == "0")
        {
            var errorMsg = flds.Length > 3 ? flds[3] : "Invalid credentials";
            await SendByeAsync(stream, ct);
            return new(false, errorMsg, null);
        }
        var duz = flds[0];

        // 5. XWB CREATE CONTEXT (OR CPRS GUI CHART) — encrypted like VistaAccount.setContext()
        var ctxEncrypted = Encrypt("OR CPRS GUI CHART");
        await QueryAsync(stream, BuildRpcMessage("XWB CREATE CONTEXT", ctxEncrypted), ct);

        // 6. XUS GET USER INFO
        var userReply = await QueryAsync(stream, BuildRpcMessage("XUS GET USER INFO"), ct);
        var userLines = userReply.Split(new[] { "\r\n" }, StringSplitOptions.None);

        // Parse: [0]=DUZ [1]=Name [2]=(empty) [3]=Station^Name^Port [4]=Title [5]=Service [6]=Language [7]=DTIME
        var user = new UserInfo(
            Duz: duz,
            Name: userLines.Length > 1 ? userLines[1] : "",
            Title: userLines.Length > 4 ? userLines[4] : "",
            ServiceSection: userLines.Length > 5 ? userLines[5] : "",
            Division: userLines.Length > 3 ? userLines[3] : "",
            Language: userLines.Length > 6 ? userLines[6] : "",
            Dtime: userLines.Length > 7 ? userLines[7] : "");

        // 7. Disconnect
        await SendByeAsync(stream, ct);

        logger.LogInformation("Login succeeded for {Name} (DUZ={Duz})", user.Name, user.Duz);
        return new(true, null, user);
    }

    // ── Wire helpers — copied from Utils.cs ─────────────────────────────

    /// <summary>
    /// SPack: 1-byte length prefix (as raw char) + string.
    /// Copied from Utils.cs SPack().
    /// </summary>
    private static string SPack(string s)
    {
        return Convert.ToChar(s.Length) + s;
    }

    /// <summary>
    /// LPack: n-digit zero-padded ASCII length prefix + string.
    /// Copied from Utils.cs LPack().
    /// </summary>
    private static string LPack(string s, int ndigits)
    {
        int lth = string.IsNullOrEmpty(s) ? 0 : s.Length;
        string result = "000000000" + Convert.ToString(lth);
        result = result.Substring(result.Length - ndigits) + s;
        return result;
    }

    /// <summary>
    /// strPack: n-digit zero-padded length prefix + string.
    /// Copied from Utils.cs strPack().
    /// </summary>
    private static string strPack(string s, int n)
    {
        int lth = s.Length;
        StringBuilder result = new StringBuilder(lth.ToString());
        while (result.Length < n)
            result.Insert(0, "0");
        return result + s;
    }

    // ── buildMessage — copied from VistaQuery.buildMessage() ────────────

    /// <summary>
    /// Build a new-style RPC message with optional LITERAL parameters.
    /// Exact copy of VistaQuery.buildMessage() logic with COUNT_WIDTH=4
    /// to match desktop CPRSChart.exe (fFrame.pas line 938).
    /// </summary>
    private static string BuildRpcMessage(string rpcName, params string[] literalArgs)
    {
        const string PREFIX = "[XWB]";
        const int COUNT_WIDTH = 4; // matches desktop CPRSChart.exe
        const string RPC_VERSION = "0";

        string sParams = "5";
        foreach (var arg in literalArgs)
        {
            sParams += '0' + LPack(arg, COUNT_WIDTH) + 'f';
        }
        if (sParams == "5")
        {
            sParams += "4f";
        }

        return PREFIX + "11" + Convert.ToString(COUNT_WIDTH) + "02"
             + SPack(RPC_VERSION) + SPack(rpcName)
             + sParams + '\x0004';
    }

    // ── Send / Receive — copied from VistaConnection ────────────────────

    /// <summary>Send a pre-built message string and read the response.</summary>
    private static async Task<string> QueryAsync(NetworkStream stream, string message, CancellationToken ct)
    {
        // Send — copied from VistaConnection.Send(): ASCII encode the string
        var data = Encoding.ASCII.GetBytes(message);
        await stream.WriteAsync(data, ct);
        await stream.FlushAsync(ct);

        // Receive — copied from VistaConnection.ReceiveCallback(): read until EOT
        using var ms = new MemoryStream();
        var buf = new byte[4096];
        bool firstChunk = true;

        while (true)
        {
            var n = await stream.ReadAsync(buf, ct);
            if (n == 0) break;

            var thisBatch = Encoding.ASCII.GetString(buf, 0, n);
            var endIdx = thisBatch.IndexOf('\x04');

            if (firstChunk)
            {
                if (n == 0)
                    throw new ApplicationException("Timeout waiting for response from VistA");

                if (endIdx != -1)
                    thisBatch = thisBatch.Substring(0, endIdx);

                // Response header parsing — from VistaConnection.ReceiveCallback()
                // byte[0] != 0 means error with length in byte[0]
                // byte[1] != 0 means error
                // byte[0]==0 && byte[1]==0 means success, skip 2 bytes
                if (buf[0] != 0)
                {
                    thisBatch = thisBatch.Substring(1, buf[0]);
                    // error response
                }
                else if (buf[1] != 0)
                {
                    thisBatch = thisBatch.Substring(2);
                    // error response
                }
                else
                {
                    thisBatch = thisBatch.Substring(2);
                }

                ms.Write(Encoding.ASCII.GetBytes(thisBatch));
                firstChunk = false;

                if (endIdx != -1)
                    break;
            }
            else
            {
                if (endIdx != -1)
                {
                    ms.Write(Encoding.ASCII.GetBytes(thisBatch.Substring(0, endIdx)));
                    break;
                }
                ms.Write(Encoding.ASCII.GetBytes(thisBatch));
            }
        }

        return Encoding.ASCII.GetString(ms.ToArray());
    }

    /// <summary>Send #BYE# — old-style disconnect.</summary>
    private static async Task SendByeAsync(NetworkStream stream, CancellationToken ct)
    {
        var data = Encoding.ASCII.GetBytes("[XWB]10304\n#BYE#\n\x0004");
        await stream.WriteAsync(data, ct);
        await stream.FlushAsync(ct);
    }

    // ── Cipher — copied verbatim from VistaQuery.Parameter.encrypt() ────

    /// <summary>
    /// Encrypt a string using the VistA cipher pad.
    /// Copied verbatim from VistaQuery.Parameter.encrypt() in
    /// care-platform-core-vista-webservice.
    /// </summary>
    private static string Encrypt(string inString)
    {
        const int MAXKEY = 19;
        string[] cipherPad =
        {
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
        };

        Random r = new Random();
        int associatorIndex = r.Next(MAXKEY);
        int identifierIndex = r.Next(MAXKEY);
        while (associatorIndex == identifierIndex)
        {
            identifierIndex = r.Next(MAXKEY);
        }

        string xlatedString = "";
        for (int i = 0; i < inString.Length; i++)
        {
            char inChar = inString[i];
            int pos = cipherPad[associatorIndex].IndexOf(inChar);
            if (pos == -1)
            {
                xlatedString += inChar;
            }
            else
            {
                xlatedString += cipherPad[identifierIndex][pos];
            }
        }

        return (char)(associatorIndex + 32) +
            xlatedString +
            (char)(identifierIndex + 32);
    }
}
