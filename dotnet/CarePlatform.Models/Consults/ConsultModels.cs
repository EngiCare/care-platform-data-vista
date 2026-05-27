// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Models.Common;

namespace CarePlatform.Models.Consults;

/// <summary>
/// Consult record from ORQQCN LIST.
/// </summary>
public class Consult
{
    public string Ien { get; set; } = "";
    public string Status { get; set; } = "";
    public string Service { get; set; } = "";
    public string Procedure { get; set; } = "";
    public string ConsultType { get; set; } = "";      // Consult or Procedure
    public DateTime? RequestDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string Urgency { get; set; } = "";
    public string RequestingProvider { get; set; } = "";
    public string AttendingProvider { get; set; } = "";
    public string ToService { get; set; } = "";
    public string ReasonForRequest { get; set; } = "";
    public string Findings { get; set; } = "";
    public string OrderId { get; set; } = "";
    public List<string> DetailLines { get; set; } = [];
    public List<ConsultComment> Comments { get; set; } = [];

    // ── From uConsults.pas TConsultRequest ──
    public string ProvDiagnosis { get; set; } = "";
    public string ProvDxCode { get; set; } = "";
    public bool ProvDxCodeInactive { get; set; }
    public DateTime? ClinicallyIndicatedDate { get; set; }
    public string ModeOfEntry { get; set; } = "";
    public string InOut { get; set; } = "";            // I=Inpatient, O=Outpatient
    public string PlaceOfConsult { get; set; } = "";
    public string Attention { get; set; } = "";
    public string AttentionName { get; set; } = "";
    public string TiuResultNarrative { get; set; } = "";
    public List<string> TiuDocuments { get; set; } = [];
    public List<string> MedResults { get; set; } = [];
    public string RequestProcessingActivity { get; set; } = "";
    public string SendingProvider { get; set; } = "";
    public string SendingProviderName { get; set; } = "";
    public string ForeignConsultFileNum { get; set; } = "";
    public string OrderingFacility { get; set; } = "";

    // ── ORQQCN LIST 12-piece display fields (parity with cprs/Consults/uConsults.pas) ──
    /// <summary>Display date string (piece 2 of ORQQCN LIST), e.g. "Apr 13,26".</summary>
    public string DisplayDate { get; set; } = "";

    /// <summary>Lowercase status abbreviation (piece 3), e.g. "p", "x", "c", "dc", "sch".</summary>
    public string StatusAbbrev { get; set; } = "";

    /// <summary>"Consult #: nnnn" string (piece 5).</summary>
    public string ConsultNumber { get; set; } = "";

    /// <summary>Order IFN (piece 6).</summary>
    public string OrderIfn { get; set; } = "";

    /// <summary>Has-children flag (piece 7) — "+" if a grouping parent.</summary>
    public string HasChildren { get; set; } = "";

    /// <summary>Parent node IEN (piece 8) — "0" for top-level.</summary>
    public string ParentNode { get; set; } = "";

    /// <summary>"Consult" | "Procedure" | "Clinical Procedure" (piece 9).</summary>
    public string TypeText { get; set; } = "";

    /// <summary>Single-letter type code (piece 12): C=Consult, P=Procedure, M=Clinical Procedure, I=IFC Consult, R=IFC Procedure.</summary>
    public string TypeCode { get; set; } = "";

