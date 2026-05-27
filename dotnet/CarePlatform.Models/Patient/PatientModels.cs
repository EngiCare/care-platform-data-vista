// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Models.Common;

namespace CarePlatform.Models.Patient;

/// <summary>
/// Patient demographics from ORWPT SELECT.
/// Piece: NAME^SEX^DOB^SSN^LOCIEN^LOCNAME^ROOMBED^CWAD^SENSITIVE^ADMITTIME^CONVERTED^SVCONN^SC%^ICN^Age^TreatSpec^SpecialtySvc
/// </summary>
public class PatientDemographics
{
    public string Dfn { get; set; } = "";
    public string Name { get; set; } = "";
    public string Sex { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public string SSN { get; set; } = "";
    public string LocationIen { get; set; } = "";
    public string LocationName { get; set; } = "";
    public string RoomBed { get; set; } = "";
    public string CWAD { get; set; } = "";
    public bool IsSensitive { get; set; }
    public DateTime? AdmitTime { get; set; }
    public string ICN { get; set; } = "";
    public int Age { get; set; }
    public string TreatingSpecialty { get; set; } = "";
    public string ServiceConnected { get; set; } = "";
    public int ServiceConnectedPercent { get; set; }

    // ── Expanded Demographics (maps to ORWPT ENCOVER, DG CHK PAT/DIV) ──
    public string NokName { get; set; } = "";
    public string NokPhone { get; set; } = "";
    public string EmergencyContactName { get; set; } = "";
    public string EmergencyContactPhone { get; set; } = "";
    public string InsuranceInfo { get; set; } = "";
    public string MaritalStatus { get; set; } = "";
    public string Religion { get; set; } = "";
    public string Ethnicity { get; set; } = "";
    public string EmploymentStatus { get; set; } = "";
    public string EligibilityCode { get; set; } = "";
    public string MeansTest { get; set; } = "";
    public string PrimaryTeam { get; set; } = "";
    public string PrimaryProvider { get; set; } = "";
    public string AttendingPhysician { get; set; } = "";

    // ── From uCore.pas TPatient ──
    public DateTime? DateDied { get; set; }
    public string WardService { get; set; } = "";
    public string InpatientProvider { get; set; } = "";
    public string MentalHealthTreatmentCoord { get; set; } = "";
    public bool IsRestricted { get; set; }
    public CombatVetInfo? CombatVet { get; set; }

    public static PatientDemographics Parse(string dfn, string vistaString)
    {
        var p = VistaStringParser.Split(vistaString);
        return new PatientDemographics
        {
            Dfn = dfn,
            Name = p.Length > 0 ? p[0] : "",
            Sex = p.Length > 1 ? p[1] : "",
            DateOfBirth = p.Length > 2 ? VistaStringParser.ParseFmDateTime(p[2]) : null,
            SSN = p.Length > 3 ? p[3] : "",
            LocationIen = p.Length > 4 ? p[4] : "",
            LocationName = p.Length > 5 ? p[5] : "",
            RoomBed = p.Length > 6 ? p[6] : "",
            CWAD = p.Length > 7 ? p[7] : "",
            IsSensitive = p.Length > 8 && VistaStringParser.ParseBool(p[8]),
            AdmitTime = p.Length > 9 ? VistaStringParser.ParseFmDateTime(p[9]) : null,
            ICN = p.Length > 13 ? p[13] : "",
            Age = p.Length > 14 ? VistaStringParser.ParseInt(p[14]) : 0,
            TreatingSpecialty = p.Length > 15 ? p[15] : "",
            ServiceConnected = p.Length > 11 ? p[11] : "",
            ServiceConnectedPercent = p.Length > 12 ? VistaStringParser.ParseInt(p[12]) : 0
        };
    }
}

/// <summary>
/// Primary care team info from ORWPT1 PRCARE.
/// Piece: PrimaryTeam^PrimaryProvider^Attending^Associate^MHTC^InProvider
/// </summary>
public class PatientPrimaryCare
{
    public string PrimaryTeam { get; set; } = "";
    public string PrimaryProvider { get; set; } = "";
    public string Attending { get; set; } = "";
    public string Associate { get; set; } = "";
    public string MentalHealthTreatmentCoordinator { get; set; } = "";
    public string InpatientProvider { get; set; } = "";

