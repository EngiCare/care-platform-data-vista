// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Models.Common;

namespace CarePlatform.Models.Orders;

/// <summary>
/// Order record from ORWORR GET / ORWORR GETBYIFN.
/// </summary>
public class Order
{
    public string OrderId { get; set; } = "";
    public string DisplayGroup { get; set; } = "";
    public string Text { get; set; } = "";
    public string Status { get; set; } = "";
    public string StatusName { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? StopDate { get; set; }
    public DateTime? ChartReviewDate { get; set; }
    public string Provider { get; set; } = "";
    public long ProviderIen { get; set; }
    public string Nurse { get; set; } = "";
    public string Clerk { get; set; } = "";
    public string Service { get; set; } = "";
    public string Location { get; set; } = "";
    public string Signature { get; set; } = "";
    public bool IsFlagged { get; set; }
    public string FlagReason { get; set; } = "";
    public string OrderAction { get; set; } = "";
    public string DGroupName { get; set; } = "";
    public int AlertWhen { get; set; }
    public bool IsVerified { get; set; }
    public bool IsOnChart { get; set; }
    public bool IsCompleted { get; set; }
    public string EventId { get; set; } = "";
    public string EventName { get; set; } = "";
    public List<string> DetailLines { get; set; } = [];

    // ── From rOrders.pas TOrder expanded fields ──
    public string DGroup { get; set; } = "";
    public int DGroupSeq { get; set; }
    public DateTime? OrderTime { get; set; }
    public string ProviderDEA { get; set; } = "";
    public string VerNurse { get; set; } = "";
    public string VerClerk { get; set; } = "";
    public int DigSigReq { get; set; }
    public string XMLText { get; set; } = "";
    public string EditOf { get; set; } = "";
    public string ActionOn { get; set; } = "";
    public string EventPtr { get; set; } = "";
    public bool EnteredInError { get; set; }
    public bool IsOrderPendDC { get; set; }
    public bool IsDelayOrder { get; set; }
    public bool IsControlledSubstance { get; set; }

    /// <summary>
    /// Parse a single caret-delimited VistA order line.
    /// Field positions from ORWORR GET / rOrders.pas:
    /// 0=OrderId, 1=DGroup, 2=Status, 3=StatusName, 4=Text,
    /// 5=StartDate(FM), 6=StopDate(FM), 7=Provider, 8=IsFlagged
    /// </summary>
    public static Order Parse(string line)
    {
        var p = line.Split('^');
        return new Order
        {
            OrderId = p.Length > 0 ? p[0] : "",
            DisplayGroup = p.Length > 1 ? p[1] : "",
            Status = p.Length > 2 ? p[2] : "",
            StatusName = p.Length > 3 ? p[3] : "",
            Text = p.Length > 4 ? p[4] : "",
            StartDate = VistaStringParser.ParseFmDateTime(p.Length > 5 ? p[5] : ""),
            StopDate = VistaStringParser.ParseFmDateTime(p.Length > 6 ? p[6] : ""),
            Provider = p.Length > 7 ? p[7] : "",
            IsFlagged = p.Length > 8 && p[8] == "1"
        };
    }

    public static List<Order> ParseList(List<string> lines)
        => lines.Where(l => !string.IsNullOrWhiteSpace(l)).Select(Parse).ToList();

    /// <summary>
    /// Parse a single abbreviated caret-delimited VistA order line from ORWORR AGET.
    /// Field positions from rOrders.pas ConvertOrders:
    /// 0=OrderId (IFN), 1=DGroup, 2=OrderTime(FM), 3=EventPtr, 4=EventName
    /// </summary>
    public static Order ParseAbbreviated(string line)
    {
        var p = line.Split('^');
        var eventPtr = p.Length > 3 ? p[3] : "";
        return new Order
        {
            OrderId = p.Length > 0 ? p[0] : "",
            DGroup = p.Length > 1 ? p[1] : "",
            OrderTime = VistaStringParser.ParseFmDateTime(p.Length > 2 ? p[2] : ""),
            EventPtr = eventPtr,
            EventName = p.Length > 4 ? p[4] : "",
            IsDelayOrder = !string.IsNullOrEmpty(eventPtr)
        };
    }

    /// <summary>
    /// Parse the full ORWORR AGET response: header line + abbreviated order lines.
    /// Header (line 0): count^TextView^CtxtTime
    /// Lines 1–N: OrderId^DGroup^OrderTime^EventPtr^EventName
    /// Returns the parsed orders plus header metadata needed for ORWORR GET4LST.
    /// </summary>
    public static (int textView, string ctxtTime, List<Order> orders) ParseAbbreviatedList(List<string> lines)
    {
        if (lines.Count == 0)
            return (0, "", []);

        // Line 0 is the header: count^TextView^CtxtTime
        var header = lines[0].Split('^');
        var textView = VistaStringParser.ParseInt(header.Length > 1 ? header[1] : "");
        var ctxtTime = header.Length > 2 ? header[2] : "";

        var orders = lines.Skip(1)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(ParseAbbreviated)
            .ToList();

        return (textView, ctxtTime, orders);
    }

    /// <summary>
    /// Apply full order fields from an ORWORR GET4LST response.
    /// Each order block starts with ~IFN^24-piece header, followed by
    /// space-prefixed text lines, then |‐delimited XML blocks.
    /// Matches rOrders.pas SetOrderFields / RetrieveOrderFields.
    /// </summary>
    public void SetFields(string headerLine, string text, string xmlText)
    {
        var p = headerLine.Split('^');
        // Piece 1 = ~IFN — strip leading ~
        var ifn = p.Length > 0 ? p[0] : "";
        if (ifn.StartsWith('~'))
            ifn = ifn[1..];
        OrderId = ifn;
        DGroup = p.Length > 1 ? p[1] : "";
        OrderTime = VistaStringParser.ParseFmDateTime(p.Length > 2 ? p[2] : "");
        StartDate = VistaStringParser.ParseFmDateTime(p.Length > 3 ? p[3] : "");
        StopDate = VistaStringParser.ParseFmDateTime(p.Length > 4 ? p[4] : "");
        Status = p.Length > 5 ? p[5] : "";
        StatusName = NameOfStatus(Status);
        Signature = p.Length > 6 ? p[6] : "";
        VerNurse = p.Length > 7 ? p[7] : "";
        Nurse = VerNurse;
        VerClerk = p.Length > 8 ? p[8] : "";
        Clerk = VerClerk;
        ProviderIen = VistaStringParser.ParseLong(p.Length > 9 ? p[9] : "");
        Provider = p.Length > 10 ? p[10] : "";
        // p[11] = ActDA (unused)
        IsFlagged = p.Length > 12 && p[12] == "1";
        // p[13] = DCType (unused)
        ChartReviewDate = VistaStringParser.ParseFmDateTime(p.Length > 14 ? p[14] : "");
        ProviderDEA = p.Length > 15 ? p[15] : "";
        // p[16] = ProviderVA# (unused)
        DigSigReq = VistaStringParser.ParseInt(p.Length > 17 ? p[17] : "");
        // p[18] = IMO location: "LocName:LocIEN" — rOrders.pas SetOrderFields
        if (p.Length > 18 && !string.IsNullOrEmpty(p[18]))
        {
            var locParts = p[18].Split(':');
            var locName = locParts.Length > 0 ? locParts[0] : "";
            Location = locName == "0;SC(" ? "Unknown" : locName;
        }
        EnteredInError = p.Length > 19 && p[19] == "1";
        IsOrderPendDC = p.Length > 20 && p[20] == "1";
        IsDelayOrder = p.Length > 21 && p[21] == "1";
        IsControlledSubstance = p.Length > 22 && p[22] == "1";
        // p[23] = IsDetox (unused)

        Text = text;
        XMLText = xmlText;
    }

    /// <summary>
    /// Map VistA order status IEN to display name.
    /// Mirrors rOrders.pas NameOfStatus.
    /// </summary>
    public static string NameOfStatus(string code) => code switch
    {
        "0" => "error",
        "1" => "discontinued",
        "2" => "complete",
        "3" => "hold",
        "4" => "flagged",
        "5" => "pending",
        "6" => "active",
        "7" => "expired",
        "8" => "scheduled",
        "9" => "partial results",
        "10" => "delayed",
        "11" => "unreleased",
        "12" => "dc/edit",
        "13" => "cancelled",
        "14" => "lapsed",
        "15" => "renewed",
        "97" => "",
        "98" => "new",
        "99" => "no status",
        _ => ""
    };

    /// <summary>
    /// Walk a multi-record ORWORR GET4LST response and apply full fields
    /// to matching orders. Matches rOrders.pas RetrieveOrderFields algorithm:
    /// lines starting with ~ are order headers, space-prefixed lines are text,
    /// | marks the start of XML block.
    /// </summary>
    public static void ApplyFieldsFromResponse(List<Order> orders, List<string> responseLines)
    {
        var orderMap = new Dictionary<string, Order>();
        foreach (var o in orders)
        {
            if (!string.IsNullOrEmpty(o.OrderId))
                orderMap[o.OrderId] = o;
        }

        int i = 0;
        while (i < responseLines.Count)
        {
            var line = responseLines[i];
            if (!line.StartsWith('~'))
            {
                i++;
                continue;
            }

            // Extract IFN from ~IFN^...
            var headerLine = line;
            var ifn = headerLine.Split('^')[0];
            if (ifn.StartsWith('~'))
                ifn = ifn[1..];

            i++;

            // Collect text lines. Delphi rOrders.pas: Copy(s, 2, Length(s))
            // unconditionally drops the first character (line-type marker:
            // 't'=text, '>'=continuation, ' '=space, etc.).
            var textLines = new List<string>();
            while (i < responseLines.Count
                && !responseLines[i].StartsWith('~')
                && !responseLines[i].StartsWith('|'))
            {
                var tl = responseLines[i];
                if (tl.Length > 0)
                    tl = tl[1..];
                textLines.Add(tl);
                i++;
            }

            // Collect XML lines (after | delimiter)
            var xmlLines = new List<string>();
            if (i < responseLines.Count && responseLines[i] == "|")
            {
                i++; // skip the | line
                while (i < responseLines.Count
                    && !responseLines[i].StartsWith('~')
                    && !responseLines[i].StartsWith('|'))
                {
                    var xl = responseLines[i];
                    if (xl.Length > 0)
                        xl = xl[1..];
                    xmlLines.Add(xl);
                    i++;
                }
            }

            var text = string.Join("\r\n", textLines);
            var xmlText = string.Join("", xmlLines);

            if (orderMap.TryGetValue(ifn, out var order))
            {
                order.SetFields(headerLine, text, xmlText);
            }
        }
    }
}

/// <summary>
/// Order view filter definition.
/// </summary>
public class OrderViewFilter
{
    public string FilterName { get; set; } = "";
    public string FilterTs { get; set; } = "";
    public string DisplayGroup { get; set; } = "";
    public string TimeFrom { get; set; } = "";
    public string TimeThru { get; set; } = "";
    public string EventId { get; set; } = "";
}

/// <summary>
/// Display group for the write orders menu.
/// </summary>
public class OrderDisplayGroup
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public string Abbreviation { get; set; } = "";
    public string DialogLink { get; set; } = "";
}

