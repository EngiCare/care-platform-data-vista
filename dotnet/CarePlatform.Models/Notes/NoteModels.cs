using CarePlatform.Models.Common;

namespace CarePlatform.Models.Notes;

/// <summary>
/// TIU document (progress note) from TIU DOCUMENTS BY CONTEXT.
/// </summary>
public class NoteDocument
{
    public string Ien { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime? ReferenceDate { get; set; }
    public string AuthorName { get; set; } = "";
    public long AuthorDuz { get; set; }
    public string CosignerName { get; set; } = "";
    public long CosignerDuz { get; set; }
    public string Status { get; set; } = "";         // unsigned, completed, amended, retracted
    public string LocationName { get; set; } = "";
    public string Subject { get; set; } = "";
    public bool HasAddenda { get; set; }
    public bool HasChildren { get; set; }
    public string ParentIen { get; set; } = "";
    public string VisitString { get; set; } = "";
    public string SignedBy { get; set; } = "";
    public DateTime? SignDate { get; set; }
    public DateTime? ExpectedCosignDate { get; set; }
    public string ProcedureSummaryCode { get; set; } = "";
    public List<string> TextLines { get; set; } = [];
    public List<NoteDocument> Addenda { get; set; } = [];
    public List<NoteDocument> IdChildren { get; set; } = [];

    /// <summary>
    /// Parse from a TiuDocument (which correctly parses TIU DOCUMENTS BY CONTEXT).
    /// </summary>
    public static NoteDocument FromTiu(TiuDocument tiu) => new()
    {
        Ien = tiu.Ien,
        Title = tiu.Title,
        ReferenceDate = tiu.ReferenceDate,
        AuthorName = tiu.AuthorName,
        AuthorDuz = tiu.AuthorDuz,
        LocationName = tiu.LocationName,
        Status = tiu.Status,
        Subject = tiu.Subject,
        HasAddenda = tiu.HasChildrenFlag.Contains('+'),
        HasChildren = tiu.HasChildren,
        ParentIen = tiu.ParentDocument,
        VisitString = tiu.VisitString,
        PackageRef = tiu.PackageRef
    };

    /// <summary>
    /// Parse a single caret-delimited VistA line from TIU DOCUMENTS BY CONTEXT.
    /// Uses TiuDocument for correct 15-piece field mapping per cprs/uDocTree.pas.
    /// </summary>
    public static NoteDocument Parse(string vistaLine) => FromTiu(TiuDocument.Parse(vistaLine));

    public static List<NoteDocument> ParseList(List<string> vistaLines)
    {
        return vistaLines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(Parse)
            .Where(n => !string.IsNullOrEmpty(n.Ien))
            .ToList();
    }

    // ── From uTIU.pas TEditNoteRec — package refs and CPT ──
    public string PackageRef { get; set; } = "";     // e.g., "GMR(123,456" for consults
    public string PackageIen { get; set; } = "";
    public string PackagePtr { get; set; } = "";
    public bool NeedCpt { get; set; }
    public string ClinProcSummaryCode { get; set; } = "";
    public DateTime? ClinProcDateTime { get; set; }
    public string IdParent { get; set; } = "";
    public string PrfIen { get; set; } = "";         // Patient Record Flag IEN
    public string ActionIen { get; set; } = "";
}

/// <summary>
/// Note title from TIU LONG LIST OF TITLES.
/// </summary>
public class NoteTitle
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public string PrintName { get; set; } = "";
    public string Class { get; set; } = "";
    public bool IsConsult { get; set; }
    public bool IsClinProc { get; set; }
    public bool IsPrf { get; set; }
}

/// <summary>
/// Note creation request.
/// </summary>
public class CreateNoteRequest
{
    public string Dfn { get; set; } = "";
    public int TitleIen { get; set; }
    public string VisitString { get; set; } = "";
    public long AuthorDuz { get; set; }
    public string FmDate { get; set; } = "";
    public long CosignerDuz { get; set; }
    public string Subject { get; set; } = "";
    public int ConsultIen { get; set; }
}

/// <summary>
/// Note creation response.
/// </summary>
public class CreateNoteResult
{
    public bool Success { get; set; }
    public string NoteIen { get; set; } = "";
    public string ErrorText { get; set; } = "";

