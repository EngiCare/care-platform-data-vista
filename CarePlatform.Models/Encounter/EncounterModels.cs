namespace CarePlatform.Models.Encounter;

/// <summary>
/// Encounter/visit data for PCE.
/// </summary>
public class EncounterData
{
    public string VisitString { get; set; } = "";
    public string LocationIen { get; set; } = "";
    public string LocationName { get; set; } = "";
    public DateTime? DateTime { get; set; }
    public string ProviderIen { get; set; } = "";
    public string ProviderName { get; set; } = "";
    public VisitTypeData VisitType { get; set; } = new();
    public List<DiagnosisEntry> Diagnoses { get; set; } = [];
    public List<ProcedureEntry> Procedures { get; set; } = [];
    public List<ImmunizationEntry> Immunizations { get; set; } = [];
    public List<SkinTestEntry> SkinTests { get; set; } = [];
    public List<HealthFactorEntry> HealthFactors { get; set; } = [];
    public List<ExamEntry> Exams { get; set; } = [];
    public List<PatientEdEntry> PatientEducation { get; set; } = [];

    // ── From uCore.pas TEncounter ──
    public bool IsInpatient { get; set; }
    public bool IsStandAlone { get; set; }
    public bool HasChanged { get; set; }
    public string VisitCategory { get; set; } = "";    // A=ambulatory, H=hospitalization, E=event
    public int IcdVersion { get; set; }                 // 9 or 10 (ICD version in use)
    public string NoteIen { get; set; } = "";           // associated note IEN
}

public class VisitTypeData
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
}

public class DiagnosisEntry
{
    public string Code { get; set; } = "";
    public string Narrative { get; set; } = "";
    public bool IsPrimary { get; set; }
    public string AddModDate { get; set; } = "";
}

public class ProcedureEntry
{
    public string Code { get; set; } = "";
    public string Narrative { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public string Provider { get; set; } = "";
    public List<string> Modifiers { get; set; } = [];
}

public class ImmunizationEntry
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Series { get; set; } = "";
    public string Reaction { get; set; } = "";
    public string ContraIndicatedReason { get; set; } = "";
}

public class SkinTestEntry
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Reading { get; set; } = "";
    public string Result { get; set; } = "";
    public DateTime? DateRead { get; set; }
}

public class HealthFactorEntry
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Level { get; set; } = "";
    public string Comment { get; set; } = "";
}

public class ExamEntry
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Result { get; set; } = "";
    public string Comment { get; set; } = "";
}

public class PatientEdEntry
{
    public string Code { get; set; } = "";
    public string Topic { get; set; } = "";
    public string Level { get; set; } = "";
    public string Comment { get; set; } = "";
}