/// <summary>
/// Order dialog definition from ORWDX DLGDEF.
/// </summary>
public class OrderDialogDef
{
    public string DialogId { get; set; } = "";
    public string DialogName { get; set; } = "";
    public string DialogType { get; set; } = "";
    public string FormId { get; set; } = "";
    public string DisplayGroup { get; set; } = "";
    public List<OrderDialogPrompt> Prompts { get; set; } = [];
}

/// <summary>
/// A single prompt in an order dialog.
/// </summary>
public class OrderDialogPrompt
{
    public string PromptId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";       // F=free text, S=set, D=date, P=pointer, etc.
    public string Default { get; set; } = "";
    public bool Required { get; set; }
    public bool Hidden { get; set; }
    public string Domain { get; set; } = "";
    public List<string> Choices { get; set; } = [];
}

/// <summary>
/// Order response (answer to a prompt in an order dialog).
/// </summary>
public class OrderResponse
{
    public string PromptId { get; set; } = "";
    public string PromptIen { get; set; } = "";
    public string Value { get; set; } = "";
    public string Text { get; set; } = "";
}

/// <summary>
/// Order check result from order checking RPCs.
/// </summary>
public class OrderCheck
{
    public string OrderId { get; set; } = "";
    public string Level { get; set; } = "";       // 1=significant, 2=moderate, 3=high
    public string Message { get; set; } = "";
    public string MonographText { get; set; } = "";