    public static CreateNoteResult Parse(string vistaString)
    {
        var p = VistaStringParser.Split(vistaString);
        var ien = p.Length > 0 ? p[0] : "";
        var error = p.Length > 1 ? p[1] : "";
        return new CreateNoteResult
        {
            NoteIen = ien,
            // VistA returns IEN^0 on success; piece 2 of "0" or empty = no error
            Success = VistaStringParser.ParseLong(ien) > 0 && (string.IsNullOrEmpty(error) || error == "0" || error.StartsWith("Record created")),
            ErrorText = (error == "0" || error.StartsWith("Record created")) ? "" : error
        };
    }
}

/// <summary>
/// Note context (user preferences for note views) from ORWTIU GET TIU CONTEXT.
/// </summary>
public class NoteContext
{
    public string BeginDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public string ViewContext { get; set; } = "";
    public string Author { get; set; } = "";
    public string SortBy { get; set; } = "";
    public int MaxDocs { get; set; }
    public bool ShowSubject { get; set; }
    public string GroupBy { get; set; } = "";
    public bool TreeView { get; set; }
    public string SearchText { get; set; } = "";
}

/// <summary>
/// Note view filter for the notes tree.
/// </summary>
public class NoteViewFilter
{
    public int Context { get; set; } = 1;       // 1=signed, 2=unsigned, 3=uncosigned, 4=author, 5=date
    public string EarlyDate { get; set; } = "";
    public string LateDate { get; set; } = "";
    public long PersonId { get; set; }
    public int OccurrenceLimit { get; set; }
    public string SortSequence { get; set; } = "";
    public bool ShowAddenda { get; set; } = true;
}

/// <summary>
/// TIU personal preferences from TIU GET PERSONAL PREFERENCES.
/// Maps to uTIU.pas TTIUPrefs.
/// </summary>
public class NotePreferences
{
    public int DfltTitleIen { get; set; }
    public string DfltTitleName { get; set; } = "";
    public int DfltLocationIen { get; set; }
    public string DfltLocationName { get; set; } = "";
    public long DfltCosignerDuz { get; set; }
    public string DfltCosignerName { get; set; } = "";
    public int MaxNotes { get; set; } = 100;
    public bool SortAscending { get; set; }
    public bool AskSubject { get; set; }
    public bool AskCosigner { get; set; }
}

/// <summary>
/// Note properties for the full properties dialog — maps to fNoteProps.pas.
/// </summary>
public class NoteProperties
{
    public string NoteIen { get; set; } = "";
    public string TitleIen { get; set; } = "";
    public string TitleName { get; set; } = "";
    public string Subject { get; set; } = "";
    public string AuthorDuz { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string CosignerDuz { get; set; } = "";
    public string CosignerName { get; set; } = "";
    public bool RequireCosign { get; set; }
    public string LocationIen { get; set; } = "";
    public string LocationName { get; set; } = "";
    public string RefDate { get; set; } = "";         // ISO datetime-local format
    public string VisitString { get; set; } = "";
    public string DocClass { get; set; } = "PN";      // PN, DS, CP, CN, SR
    public string ConsultIen { get; set; } = "";
    public string Status { get; set; } = "";
}

/// <summary>
/// Adhoc health summary report definition — maps to fReportsAdhocComponent1.pas.
/// </summary>
public class AdhocReportDefinition
{
    public string Name { get; set; } = "";
    public int DateRangeDays { get; set; } = 365;
    public List<AdhocComponent> Components { get; set; } = [];
}

/// <summary>
/// Individual component in an adhoc health summary report.
/// </summary>
public class AdhocComponent
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int MaxOccurrences { get; set; } = 10;
    public int TimeLimitDays { get; set; } = 365;
    public string SortOrder { get; set; } = "R";       // R=Reverse, C=Chrono, A=Alpha
    public string DisplayFormat { get; set; } = "T";   // T=Text, B=Brief, D=Detailed
}

/// <summary>
/// Lab test group for custom test grouping — maps to fLabTestGroups.pas.
/// </summary>
public class LabTestGroup
{
    public string Name { get; set; } = "";
    public List<string> Tests { get; set; } = [];
}
