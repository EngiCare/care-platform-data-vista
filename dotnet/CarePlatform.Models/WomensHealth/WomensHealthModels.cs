// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
namespace CarePlatform.Models.WomensHealth;

/// <summary>
/// Women's Health patient data — maps to oWVPatient.pas / iWVInterface.pas.
/// Tracks pregnancy, lactation, and conception status for medication safety.
/// </summary>
public class WomensHealthStatus
{
    public string Dfn { get; set; } = "";
    public PregnancyStatus PregnancyStatus { get; set; } = PregnancyStatus.Unknown;
    public LactationStatus LactationStatus { get; set; } = LactationStatus.Unknown;
    public AbleToConceive AbleToConceive { get; set; } = AbleToConceive.Unknown;
    public bool Hysterectomy { get; set; }
    public bool Menopause { get; set; }
    public DateTime? LastMenstrualPeriod { get; set; }
    public bool AskForPregnancyData { get; set; }
    public bool AskForLactationData { get; set; }
}

/// <summary>
/// Request to update pregnancy/lactation status.
/// Maps to fWVPregLacStatusUpdate.pas.
/// </summary>
public class PregLacUpdateRequest
{
    public string Dfn { get; set; } = "";
    public string PregnancyStatus { get; set; } = "";   // Unknown, Yes, No, Unsure
    public string LactationStatus { get; set; } = "";    // Unknown, Yes, No
    public string AbleToConceive { get; set; } = "";     // Unknown, Yes, No
    public DateTime? LastMenstrualPeriod { get; set; }
}

/// <summary>
/// Women's Health entered-in-error record.
/// Maps to fWVEIEReasonsDlg.pas.
/// </summary>
public class WomensHealthEieRequest
{
    public string ItemId { get; set; } = "";
    public string Reason { get; set; } = "";
}

/// <summary>
/// Women's Health external website link.
/// Maps to oWVWebSite.pas / IWVWebSite interface.
/// </summary>
public class WomensHealthWebsite
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
}

// ── Enumerations from iWVInterface.pas ──

public enum PregnancyStatus
{
    Unknown = 0,
    Yes = 1,
    No = 2,
    Unsure = 3
}

public enum LactationStatus
{
    Unknown = 0,
    Yes = 1,
    No = 2
}

public enum AbleToConceive
{
    Unknown = 0,
    Yes = 1,
    No = 2
}