    public static OrderCheck Parse(string line)
    {
        var p = line.Split('^');
        return new OrderCheck
        {
            OrderId = p.Length > 0 ? p[0] : "",
            Level = p.Length > 1 ? p[1] : "",
            Message = p.Length > 2 ? p[2] : "",
            MonographText = p.Length > 3 ? p[3] : ""
        };
    }

    public static List<OrderCheck> ParseList(List<string> lines)
        => lines.Where(l => !string.IsNullOrWhiteSpace(l) && l.Split('^').Length >= 3).Select(Parse).ToList();
}

/// <summary>
/// Order sign request.
/// </summary>
public class OrderSignRequest
{
    public string Dfn { get; set; } = "";
    public long ProviderId { get; set; }
    public long LocationId { get; set; }
    public string ElectronicSignatureCode { get; set; } = "";
    public List<string> OrderIds { get; set; } = [];
    public string ReleaseMethod { get; set; } = "";  // R=release, V=verbal, P=phone, I=policy
}

/// <summary>
/// Order action result.
/// </summary>
public class OrderActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public List<OrderCheck> OrderChecks { get; set; } = [];
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// Order authorization context from uOrders.pas.
/// Tracks whether the user can create/verify/sign orders.
/// </summary>
public class OrderAuthorization
{
    public bool IsAuthorized { get; set; }
    public bool CanVerify { get; set; }
    public bool NoOrdering { get; set; }
    public bool DisableHold { get; set; }
    public int OrderingRole { get; set; }        // OR_NOKEY=0, OR_CLERK=1, OR_NURSE=2, OR_PHYSICIAN=3, OR_STUDENT=4
    public bool IsLocked { get; set; }
    public string LockMessage { get; set; } = "";
}

