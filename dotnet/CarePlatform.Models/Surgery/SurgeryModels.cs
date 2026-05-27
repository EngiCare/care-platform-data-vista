namespace CarePlatform.Models.Surgery;

/// <summary>
/// Surgery case record from ORWSR SHOW SURG TAB / ORWSR LIST.
/// Maps to rSurgery.pas TOrder-like fields.
/// </summary>
public class SurgeryCase
{
    public string Ien { get; set; } = "";
    public string Procedure { get; set; } = "";
    public DateTime? DateTime { get; set; }
    public string Surgeon { get; set; } = "";
    public string Status { get; set; } = "";
    public string Location { get; set; } = "";
    public bool IsNonORProcedure { get; set; }
    public string ParentIen { get; set; } = "";
    public string CosignerName { get; set; } = "";
    public bool RequiresCosign { get; set; }
    public List<string> ReportText { get; set; } = [];
    public List<SurgeryAddendum> Addenda { get; set; } = [];
}

/// <summary>
/// Addendum attached to a surgery report.
/// </summary>
public class SurgeryAddendum
{
    public string Ien { get; set; } = "";
    public string Author { get; set; } = "";
    public DateTime? DateTime { get; set; }
    public string Status { get; set; } = "";
    public List<string> Text { get; set; } = [];
}

/// <summary>
/// Surgery view context — maps to rSurgery.pas TSurgCaseContext.
/// Persisted per-user via ORWSR SAVE SURG CONTEXT / ORWSR GET SURG CONTEXT.
/// </summary>
public class SurgeryViewContext
{
    public string BeginDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public string Status { get; set; } = "";
    public string GroupBy { get; set; } = "";
    public int MaxDocs { get; set; }
    public bool TreeAscending { get; set; } = true;
}

/// <summary>
/// Surgery view filter used for querying.
/// </summary>
public class SurgeryViewFilter
{
    public string Context { get; set; } = "";
    public string BeginDate { get; set; } = "";
    public string EndDate { get; set; } = "";
}

/// <summary>
/// Additional signer for a surgery report.
/// Maps to fSurgery.pas mnuActIdentifyAddlSignersClick → TSignerList.
/// </summary>
public class SurgerySigner
{
    public string Duz { get; set; } = "";
    public string Name { get; set; } = "";
    public string SignerType { get; set; } = ""; // "Expected Cosigner" or "Additional Signer"
    public bool HasSigned { get; set; }
}

/// <summary>
/// PCE encounter data displayed alongside a surgery report.
/// Maps to fSurgery.pas DisplayPCE → memPCEShow.
/// </summary>
public class SurgeryEncounter
{
    public string VisitString { get; set; } = "";
    public string Location { get; set; } = "";
    public DateTime? DateTime { get; set; }
    public string Category { get; set; } = "";
    public List<string> Diagnoses { get; set; } = [];
    public List<string> Procedures { get; set; } = [];
    public List<string> Vitals { get; set; } = [];
}

/// <summary>
/// Properties for a surgery report — maps to fNoteProps.pas.
/// </summary>
public class SurgeryReportProperties
{
    public string Ien { get; set; } = "";
    public string Title { get; set; } = "";
    public string TitleIen { get; set; } = "";
    public string Author { get; set; } = "";
    public string AuthorDuz { get; set; } = "";
    public string Cosigner { get; set; } = "";
    public string CosignerDuz { get; set; } = "";
    public bool RequireCosign { get; set; }
    public string Subject { get; set; } = "";
    public string RefDate { get; set; } = "";
    public string Location { get; set; } = "";
    public string LocationIen { get; set; } = "";
    public string CaseIen { get; set; } = "";
    public bool IsNonOR { get; set; }
}
