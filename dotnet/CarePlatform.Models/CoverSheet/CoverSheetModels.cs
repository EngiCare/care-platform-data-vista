using CarePlatform.Models.Common;

namespace CarePlatform.Models.CoverSheet;

/// <summary>
/// A single allergy item from ORQQAL LIST.
/// </summary>
public class AllergyItem
{
    public string Ien { get; set; } = "";
    public string AllergenReactant { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Symptoms { get; set; } = "";
    public string Originator { get; set; } = "";
    public DateTime? OriginationDate { get; set; }

    // ── Expanded from fODAllgy.pas / ORQQAL DETAIL ──
    public string AllergyType { get; set; } = "";           // D=Drug, F=Food, O=Other
    public string Mechanism { get; set; } = "";              // A=Allergy, P=Pharmacologic, U=Unknown
    public string ObservedHistorical { get; set; } = "";     // o=Observed, h=Historical
    public DateTime? ObservedDate { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public string VerifiedBy { get; set; } = "";
    public string Comments { get; set; } = "";
    public string DrugClass { get; set; } = "";
    public string DrugIngredient { get; set; } = "";
    public List<string> SymptomList { get; set; } = [];
    public List<string> ReactionDates { get; set; } = [];

    public static AllergyItem Parse(string vistaLine)
    {
        var p = VistaStringParser.Split(vistaLine);
        return new AllergyItem
        {
            Ien = p.Length > 0 ? p[0] : "",
            AllergenReactant = p.Length > 1 ? p[1] : "",
            Severity = p.Length > 2 ? p[2] : "",
            Symptoms = p.Length > 3 ? p[3] : "",
            Originator = p.Length > 4 ? p[4] : "",
            OriginationDate = p.Length > 5 ? VistaStringParser.ParseFmDateTime(p[5]) : null
        };
    }

    public static List<AllergyItem> ParseList(List<string> vistaLines)
    {
        return vistaLines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(Parse)
            .ToList();
    }
}

/// <summary>
/// Allergy entry request for the allergy order dialog (fODAllgy.pas).
/// </summary>
public class AllergyEntryRequest
{
    public string Dfn { get; set; } = "";
    public string AllergenName { get; set; } = "";
    public string AllergenIen { get; set; } = "";
    public string AllergyType { get; set; } = "";       // D=Drug, F=Food, O=Other
    public string Mechanism { get; set; } = "";          // A=Allergy, P=Pharmacologic, U=Unknown
    public string ObservedHistorical { get; set; } = ""; // o=Observed, h=Historical
    public DateTime? ObservedDate { get; set; }
    public string Severity { get; set; } = "";
    public List<string> Symptoms { get; set; } = [];
    public string Comments { get; set; } = "";
    public long OriginatorDuz { get; set; }
}

/// <summary>
/// A single posting/CWAD item from ORQQPP LIST.
/// </summary>
public class PostingItem
{
    public string Ien { get; set; } = "";
    public string Type { get; set; } = "";  // C=Crisis, W=Warning, A=Allergy, D=Directive
    public string Text { get; set; } = "";
    public DateTime? Date { get; set; }

    public string TypeLabel => Type switch
    {
        "C" => "Crisis Note",
        "W" => "Clinical Warning",
        "A" => "Allergy/ADR",
        "D" => "Advance Directive",
        _ => Type
    };

    public static PostingItem Parse(string vistaLine)
    {
        var p = VistaStringParser.Split(vistaLine);
        return new PostingItem
        {
            Ien = p.Length > 0 ? p[0] : "",
            Type = p.Length > 1 ? p[1] : "",
            Text = p.Length > 2 ? p[2] : "",
            Date = p.Length > 3 ? VistaStringParser.ParseFmDateTime(p[3]) : null
        };
    }

    public static List<PostingItem> ParseList(List<string> vistaLines)
    {
        return vistaLines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(Parse)
            .ToList();
    }
}

/// <summary>
/// Cover sheet panel configuration parsed from ORWCV1 COVERSHEET LIST.
/// Maps to Pascal TCoverSheetParam_CPRS in oCoverSheetParam_CPRS.pas.
/// Each line from VistA is a caret-delimited string with pieces 1-19.
/// </summary>
public class CoverSheetPanelConfig
{
    public int Id { get; set; }                 // Piece 1: panel ID (10=Problems, 20=Allergies, etc.)
    public string Title { get; set; } = "";     // Piece 2: display title
    public string Status { get; set; } = "";    // Piece 3
    public string MainRpc { get; set; } = "";   // Piece 6: RPC to call for data
    public bool TitleCase { get; set; }         // Piece 7: "1" = true
    public bool Invert { get; set; }            // Piece 8: "1" = reverse sort results
    public string HighlightColor { get; set; } = ""; // Piece 9
    public string DateFormat { get; set; } = "";     // Piece 10: "D"=date, "T"=datetime
    public int DatePiece { get; set; }          // Piece 11: which result piece has the date
    public string Param1 { get; set; } = "";    // Piece 12: extra param after DFN
    public string DetailRpc { get; set; } = ""; // Piece 16: detail drill-down RPC
    public string PollingId { get; set; } = ""; // Piece 19: for ORWCV POLL background updates

    public static CoverSheetPanelConfig Parse(string vistaLine)
    {
        var p = vistaLine.Split('^');
        return new CoverSheetPanelConfig
        {
            Id = p.Length > 0 && int.TryParse(p[0], out var id) ? id : 0,
            Title = p.Length > 1 ? p[1] : "",
            Status = p.Length > 2 ? p[2] : "",
            MainRpc = p.Length > 5 ? p[5] : "",
            TitleCase = p.Length > 6 && p[6] == "1",
            Invert = p.Length > 7 && p[7] == "1",
            HighlightColor = p.Length > 8 ? p[8] : "",
            DateFormat = p.Length > 9 ? p[9] : "",
            DatePiece = p.Length > 10 && int.TryParse(p[10], out var dp) ? dp : 0,
            Param1 = p.Length > 11 ? p[11] : "",
            DetailRpc = p.Length > 15 ? p[15] : "",
            PollingId = p.Length > 18 ? p[18] : ""
        };
    }

    public static List<CoverSheetPanelConfig> ParseList(List<string> vistaLines)
    {
        return vistaLines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(Parse)
            .Where(c => c.Id > 0)
            .OrderBy(c => c.Id)
            .ToList();
    }
}

/// <summary>
/// Complete cover sheet data model — assembled from multiple API calls.
/// </summary>
public class CoverSheetData
{
    public List<CoverSheetPanelConfig> PanelConfigs { get; set; } = [];
    public List<AllergyItem> Allergies { get; set; } = [];
    public List<PostingItem> Postings { get; set; } = [];
    public List<ProblemListItem> ActiveProblems { get; set; } = [];
    public List<MedicationListItem> ActiveMedications { get; set; } = [];
    public List<VitalSignItem> RecentVitals { get; set; } = [];
    public List<LabResultItem> RecentLabs { get; set; } = [];
    public List<AppointmentItem> Appointments { get; set; } = [];
    public List<ReminderItem> Reminders { get; set; } = [];
    public List<ImmunizationItem> Immunizations { get; set; } = [];
    public List<WomensHealthItem> WomensHealth { get; set; } = [];
    public Dictionary<string, string> WidgetErrors { get; set; } = [];

    /// <summary>
    /// Raw data for panels with unrecognized IDs — rendered as generic caret-delimited tables.
    /// Key = panel ID, Value = raw VistA response lines.
    /// </summary>
    public Dictionary<int, List<string>> GenericPanels { get; set; } = [];
}

/// <summary>
/// Problem list item for cover sheet display.
/// </summary>
public class ProblemListItem
{
    public string Ien { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime? OnsetDate { get; set; }
    public DateTime? LastModified { get; set; }
    public string IcdCode { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Location { get; set; } = "";
    public string Provider { get; set; } = "";
}

/// <summary>
/// Medication list item — expanded from rMeds.pas medication RPCs.
/// </summary>
public class MedicationListItem
{
    public string OrderId { get; set; } = "";
    public string DrugName { get; set; } = "";
    public string Sig { get; set; } = "";
    public string Status { get; set; } = "";
    public string Type { get; set; } = "";  // IP=Inpatient, OP=Outpatient, NV=Non-VA
    public DateTime? StartDate { get; set; }
    public DateTime? StopDate { get; set; }
    public int Refills { get; set; }
    public string PharmacyText { get; set; } = "";

    // ── From rMeds.pas TMedListRec ──
    public string OrderableItemIen { get; set; } = "";
    public string DispenseDrug { get; set; } = "";
    public string Route { get; set; } = "";
    public string Schedule { get; set; } = "";
    public int DaysSupply { get; set; }
    public string Quantity { get; set; } = "";
    public DateTime? LastFillDate { get; set; }
    public string DEASchedule { get; set; } = "";   // 2-5 for controlled substances
    public bool IsNonVA { get; set; }
    public bool IsSupply { get; set; }
    public bool IsUnitDose { get; set; }
    public bool IsIV { get; set; }
    public string PharmacyIen { get; set; } = "";
    public string ProviderName { get; set; } = "";
}

/// <summary>
/// Vital sign item — expanded from GMV vitals RPCs.
/// </summary>
public class VitalSignItem
{
    public string Ien { get; set; } = "";
    public string Type { get; set; } = "";      // T, P, R, BP, Ht, Wt, Pain, POx, etc.
    public string TypeName { get; set; } = "";
    public string Value { get; set; } = "";
    public string Units { get; set; } = "";
    public DateTime? DateTime { get; set; }
    public string Qualifier { get; set; } = "";
    public bool IsAbnormal { get; set; }
    public string EnteredBy { get; set; } = "";
    public string LocationName { get; set; } = "";
    public double? Bmi { get; set; }
    public bool EnteredInError { get; set; }
    public List<string> Qualifiers { get; set; } = [];   // position, cuff size, location, etc.
}

/// <summary>
/// Lab result item — expanded from rLabs.pas lab data RPCs.
/// </summary>
public class LabResultItem
{
    public string Ien { get; set; } = "";
    public string TestName { get; set; } = "";
    public string Value { get; set; } = "";
    public string Units { get; set; } = "";
    public string ReferenceRange { get; set; } = "";
    public string Flag { get; set; } = "";  // H=High, L=Low, H*=Critical High, L*=Critical Low
    public DateTime? CollectionDate { get; set; }
    public string Specimen { get; set; } = "";
    public bool IsAbnormal => !string.IsNullOrEmpty(Flag);
    public string OrderedBy { get; set; } = "";
    public string Comment { get; set; } = "";
    public string Interpretation { get; set; } = "";
    public string LabSection { get; set; } = "";   // CH=Chemistry, HE=Hematology, MI=Microbiology, etc.
    public string AccessionNumber { get; set; } = "";
}

/// <summary>
/// Appointment/visit item.
/// </summary>
public class AppointmentItem
{
    public DateTime? DateTime { get; set; }
    public string LocationName { get; set; } = "";
    public string Status { get; set; } = "";
    public string Type { get; set; } = "";
}

/// <summary>
/// Clinical reminder item.
/// </summary>
public class ReminderItem
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";  // DUE, APPLICABLE, N/A, etc.
    public DateTime? DueDate { get; set; }
    public DateTime? LastOccurrence { get; set; }
    public string Priority { get; set; } = "";
}

/// <summary>
/// Immunization item for cover sheet display (mCoverSheetDisplayPanel_CPRS_Immunizations.pas).
/// </summary>
public class ImmunizationItem
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime? AdminDate { get; set; }
    public string Series { get; set; } = "";
    public string Reaction { get; set; } = "";
    public string Contraindicated { get; set; } = "";
    public string Facility { get; set; } = "";
}

/// <summary>
/// Women's health item for cover sheet display (mCoverSheetDisplayPanel_CPRS_WH.pas).
/// WVRPCOR COVER returns a count header line followed by data lines:
///   TypeAndIENs^Caption^Value
/// Piece 1 = type + IENs (e.g. "4;1,61,"), Piece 2 = caption, Piece 3 = value.
/// Pascal OnAddItems deletes line 0, then displays Piece(2) + " " + Piece(3).
/// </summary>
public class WomensHealthItem
{
    public string ItemId { get; set; } = "";      // Piece 1: type;IENs (used by WVRPCOR DETAIL for drill-down)
    public string Category { get; set; } = "";    // Piece 2: Caption (Pregnant:, Pap Smear:, Lactating:, etc.)
    public string Value { get; set; } = "";       // Piece 3: Value (Yes, No, Normal, Not Applicable, etc.)
}