/// <summary>
/// IMO (Inpatient Medication for Outpatient) validation context.
/// </summary>
public class ImoContext
{
    public bool IsImoDialog { get; set; }
    public bool AllowAction { get; set; }
    public string TimeFrame { get; set; } = "";
    public string ValidationMessage { get; set; } = "";
}

/// <summary>
/// Delayed/release event from rOrders.pas TParentEvent.
/// Models event-delayed orders that release on admission, discharge, transfer, etc.
/// </summary>
public class OrderEvent
{
    public string EventId { get; set; } = "";
    public string EventName { get; set; } = "";
    public string EventType { get; set; } = "";     // A=Admit, D=Discharge, T=Transfer, S=Specialty, M=Manual
    public string Specialty { get; set; } = "";
    public string Division { get; set; } = "";
    public bool IsActive { get; set; }
    public List<string> OrderIds { get; set; } = [];
}

/// <summary>
/// Quick order list item from ORWDXQ GETQLST.
/// </summary>
public class QuickOrderItem
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public string DisplayGroup { get; set; } = "";
    public string DialogName { get; set; } = "";
    public int SortOrder { get; set; }
}

/// <summary>
/// Order menu item from ORWDXM MENU.
/// </summary>
public class OrderMenuItem
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public string ItemType { get; set; } = "";      // M=Menu, D=Dialog, Q=Quick Order, O=Order Set
    public string DisplayName { get; set; } = "";
    public int Column { get; set; }
    public int Row { get; set; }
    public string Mnemonic { get; set; } = "";
}

/// <summary>
/// Service-connected / copay flags for order signing.
/// Maps to fOrdersSign.pas SC/EI billing awareness flags.
/// </summary>
public class OrderCopayFlags
{
    public bool ServiceConnected { get; set; }
    public bool CombatVeteran { get; set; }
    public bool AgentOrange { get; set; }
    public bool IonizingRadiation { get; set; }
    public bool MilitarySexualTrauma { get; set; }
    public bool HeadNeckCancer { get; set; }
    public bool SWAsiaConditions { get; set; }
}