    public static PatientPrimaryCare Parse(string vistaString)
    {
        var p = VistaStringParser.Split(vistaString);
        return new PatientPrimaryCare
        {
            PrimaryTeam = p.Length > 0 ? p[0] : "",
            PrimaryProvider = p.Length > 1 ? p[1] : "",
            Attending = p.Length > 2 ? p[2] : "",
            Associate = p.Length > 3 ? p[3] : "",
            MentalHealthTreatmentCoordinator = p.Length > 4 ? p[4] : "",
            InpatientProvider = p.Length > 5 ? p[5] : ""
        };
    }
}

/// <summary>
/// Patient search result item from ORWPT LAST5, FULLSSN, BYWARD.
/// Line format: DFN^PatientName^...
/// </summary>
public class PatientSearchResult
{
    public string Dfn { get; set; } = "";
    public string Name { get; set; } = "";
    public string SSN { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public string RoomBed { get; set; } = "";
    public string AppointmentFmDateTime { get; set; } = "";

    public static PatientSearchResult Parse(string vistaLine)
    {
        var p = VistaStringParser.Split(vistaLine);
        return new PatientSearchResult
        {
            Dfn = p.Length > 0 ? p[0] : "",
            Name = p.Length > 1 ? p[1] : "",
            SSN = p.Length > 3 ? p[3] : "",
            DateOfBirth = p.Length > 2 ? VistaStringParser.ParseFmDateTime(p[2]) : null,
            RoomBed = p.Length > 4 ? p[4] : ""
        };
    }

    public static List<PatientSearchResult> ParseList(List<string> vistaLines)
    {
        return vistaLines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(Parse)
            .Where(r => !string.IsNullOrEmpty(r.Dfn))
            .ToList();
    }

    /// <summary>
    /// Parse clinic patient line from ORQPT CLINIC PATIENTS.
    /// Format: DFN^Name^Location^ApptFMDateTime
    /// </summary>
    public static PatientSearchResult ParseClinic(string vistaLine)
    {
        var p = VistaStringParser.Split(vistaLine);
        return new PatientSearchResult
        {
            Dfn = p.Length > 0 ? p[0] : "",
            Name = p.Length > 1 ? p[1] : "",
            RoomBed = p.Length > 2 ? p[2] : "",
            AppointmentFmDateTime = p.Length > 3 ? p[3] : ""
        };
    }

    public static List<PatientSearchResult> ParseClinicList(List<string> vistaLines)
    {
        return vistaLines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(ParseClinic)
            .Where(r => !string.IsNullOrEmpty(r.Dfn))
            .ToList();
    }
}

/// <summary>
/// Sensitive record access check result from DG SENSITIVE RECORD ACCESS.
/// Line 0 status: 0=no action, 1=warn, 2=warn+log, 3=no access, -1=error.
/// </summary>
public class SensitiveRecordResult
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public bool RequiresWarning => Status is 1 or 2;
    public bool AccessDenied => Status == 3;
    public bool HasError => Status == -1;

    public static SensitiveRecordResult Parse(List<string> vistaLines)
    {
        var result = new SensitiveRecordResult();
        if (vistaLines.Count > 0)
            result.Status = VistaStringParser.ParseInt(vistaLines[0]);
        if (vistaLines.Count > 1)
            result.Message = string.Join("\n", vistaLines.Skip(1));
        return result;
    }
}

/// <summary>
/// Patient ID info from ORWPT ID INFO.
/// Piece: SSN^DOB^SEX^VET^SC%^WARD^RM-BED^NAME
/// </summary>
public class PatientIdInfo
{
    public string SSN { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public string Sex { get; set; } = "";
    public bool IsVeteran { get; set; }
    public int ServiceConnectedPercent { get; set; }
    public string Ward { get; set; } = "";
    public string RoomBed { get; set; } = "";
    public string Name { get; set; } = "";

    public static PatientIdInfo Parse(string vistaString)
    {
        var p = VistaStringParser.Split(vistaString);
        return new PatientIdInfo
        {
            SSN = p.Length > 0 ? p[0] : "",
            DateOfBirth = p.Length > 1 ? VistaStringParser.ParseFmDateTime(p[1]) : null,
            Sex = p.Length > 2 ? p[2] : "",
            IsVeteran = p.Length > 3 && VistaStringParser.ParseBool(p[3]),
            ServiceConnectedPercent = p.Length > 4 ? VistaStringParser.ParseInt(p[4]) : 0,
            Ward = p.Length > 5 ? p[5] : "",
            RoomBed = p.Length > 6 ? p[6] : "",
            Name = p.Length > 7 ? p[7] : ""
        };
    }
}

/// <summary>
/// Combat veteran status from uCore.pas TCombatVet.
/// </summary>
public class CombatVetInfo
{
    public bool IsActive { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string EligibilityStatus { get; set; } = "";
}