    /// <summary>Mapping of full status names to lowercase abbreviations used in piece 3 of ORQQCN LIST.</summary>
    public static readonly IReadOnlyDictionary<string, string> StatusToAbbrev = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["PENDING"] = "p",
        ["ACTIVE"] = "a",
        ["SCHEDULED"] = "sch",
        ["COMPLETE"] = "c",
        ["CANCELLED"] = "x",
        ["DISCONTINUED"] = "dc",
        ["HOLD"] = "h",
        ["FLAGGED"] = "f",
        ["EXPIRED"] = "e",
        ["PARTIAL RESULTS"] = "pr",
        ["DELAYED"] = "d",
        ["UNRELEASED"] = "u",
        ["CHANGED"] = "ch",
        ["LAPSED"] = "l",
        ["RENEWED"] = "rn"
    };

    /// <summary>Inverse map — abbreviation to canonical status name.</summary>
    public static readonly IReadOnlyDictionary<string, string> AbbrevToStatus =
        StatusToAbbrev.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);

    /// <summary>Resolve any input (full name or abbreviation) to the canonical full status name.</summary>
    public static string NormalizeStatus(string statusOrAbbrev)
    {
        if (string.IsNullOrWhiteSpace(statusOrAbbrev)) return "";
        var s = statusOrAbbrev.Trim();
        if (StatusToAbbrev.ContainsKey(s)) return s.ToUpperInvariant();
        return AbbrevToStatus.TryGetValue(s, out var full) ? full : s.ToUpperInvariant();
    }

    /// <summary>Resolve any input to the lowercase abbreviation; returns input lowercased if unknown.</summary>
    public static string NormalizeAbbrev(string statusOrAbbrev)
    {
        if (string.IsNullOrWhiteSpace(statusOrAbbrev)) return "";
        var s = statusOrAbbrev.Trim();
        if (AbbrevToStatus.ContainsKey(s)) return s.ToLowerInvariant();
        return StatusToAbbrev.TryGetValue(s, out var abbr) ? abbr : s.ToLowerInvariant();
    }

    /// <summary>
    /// Build the displayed list-row text matching MakeConsultListDisplayText in
    /// cprs/Consults/uConsults.pas (line 237):
    ///   "{DisplayDate}  ({StatusAbbrev})  {Service} {ConsultNumber}"
    /// </summary>
    public string BuildDisplayText()
    {
        var date = string.IsNullOrWhiteSpace(DisplayDate)
            ? RequestDate?.ToString("MMM dd,yy") ?? ""
            : DisplayDate.Trim();
        var abbrev = string.IsNullOrWhiteSpace(StatusAbbrev) ? NormalizeAbbrev(Status) : StatusAbbrev;
        var statusPart = string.IsNullOrWhiteSpace(abbrev) ? "" : $"({abbrev})";
        var parts = new[] { date, statusPart, Service, ConsultNumber }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join("  ", parts).Trim();
    }

    /// <summary>
    /// Parse a single caret-delimited VistA consult line.
    /// Real ORQQCN LIST returns 12 pieces (per cprs/Consults/uConsults.pas#L191):
    ///   1=Ien, 2=DisplayDate, 3=StatusAbbrev, 4=Service/Procedure text,
    ///   5=Consult #: ien, 6=OrderIfn, 7=HasChildren, 8=ParentNode,
    ///   9=TypeText (Consult|Procedure|Clinical Procedure), 10=Service Name,
    ///   11=FmDate, 12=TypeCode (C|P|M|I|R).
    /// Falls back to the legacy stub schema (Ien^Service^Procedure^Status^FmDate^Urgency^Provider)
    /// when piece 11 is empty/non-numeric, so existing fixtures keep working.
    /// </summary>
    public static Consult Parse(string line)
    {
        var p = line.Split('^');
        var pc = (int idx) => idx >= 0 && idx < p.Length ? p[idx] : "";

        // Heuristic: real VistA lines have a numeric FM date in piece 11 (index 10).
        var p11 = pc(10);
        bool isWireFormat = p.Length >= 11 && !string.IsNullOrWhiteSpace(p11)
                            && double.TryParse(p11, out _);

        if (isWireFormat)
        {
            var statusAbbrev = pc(2).Trim();
            var serviceText = pc(3);
            var consultNum = pc(4);
            var typeText = pc(8);
            var typeCode = pc(11);

            return new Consult
            {
                Ien = pc(0),
                DisplayDate = pc(1).Trim(),
                StatusAbbrev = statusAbbrev,
                Status = AbbrevToStatus.TryGetValue(statusAbbrev, out var full) ? full : "",
                Service = serviceText,
                ConsultNumber = consultNum,
                OrderIfn = pc(5),
                HasChildren = pc(6),
                ParentNode = pc(7),
                TypeText = typeText,
                ConsultType = typeText,
                Procedure = string.Equals(typeText, "Procedure", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(typeText, "Clinical Procedure", StringComparison.OrdinalIgnoreCase)
                            ? serviceText : "",
                TypeCode = typeCode,
                ToService = pc(9),
                RequestDate = VistaStringParser.ParseFmDateTime(p11)
            };
        }

        // Legacy 7-piece stub fallback
        var status = pc(3);
        return new Consult
        {
            Ien = pc(0),
            Service = pc(1),
            Procedure = pc(2),
            Status = status,
            StatusAbbrev = NormalizeAbbrev(status),
            RequestDate = VistaStringParser.ParseFmDateTime(pc(4)),
            Urgency = pc(5),
            RequestingProvider = pc(6)
        };
    }

    public static List<Consult> ParseList(List<string> lines)
        => lines.Where(l => !string.IsNullOrWhiteSpace(l)).Select(Parse).ToList();
}

/// <summary>
/// Consult comment entry.
/// </summary>
public class ConsultComment
{
    public string Text { get; set; } = "";
    public string Author { get; set; } = "";
    public DateTime? DateTime { get; set; }
    public string Action { get; set; } = "";
}

/// <summary>
/// Consult service for ordering.
/// </summary>
public class ConsultService
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public string Synonym { get; set; } = "";
    public bool HasPrerequisites { get; set; }
    public string PrerequisiteText { get; set; } = "";
}

/// <summary>
/// Consult view filter.
/// </summary>
public class ConsultViewFilter
{
    public string ViewContext { get; set; } = "all";   // all, status, service, date, custom
    public string Status { get; set; } = "";
    public string Service { get; set; } = "";
    public string BeginDate { get; set; } = "";
    public string EndDate { get; set; } = "";
}

/// <summary>
/// Medicine result entry that can be attached to a consult — maps to fConsMedRslt.pas.
/// </summary>
public class MedResultEntry
{
    public string Id { get; set; } = "";
    public string Date { get; set; } = "";
    public string Procedure { get; set; } = "";
    public string Provider { get; set; } = "";
    public string Status { get; set; } = "";
    public List<string> DetailLines { get; set; } = [];
}

/// <summary>
/// SF-513 formatted data for display/print — maps to fConsult513Prt.pas.
/// </summary>
public class SF513Data
{
    public string ConsultIen { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string SSN { get; set; } = "";
    public string DOB { get; set; } = "";
    public string Location { get; set; } = "";
    public string Service { get; set; } = "";
    public string Procedure { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string Place { get; set; } = "";
    public string FromProvider { get; set; } = "";
    public string RequestDate { get; set; } = "";
    public string Status { get; set; } = "";
    public string Attention { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string Reason { get; set; } = "";
    public string Results { get; set; } = "";
    public string Activity { get; set; } = "";
}
