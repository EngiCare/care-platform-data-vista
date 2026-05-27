namespace CarePlatform.Models.Reminders;

/// <summary>
/// Lifecycle status for a TIU note. Mirrors the cprs/uTIU.pas TX_STS constants
/// used by the desktop CPRS authoring workflow:
///   UNSIGNED       — note is being authored (also serves as "in progress")
///   COMPLETED      — author has signed; note is final
///   AMENDED        — addendum chain or amendment present
///   RETRACTED      — administratively retracted
/// </summary>
public enum NoteLifecycleStatus
{
    Unknown = 0,
    Unsigned,
    Completed,
    Amended,
    Retracted
}

/// <summary>
/// Component taxonomy for a reminder-dialog element. Mirrors the desktop
/// uDlgComponents.pas component types used by fReminderDialog.pas.
/// </summary>
public enum ReminderPromptType
{
    Static = 0,
    Radio,
    Check,
    FreeText,
    Date,
    Numeric,
    Combo,
    MhTest,
    Group
}

/// <summary>
/// Strongly-typed structured reminder dialog returned by
/// <c>PXRM DIALOG GET DEF</c>. The web client renders one element per entry
/// using <see cref="PromptType"/> to pick the input control.
/// </summary>
public class ReminderDialogDefinition
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public string ReminderIen { get; set; } = "";
    public List<ReminderDialogElementDefinition> Elements { get; set; } = [];
}

/// <summary>
/// One element in a structured reminder dialog. <see cref="FindingType"/> uses
/// the PCE category codes (HF, DX, PRC, IMM, SK, PED, EX) consumed by
/// <c>ORWPCE SAVE</c> and <c>PXRMRPCG GENFUPD</c>.
/// </summary>
public class ReminderDialogElementDefinition
{
    public string Ien { get; set; } = "";
    public int Sequence { get; set; }
    public string PromptType { get; set; } = "Static";
    public string Caption { get; set; } = "";
    public bool Required { get; set; }
    public string Boilerplate { get; set; } = "";
    public string FindingType { get; set; } = "";
    public string FindingItem { get; set; } = "";
    public int? MaxLen { get; set; }
    public int? Min { get; set; }
    public int? Max { get; set; }
    /// <summary>
    /// Explicit desktop element type code (D/C/H/T or blank).
    /// </summary>
    public string ElemTypeCode { get; set; } = "";
    /// <summary>
    /// Mirrors cprs/uReminders.pas <c>Historical</c> (piece 8).
    /// </summary>
    public bool Historical { get; set; }
    /// <summary>
    /// When true, render as a section heading instead of an interactive control.
    /// </summary>
    public bool IsHeader { get; set; }
    /// <summary>
    /// Optional logical section identifier ("A", "B", ...) for grouping.
    /// </summary>
    public string Section { get; set; } = "";
    /// <summary>Type-3 finding rows attached to this element.</summary>
    public List<ReminderFindingDefinition> Findings { get; set; } = [];
    /// <summary>Type-4 prompt rows attached to this element.</summary>
    public List<ReminderPromptDefinition> Prompts { get; set; } = [];
    /// <summary>
    /// When true, child elements are removed from the layout (not just
    /// dimmed) while disabled. Mirrors cprs/uReminders.pas
    /// <c>HideChildren = (Piece(FRec1, U, 15) &lt;&gt; '0')</c>.
    /// </summary>
    public bool HideWhenDisabled { get; set; }
    /// <summary>
    /// Desktop-equivalent <c>ChildrenRequired</c> flag (piece 18 of the
    /// reminder-dialog element row): <c>""</c>/<c>"0"</c> none, <c>"1"</c>
    /// crOne (radio-style, exactly one), <c>"2"</c> crAtLeastOne, <c>"3"</c>
    /// crNoneOrOne (radio-style optional), <c>"4"</c> crAll.
    /// </summary>
    public string ChildrenRequired { get; set; } = "";
    /// <summary>
    /// Desktop-equivalent <c>ChildrenSharePrompts</c> flag (piece 17): when
    /// true, immediate children share one prompt control set instead of
    /// rendering their own.
    /// </summary>
    public bool ChildrenSharePrompts { get; set; }
    public List<ReminderDialogChoice> Choices { get; set; } = [];
    /// <summary>
    /// Nested elements that activate when this Check element is selected.
    /// Mirrors cprs/uReminders.pas <c>EnableChildren</c> walk.
    /// </summary>
    public List<ReminderDialogElementDefinition> Children { get; set; } = [];
}

public class ReminderDialogChoice
{
    public string Code { get; set; } = "";
    public string Label { get; set; } = "";
    public string Boilerplate { get; set; } = "";
    public string FindingType { get; set; } = "";
    public string FindingItem { get; set; } = "";
    /// <summary>Per-choice <c>HideChildren</c> flag (uReminders.pas).</summary>
    public bool HideWhenDisabled { get; set; }
    /// <summary>
    /// Per-choice nested elements composed only when this option is
    /// selected. Mirrors <c>SetChecked</c> + <c>ChildrenRequired = crOne</c>.
    /// </summary>
    public List<ReminderDialogElementDefinition> Children { get; set; } = [];
}

/// <summary>
/// Type-3 finding row attached to a reminder-dialog element. Mirrors
/// cprs/uReminders.pas <c>FFindings</c>.
/// </summary>
public class ReminderFindingDefinition
{
    /// <summary>Data type code: VIT/WHR/WH/MH/Q/GFIND/HF/DX/PRC/IMM/SK/PED/EX.</summary>
    public string DataTypeCode { get; set; } = "";
    public string FindingCode { get; set; } = "";
    public string Narrative { get; set; } = "";
    public string Category { get; set; } = "";
    public string OrderSubtype { get; set; } = "";
    public string Code2 { get; set; } = "";
}

/// <summary>
/// Type-4 prompt row attached to a reminder-dialog element. Mirrors
/// cprs/uReminders.pas <c>TRemPrompt</c>.
/// </summary>
public class ReminderPromptDefinition
{
    /// <summary>Prompt code (COM, DATE, VST_LOC, ... per RemPromptCodes).</summary>
    public string Code { get; set; } = "";
    public string Caption { get; set; } = "";
    public bool Required { get; set; }
    public string Default { get; set; } = "";
    /// <summary>Auto-filled and read-only when true (TRemPrompt.Forced).</summary>
    public bool Forced { get; set; }
    /// <summary>F=future-only, H=past/today-only, empty=no constraint.</summary>
    public string Validate { get; set; } = "";
    public int Sequence { get; set; }
}

/// <summary>
/// User responses to a reminder dialog. Each entry is keyed by
/// <see cref="ReminderDialogElementDefinition.Ien"/> with the selected value
/// (radio/combo: choice code; check: "1"/"0"; free text/date/numeric: literal).
/// </summary>
public class ReminderDialogResponse
{
    public string DialogIen { get; set; } = "";
    public string NoteIen { get; set; } = "";
    public string EncounterIen { get; set; } = "";
    public Dictionary<string, string> Responses { get; set; } = new();
}

/// <summary>
/// Outcome of <c>PXRM DIALOG PROCESS</c>: composed boilerplate text plus the
/// PCE findings that were written to the encounter store.
/// </summary>
public class ReminderDialogProcessResult
{
    public string GeneratedText { get; set; } = "";
    public List<PceFindingWrite> PceWritten { get; set; } = [];
}

public class PceFindingWrite
{
    public string Type { get; set; } = "";
    public string Item { get; set; } = "";
    public string Label { get; set; } = "";
}
